using CsvHelper.Configuration.Attributes;

namespace Azure.BudgetExporter
{
    public class Budget
    {
        [Index(0)]
        public string? Id { get; set; }

        public string? Name { get; set; }

        public string? Scope { get; set; }

        public string? ScopeName { get; set; }

        public string? ScopeIdentifier { get; set; }

        public string? SubscriptionDisplayName { get; set; }

        public string? ResourceGroupDisplayName { get; set; }

        public string? ResetPeriod { get; set; }

        //[NumberStyles(NumberStyles.Currency | NumberStyles.AllowCurrencySymbol)]
        public string? Amount { get; set; }

        //[NumberStyles(NumberStyles.Currency | NumberStyles.AllowCurrencySymbol)]
        public string? EvaluatedSpend { get; set; }

        public string? ForecastSpend { get; set; }
    }
}
