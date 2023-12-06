namespace CyberImpactByCAPM.Models
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class CyberAttackDatabase
    {
        public bool success { get; set; }
        public Row[] rows { get; set; }
        public object lastRow { get; set; }
        public Args args { get; set; }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class Args
    {
        public bool _public { get; set; }
        public string start_row { get; set; }
        public string end_row { get; set; }
        public string summary { get; set; }
        public string filter { get; set; }
        public string sort { get; set; }
        public int page { get; set; }
        public int per_page { get; set; }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class Row
    {
        public string slug { get; set; }
        public string event_title { get; set; }
        public DateTime event_date { get; set; }
        public string actor { get; set; }
        public string motive { get; set; }
        public string event_type { get; set; }
        public string event_subtype { get; set; }
        public string organization { get; set; }
        public string country_code { get; set; }
        public string country_raw { get; set; }
        public string country { get; set; }
        public string industry { get; set; }
        public string actor_country { get; set; }
        public string actor_country_code { get; set; }
        public string? ticker_symbol { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
