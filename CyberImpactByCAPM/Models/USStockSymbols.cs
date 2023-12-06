namespace CyberImpactByCAPM.Models
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class USStockSymbols
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string symbol { get; set; }
        public string name { get; set; }
        public string lastsale { get; set; }
        public string netchange { get; set; }
        public string pctchange { get; set; }
        public string volume { get; set; }
        public string marketCap { get; set; }
        public string country { get; set; }
        public string ipoyear { get; set; }
        public string industry { get; set; }
        public string sector { get; set; }
        public string url { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }

}
