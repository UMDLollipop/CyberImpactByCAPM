using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberImpactByCAPM.Models
{
    public class CummulativeAbnormalReturn
    {
        [Index(0)]
        public string? Date { get; set; }
        [Index(1)]
        public string? Organization { get; set; }
        [Index(2)]
        public double? CAR { get; set; }
        [Index(3)]
        public double? CARFF3F { get; set; }
        [Index(4)]
        public string? EventType { get; set; }
        [Index(5)]
        public string? EventSubType { get; set; }
    }
}
