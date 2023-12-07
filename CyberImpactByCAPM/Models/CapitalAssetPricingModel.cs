using Fynance.Result;
using MathNet.Numerics.LinearAlgebra;

namespace CyberImpactByCAPM.Models
{
    public class CapitalAssetPricingModel
    {
        public Row? CyberEvent { get; set; }
        public FyQuote[]? ModelQuotes { get; set; }
        public FinancialStats[]? ModelFinancialStats { get; set; }
        public FyQuote[]? EventQuotes { get; set; }
        public FinancialStats[]? EventFinancialStats { get; set; }
        public double[]? ModelXValue => ModelFinancialStats!.Zip(ModelFinancialStats!, (a, b) => a.Mkt_RF - b.RF).ToArray();
        public double[]? ModelYValue => Enumerable.Range(1, ModelQuotes!.Length - 1)
            .Select(index =>
                (double)(ModelQuotes[index].AdjClose / ModelQuotes[index - 1].AdjClose - 1) - ModelFinancialStats![index].RF)
            .ToArray();
        public Vector<double>? ModelVectors { get; set; }
        public Func<double, double> CapitalAssetPricingFunction => (x) => ModelVectors![1] + ModelVectors![0] * x;
        public Func<int, double> AbnormalReturnFunction => (index) => (((double)(EventQuotes![index].AdjClose / EventQuotes![index - 1].AdjClose - 1) - EventFinancialStats![index].RF) - CapitalAssetPricingFunction(EventFinancialStats![index].Mkt_RF - EventFinancialStats![index].RF));
        public Vector<double>? ModelFamaFrench3FactorsVectors { get; set; }
        public Func<double, double, double, double> CapitalAssetPricingFF3FFunction => (x, y, z) => ModelFamaFrench3FactorsVectors![3] + ModelFamaFrench3FactorsVectors![2] * z + ModelFamaFrench3FactorsVectors![1] * y + ModelFamaFrench3FactorsVectors![0] * x;
        public Func<int, double> AbnormalReturnFF3FFunction => (index) => (((double)(EventQuotes![index].AdjClose / EventQuotes![index - 1].AdjClose - 1) - EventFinancialStats![index].RF) - CapitalAssetPricingFF3FFunction(EventFinancialStats![index].Mkt_RF - EventFinancialStats![index].RF, EventFinancialStats![index].SMB, EventFinancialStats![index].HML));
        public double? CumulativeAbnormalReturn => Enumerable.Range(1, EventQuotes!.Length - 1)
            .Select(index => AbnormalReturnFunction(index))
            .Sum();
        public double? CumulativeAbnormalReturnFF3F => Enumerable.Range(1, EventQuotes!.Length - 1)
            .Select(index => AbnormalReturnFF3FFunction(index))
            .Sum();
    }
}