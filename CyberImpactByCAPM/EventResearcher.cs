using CyberImpactByCAPM.Models;
using CsvHelper;
using Fynance;
using Fynance.Result;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Globalization;
using System.Text.Json;

namespace CyberImpactByCAPM
{
    internal class EventResearcher
    {
        private const string _dbPath = "Data\\download.json";
        private const string _symbolPath = "Data\\nasdaq_full_tickers.json";
        private const string _factorPath = "Data\\F-F_Research_Data_Factors_daily.CSV";
        private static readonly string[] _manualRemoveList = new string[] { "Voyager", "Krystal", "Apollo" };
        public static readonly DateTime CovidDeclaredDate = new(2020, 3, 1, 0, 0, 0);
        private static DateTime _researchStartDate;
        private static DateTime _researchEndDate;
        private IEnumerable<IGrouping<string, Row>>? _attacksByOrganization;
        private List<IGrouping<string, Row>>? _nasdaqOrgAtkList;
        public List<CapitalAssetPricingModel> CapitalAssetPricingModelList { get; set; } = new();
        private static int _estimationPeriods;
        private static int _eventPeriods;
        public CyberAttackDatabase? CyberAttackDatabase { get; set; }
        public List<USStockSymbols>? StockSymbolList { get; set; }
        public List<FinancialStats> FinancialStatsList { get; set; } = new();
        public EventResearcher(int estimationPeriods, int eventPeriods, int yearsBeforeDeclaredDate, int yearsAfterDeclaredDate)
        {
            _estimationPeriods = estimationPeriods;
            _eventPeriods = eventPeriods;
            _researchStartDate = CovidDeclaredDate.AddYears(-1 * yearsBeforeDeclaredDate);
            _researchEndDate = CovidDeclaredDate.AddYears(yearsAfterDeclaredDate);
        }
        public EventResearcher LoadDatabase()
        {
            CyberAttackDatabase = JsonSerializer.Deserialize<CyberAttackDatabase>(File.ReadAllText(_dbPath));
            StockSymbolList = JsonSerializer.Deserialize<List<USStockSymbols>>(File.ReadAllText(_symbolPath));
            FinancialStatsList = new CsvReader(new StreamReader(_factorPath), CultureInfo.InvariantCulture).GetRecords<FinancialStats>().ToList();
            return this;
        }
        public EventResearcher GroupAttacks()
        {
            // Group attacks by organizations
            _attacksByOrganization = CyberAttackDatabase!.rows
                    .Where(x => x.event_date >= _researchStartDate && x.event_date <= _researchEndDate)
                    .GroupBy(incident => incident.organization)
                    .Where(z => z.Count() == 1);
            return this;
        }
        public EventResearcher FilterAttacks()
        {
            // Filters and add ticker symbols
            if (CyberAttackDatabase is not null && StockSymbolList is not null)
            {
                if (_attacksByOrganization is null)
                    throw new NullReferenceException("Group attacks by organization before applying filter");
                foreach (IGrouping<string, Row> group in _attacksByOrganization)
                {
                    foreach (Row row in group)
                    {
                        if (row.country_code == "USA"
                            && row.event_type != "Mixed"
                            && row.event_type != "Undetermined"
                            && row.event_subtype is not null
                            && row.event_subtype != "Undetermined"
                            && row.industry != "Public Administration"
                            && StockSymbolList.Any(x => row.organization.Split(" ").All(element => x.name.Split(" ").Contains(element))))
                        {
                            row.ticker_symbol = StockSymbolList.First(x => row.organization.Split(" ").All(element => x.name.Split(" ").Contains(element))).symbol;
                        }
                    }
                }
            }
            return this;
        }
        public EventResearcher ApplyManualFilter()
        {
            if (_attacksByOrganization is null)
                throw new NullReferenceException("Manual filter must be applied after default filter.");
            _nasdaqOrgAtkList = _attacksByOrganization
                .Where(group => group.Any(row => row.ticker_symbol != null))
                .Where(group => group.Any(row => !_manualRemoveList.Contains(row.organization))).ToList();
            return this;
        }
        public async Task<EventResearcher> PrepareModelAsync()
        {
            if (_nasdaqOrgAtkList is null)
                throw new NullReferenceException("Model may not be prepared before manual filter is applied.");
            foreach (IGrouping<string, Row> group in _nasdaqOrgAtkList)
            {
                foreach (Row @event in group.ToList())
                {
                    try
                    {
                        var history = await Ticker.Build()
                                 .SetSymbol(@event.ticker_symbol)
                                 .SetPeriod(Period.TenYears)
                                 .SetInterval(Interval.OneDay)
                                 .SetInterval(@event.event_date.AddDays(_estimationPeriods * -2), @event.event_date.AddDays(_eventPeriods + 7))
                                 .SetSplits(true)
                                 .GetAsync();
                        if (history.Splits.Length != 0)
                        {
                            Console.WriteLine(string.Format("{0} has split during the event window", @event.ticker_symbol));
                        }
                        else
                        {
                            try
                            {
                                // The first business day the event started. If the event started when the market is closed, the next business date is used.
                                int eventStartIndex = history.Quotes.ToList().FindIndex(quotes => quotes.Period.Date >= @event.event_date.Date);
                                int modelStartIndex = eventStartIndex - _estimationPeriods - 1;
                                List<FyQuote> modelQuotes = new();
                                modelQuotes.AddRange(history.Quotes.ToList().GetRange(modelStartIndex, _estimationPeriods + 1));
                                List<FyQuote> eventQuotes = new();
                                eventQuotes.AddRange(history.Quotes.ToList().GetRange(eventStartIndex - 1, _eventPeriods + 1));
                                List<FinancialStats> modelFinStats = new();
                                foreach (FyQuote fyQuote in modelQuotes)
                                {
                                    modelFinStats.Add(FinancialStatsList.First(x => x.DateTime!.Value.Date == fyQuote.Period.Date));
                                }
                                List<FinancialStats> eventFinStats = new();
                                foreach (FyQuote fyQuote in eventQuotes)
                                {
                                    eventFinStats.Add(FinancialStatsList.First(x => x.DateTime!.Value.Date == fyQuote.Period.Date));
                                }
                                if (modelQuotes.Count != modelFinStats.Count || eventQuotes.Count != eventFinStats.Count)
                                {
                                    Console.WriteLine(string.Format("{0} has mismatch financial stats and quotes to build a model", @event.ticker_symbol));
                                }
                                else
                                {
                                    CapitalAssetPricingModelList.Add(new CapitalAssetPricingModel()
                                    {
                                        CyberEvent = @event,
                                        ModelQuotes = modelQuotes.ToArray(),
                                        ModelFinancialStats = modelFinStats.ToArray(),
                                        EventQuotes = eventQuotes.ToArray(),
                                        EventFinancialStats = eventFinStats.ToArray()
                                    });
                                }
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                Console.WriteLine(string.Format("{0} does not have enough stock quote history to build a model", @event.ticker_symbol));
                            }
                        }
                    }
                    catch (Fynance.FynanceException)
                    {
                        Console.WriteLine(string.Format("{0} has no stock quote history during the event window", @event.ticker_symbol));
                    }
                }
            }
            return this;
        }
        public EventResearcher BuildModel()
        {
            // CAPM
            foreach (var capm in CapitalAssetPricingModelList)
            {
                double[] xValues = capm.ModelXValue?[1..] ?? Array.Empty<double>();
                double[] yValues = capm.ModelYValue ?? Array.Empty<double>();
                var matrixX = DenseMatrix.OfColumnVectors(Vector<double>.Build.DenseOfArray(xValues), Vector<double>.Build.Dense(xValues.Length, 1));

                // Create a vector from the y values
                var vectorY = Vector<double>.Build.DenseOfArray(yValues);

                // Perform linear regression
                capm.ModelVectors = matrixX.QR().Solve(vectorY);
            }
            // Fama–French three-factor model
            foreach (var capm in CapitalAssetPricingModelList)
            {
                double[] x1Values = capm.ModelXValue?[1..] ?? Array.Empty<double>();
                double[] x2Values = capm.ModelFinancialStats?[1..]?.Select(x => x.SMB).ToArray() ?? Array.Empty<double>();
                double[] x3Values = capm.ModelFinancialStats?[1..]?.Select(x => x.HML).ToArray() ?? Array.Empty<double>();
                double[] yValues = capm.ModelYValue ?? Array.Empty<double>();
                var matrixX = DenseMatrix.OfColumnVectors(
                    Vector<double>.Build.DenseOfArray(x1Values),
                    Vector<double>.Build.DenseOfArray(x2Values),
                    Vector<double>.Build.DenseOfArray(x3Values),
                    Vector<double>.Build.Dense(x1Values.Length, 1)
                    );

                // Create a vector from the y values
                var vectorY = Vector<double>.Build.DenseOfArray(yValues);

                // Perform linear regression
                capm.ModelFamaFrench3FactorsVectors = matrixX.QR().Solve(vectorY);
            }
            return this;
        }
    }
}