using CsvHelper;
using CyberImpactByCAPM.Models;
using System.Globalization;

namespace CyberImpactByCAPM
{
    public class Program
    {
        private const string CARWholeResultPath = "Data\\CARWholeResultPath.csv";
        private const string CARPreResultPath = "Data\\CARPreResultPath.csv";
        private const string CARPostResultPath = "Data\\CARPostResultPath.csv";
        public static async Task Main()
        {
            EventResearcher eventResearcher = new(120, 3, 3, 3);
            await eventResearcher.LoadDatabase()
                .GroupAttacks()
                .FilterAttacks()
                .ApplyManualFilter()
                .PrepareModelAsync();
            eventResearcher.BuildModel();

            List<CapitalAssetPricingModel> CapitalAssetPricingModelPreCovidList = eventResearcher.CapitalAssetPricingModelList
                .Where(x => x.CyberEvent!.event_date.Date <= EventResearcher.CovidDeclaredDate.Date).ToList();

            List<CapitalAssetPricingModel> CapitalAssetPricingModelPostCovidList = eventResearcher.CapitalAssetPricingModelList
                .Where(x => x.CyberEvent!.event_date.Date > EventResearcher.CovidDeclaredDate.Date).ToList();

            Console.WriteLine(string.Format("Whole Sample 2017~2023, Sample Size: {0}", eventResearcher.CapitalAssetPricingModelList.Count));
            foreach (var capm in eventResearcher.CapitalAssetPricingModelList)
            {
                string stringFormat = "Organization: {0}, CAR: {1}, CAR by FF3F: {2}";
                Console.WriteLine(string.Format(stringFormat, capm.CyberEvent!.organization, capm.CumulativeAbnormalReturn, capm.CumulativeAbnormalReturnFF3F));
            }

            Console.WriteLine(string.Format("Pre Covid-19, Sample Size: {0}", CapitalAssetPricingModelPreCovidList.Count));
            foreach (var capm in CapitalAssetPricingModelPreCovidList)
            {
                string stringFormat = "Organization: {0}, CAR: {1}, CAR by FF3F: {2}";
                Console.WriteLine(string.Format(stringFormat, capm.CyberEvent!.organization, capm.CumulativeAbnormalReturn, capm.CumulativeAbnormalReturnFF3F));
            }

            Console.WriteLine(string.Format("Post Covid-19, Sample Size: {0}", CapitalAssetPricingModelPostCovidList.Count));
            foreach (var capm in CapitalAssetPricingModelPostCovidList)
            {
                string stringFormat = "Organization: {0}, CAR: {1}, CAR by FF3F: {2}";
                Console.WriteLine(string.Format(stringFormat, capm.CyberEvent!.organization, capm.CumulativeAbnormalReturn, capm.CumulativeAbnormalReturnFF3F));
            }
            WriteToCsv(eventResearcher.CapitalAssetPricingModelList, CARWholeResultPath);
            WriteToCsv(CapitalAssetPricingModelPreCovidList, CARPreResultPath);
            WriteToCsv(CapitalAssetPricingModelPostCovidList, CARPostResultPath);
        }
        public static void WriteToCsv(List<CapitalAssetPricingModel> capitalAssetPricingModelList, string path)
        {
            using var writer = new StreamWriter(path);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteHeader<CummulativeAbnormalReturn>();
            csv.NextRecord();
            foreach (var record in capitalAssetPricingModelList)
            {
                CummulativeAbnormalReturn cummulativeAbnormalReturn = new()
                {
                    Date = record.CyberEvent?.event_date.ToLongDateString(),
                    Organization = record.CyberEvent?.organization,
                    CAR = record.CumulativeAbnormalReturn,
                    CARFF3F = record.CumulativeAbnormalReturnFF3F,
                    EventType = record.CyberEvent?.event_type,
                    EventSubType = record.CyberEvent?.event_subtype

                };
                csv.WriteRecord(cummulativeAbnormalReturn);
                csv.NextRecord();
            }
        }
    }
}