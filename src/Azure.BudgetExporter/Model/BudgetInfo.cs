using Azure.ResourceManager.Consumption;

namespace Azure.BudgetExporter.Model
{
    public class BudgetInfo
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public decimal Amount { get; set; } = decimal.MinValue;

        public decimal? EvaluatedSpend { get; set; }

        public decimal? ForecastedSpend { get; set; }

        public string? ResetPeriod { get; set; }

        public BudgetInfo()
        {
            
        }

        public BudgetInfo(ConsumptionBudgetResource resource)
        {
            ArgumentNullException.ThrowIfNull(resource);

            Id = resource.Id!;
            Name = resource.Data.Name;
            Amount = resource.Data.Amount.Value;
            EvaluatedSpend = resource.Data.CurrentSpend?.Amount;
            ForecastedSpend = resource.Data.ForecastSpend?.Amount;
            ResetPeriod = resource.Data.TimeGrain.ToString();
        }
    }
}
