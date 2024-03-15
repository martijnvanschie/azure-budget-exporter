using Azure.BudgetExporter.Model;
using CsvHelper;
using System.Globalization;

namespace Azure.BudgetExporter
{
    internal class BudgetExporter
    {
        public void ExportToCsv(List<BudgetScopeResource> resources, string filename = "budgets.csv")
        {
            var budgets = ParseResources(resources);

            using (var writer = new StreamWriter(filename))

            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    try
                    {
                        //csv.Context.RegisterClassMap<FooMap>();
                        csv.WriteRecords(budgets);
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                }
            }

            ExportManagementGroups(resources);
            ExportSubscriptions(resources);
            ExportResourceGroups(resources);
        }

        private void ExportManagementGroups(List<BudgetScopeResource> resources, string filename = "managementgroups.csv")
        {
            var mgs = ParseManagementGroups(resources);

            using (var writer = new StreamWriter(filename))

            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    try
                    {
                        //csv.Context.RegisterClassMap<FooMap>();
                        csv.WriteRecords(mgs);
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                }
            }
        }

        private List<ManagementGroup> ParseManagementGroups(List<BudgetScopeResource> resources)
        {
            var mgs = new List<ManagementGroup>();

            foreach (var resource in resources)
            {
                if (resource.ResourceType == ResourceType.ManagementGroup)
                {

                    var b = new ManagementGroup()
                    {
                        ResourceId = resource.ResourceId,
                        ManagementGroupId = resource.Name,
                        DisplayName = resource.DisplayName
                    };

                    mgs.Add(b);
                }
                else
                {

                }
            }

            return mgs;
        }

        private void ExportSubscriptions(List<BudgetScopeResource> resources, string filename = "subscriptions.csv")
        {
            var mgs = ParseSubscriptions(resources);

            using (var writer = new StreamWriter(filename))

            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    try
                    {
                        //csv.Context.RegisterClassMap<FooMap>();
                        csv.WriteRecords(mgs);
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                }
            }
        }

        private List<Subscription> ParseSubscriptions(List<BudgetScopeResource> resources)
        {
            var mgs = new List<Subscription>();

            foreach (var resource in resources)
            {
                if (resource.ResourceType == ResourceType.Subscription)
                {

                    var b = new Subscription()
                    {
                        ResourceId = resource.ResourceId,
                        SubscriptionId = resource.Name,
                        DisplayName = resource.DisplayName
                    };

                    mgs.Add(b);
                }
                else
                {

                }
            }

            return mgs;
        }

        private void ExportResourceGroups(List<BudgetScopeResource> resources, string filename = "resourcegroups.csv")
        {
            var mgs = ParseResourceGroups(resources);

            using (var writer = new StreamWriter(filename))

            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    try
                    {
                        //csv.Context.RegisterClassMap<FooMap>();
                        csv.WriteRecords(mgs);
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                }
            }
        }

        private List<ResourceGroup> ParseResourceGroups(List<BudgetScopeResource> resources)
        {
            var mgs = new List<ResourceGroup>();

            foreach (var resource in resources)
            {
                if (resource.ResourceType == ResourceType.ResourceGroup)
                {

                    var b = new ResourceGroup()
                    {
                        ResourceId = resource.ResourceId,
                        DisplayName = resource.DisplayName
                    };

                    mgs.Add(b);
                }
                else
                {

                }
            }

            return mgs;
        }

        private List<Budget> ParseResources(List<BudgetScopeResource> resources)
        {
            var budgets = new List<Budget>();

            foreach (var resource in resources)
            { 
                if (resource.HasBudgets)
                {
                    foreach (var budget in resource.Budgets)
                    {
                        var b = new Budget()
                        {
                            Id = resource.ResourceId,
                            Name = resource.Name,
                            Scope = resource.ResourceType.ToString(),
                            ScopeIdentifier = resource.ResourceId,
                            ResetPeriod = budget.ResetPeriod,
                            Amount = Math.Round(budget.Amount, 2).ToString()
                        };

                        budgets.Add(b);
                    }
                }
                else
                {

                }
            }

            return budgets;
        }
    }
}
