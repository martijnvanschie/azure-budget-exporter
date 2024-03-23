using Azure.BudgetExporter.Model;
using CsvHelper.Configuration;

namespace Azure.BudgetExporter
{
    public class BudgetMap : ClassMap<Budget>
    {
        public BudgetMap()
        {
            Map(m => m.Id);
            Map(m => m.Amount).Name("Amount").TypeConverterOption.NumberStyles(System.Globalization.NumberStyles.Currency);
        }
    }
}
