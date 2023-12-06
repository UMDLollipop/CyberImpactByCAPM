using CsvHelper.Configuration.Attributes;

namespace CyberImpactByCAPM.Models
{
    public class FinancialStats
    {
        [Index(0)]
        public string? Date { get; set; }
        [Index(1)]
        public double Mkt_RF { get; set; }
        [Index(2)]
        public double SMB { get; set; }
        [Index(3)]
        public double HML { get; set; }
        [Index(4)]
        public double RF { get; set; }
        [Ignore]
        public DateTime? DateTime => System.DateTime.ParseExact(Date!, "yyyyMMdd", null);
    }
}
