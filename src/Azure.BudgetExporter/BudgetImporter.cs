using Azure.BudgetExporter;
using Azure.ResourceManager;
using Azure.ResourceManager.Consumption;
using Azure.ResourceManager.ManagementGroups;
using Azure.ResourceManager.Resources;

namespace Azure.BudgetExporter;
public class BudgetImporter
{
    private List<ManagementGroupResource> _managementGroupResources = new List<ManagementGroupResource>();
    private List<SubscriptionResource> _subscriptionResource = new List<SubscriptionResource>();
    private List<ResourceGroupResource> _resourceGroupResource = new List<ResourceGroupResource>();
    private List<ConsumptionBudgetResource> _budgetResources = new List<ConsumptionBudgetResource>();
    private List<Budget> _budgets = new List<Budget>();
    private ArmClient _client;

    public event EventHandler<ResourceScannedEventArgs> ResourceScanned;

    public List<ConsumptionBudgetResource> BudgetResources
    {
        get { return _budgetResources; }
    }

    public List<Budget> Budgets
    {
        get { return _budgets; }
    }

    public BudgetImporter(ArmClient client)
    {
        _client = client;
    }

    public void ParseBudgetResources()
    {
        foreach (ConsumptionBudgetResource r in _budgetResources)
        {
            if (r.HasData)
            {
                Budget b = new Budget();

                b.Id = r.Id;
                b.Name = r.Data.Name;
                b.Scope = r.Id.Parent.ResourceType.Type;
                b.ScopeIdentifier = r.Id.Parent;

                b.ResetPeriod = r.Data.TimeGrain.ToString();
                b.Amount = r.Data.Amount.Value.ToString();
                b.EvaluatedSpend = r.Data.CurrentSpend.Amount.ToString();
                b.ForecastSpend = r.Data.ForecastSpend?.Amount?.ToString() ?? "NaN";

                switch (r.Id.Parent.ResourceType.Type.ToLower())
                {
                    case "managementgroups":
                        b.ScopeName = "Not implemented yet";
                        b.SubscriptionDisplayName = "";
                        b.ResourceGroupDisplayName = "";
                        break;

                    case "subscriptions":
                        var sub = _subscriptionResource.FirstOrDefault(s => s.Id == r.Id.Parent);

                        if (sub is not null)
                        {
                            b.ScopeName = sub.Data.DisplayName;
                            b.SubscriptionDisplayName = sub.Data.DisplayName;
                        }

                        b.ResourceGroupDisplayName = "";
                        break;

                    case "resourcegroups":
                        var sub2 = _subscriptionResource.FirstOrDefault(s => s.Id == r.Id.Parent.Parent);

                        if (sub2 is not null)
                        {
                            b.SubscriptionDisplayName = sub2.Data.DisplayName;
                        }

                        b.ScopeName = r.Id.Parent.ResourceGroupName;
                        b.ResourceGroupDisplayName = r.Id.Parent.ResourceGroupName;
                        break;

                    default:
                        b.SubscriptionDisplayName = "";
                        b.ResourceGroupDisplayName = "";
                        break;

                }

                _budgets.Add(b);
            }

        }
    }

    public void ImportManagementGroup(ManagementGroupResource managementGroupResource)
    {
        try
        {
            _managementGroupResources.Add(managementGroupResource);

            var budgets = _client.GetConsumptionBudgets(managementGroupResource.Id);
            _budgetResources.AddRange(budgets);

            ResourceScanned?.Invoke(this, new ResourceScannedEventArgs() { ResourceType = "Managemeent Group", ResourceName = managementGroupResource.Data.DisplayName });
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"{ex}");
        }
    }

    public void ImportSubscription(SubscriptionResource subscriptionResource)
    {
        try
        {
            _subscriptionResource.Add(subscriptionResource);

            var budgets = _client.GetConsumptionBudgets(subscriptionResource.Id);
            _budgetResources.AddRange(budgets);
            ResourceScanned?.Invoke(this, new ResourceScannedEventArgs() { ResourceType = "Subscription", ResourceName = subscriptionResource.Data.DisplayName });

            var rgs = subscriptionResource.GetResourceGroups();

            foreach (ResourceGroupResource rg in rgs)
            {
                ImportResourceGroup(rg);
                ResourceScanned?.Invoke(this, new ResourceScannedEventArgs() { ResourceType = "Resource Group", ResourceName = rg.Data.Name });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex}");
        }
    }

    public void ImportResourceGroup(ResourceGroupResource resourceGroupResource)
    {
        try
        {
            _resourceGroupResource.Add(resourceGroupResource);

            var budgets = _client.GetConsumptionBudgets(resourceGroupResource.Id);
            _budgetResources.AddRange(budgets);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex}");
        }
    }
}