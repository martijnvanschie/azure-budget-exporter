using Azure.BudgetExporter.Model;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.Consumption;
using Azure.ResourceManager.ManagementGroups;
using Azure.ResourceManager.ManagementGroups.Models;
using Azure.ResourceManager.Resources;

namespace Azure.BudgetExporter;
public class BudgetImporter
{
    private List<ManagementGroupResource> _managementGroupResources = new List<ManagementGroupResource>();

    private List<ManagementGroup> _managementGroups = new List<ManagementGroup>();

    private List<SubscriptionResource> _subscriptionResource = new List<SubscriptionResource>();
    private List<ResourceGroupResource> _resourceGroupResource = new List<ResourceGroupResource>();
    private List<ConsumptionBudgetResource> _budgetResources = new List<ConsumptionBudgetResource>();
    private List<Budget> _budgets = new List<Budget>();
    private ArmClient _client;

    private List<BudgetScopeResource> _budgetScopeResources = new List<BudgetScopeResource>();

    public event EventHandler<ResourceScannedEventArgs> ResourceScanned;
    public event EventHandler<BudgetImportingEventArgs> BudgetImporting;

    public List<ManagementGroup> ManagemenetGroups
    {
        get { return _managementGroups; }
    }

    public List<Budget> Budgets
    {
        get { return _budgets; }
    }

    public List<BudgetScopeResource> BudgetScopeResources
    { 
        get 
        { 
            return _budgetScopeResources; 
        } 
    }

    public BudgetImporter()
    {
        _client = AzureCliContext.Client;
    }

    public void StartImport()
    {
        ImportBudgetScopeResources();
        ImportBudgets();

        BudgetExporter b = new BudgetExporter();
        b.ExportToCsv(_budgetScopeResources);
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
            if (managementGroupResource is not null)
            {
                _managementGroupResources.Add(managementGroupResource);
                _managementGroups.Add(new ManagementGroup() 
                {
                    ResourceId = managementGroupResource.Id!,
                    ManagementGroupId = managementGroupResource.Data.Name,
                    DisplayName = managementGroupResource.Data.DisplayName
                });

                var budgets = _client.GetConsumptionBudgets(managementGroupResource.Id);
                _budgetResources.AddRange(budgets);

                ResourceScanned?.Invoke(this, new ResourceScannedEventArgs() { ResourceType = "Managemeent Group", ResourceName = managementGroupResource.Data.DisplayName });
            }
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

    private void ImportBudgetScopeResources()
    {
        // Build Management Group tree
        var managementGroups = _client.GetManagementGroups().ToList();

        // Get the details
        foreach (var item in managementGroups)
        {
            var r = new BudgetScopeResource();
            r.ResourceType = Model.ResourceType.ManagementGroup;
            r.ResourceId = item.Id.ToString();
            r.Name = item.Data.Name;
            r.DisplayName = item.Data.DisplayName;

            if (_budgetScopeResources.Any(r => r.ResourceId == item.Id) == false)
            {
                _budgetScopeResources.Add(r);
            }

            // Get all Subscription
            var details = item.Get(expand: ManagementGroupExpandType.Children).Value;

            if (details != null && details.HasData)
            {
                if (details.Data.Children is not null)
                {
                    foreach (var child in details.Data.Children)
                    {
                        if (child.ChildType!.Value.ToString().Contains("managementGroups"))
                        {
                            var childr = new BudgetScopeResource();
                            childr.Parent = r;
                            childr.ResourceType = Model.ResourceType.ManagementGroup;
                            childr.ResourceId = child.Id;
                            childr.Name = child.Name;
                            childr.DisplayName = child.DisplayName;

                            if (_budgetScopeResources.Any(r => r.ResourceId == childr.ResourceId) == false)
                            {
                                _budgetScopeResources.Add(childr);
                            }
                            else
                            {
                                _budgetScopeResources.First().Parent = r;
                            }

                            ResourceScanned?.Invoke(this, new ResourceScannedEventArgs() { Resource = childr });
                        }

                        if (child.ChildType!.Value.ToString().Contains("subscription"))
                        {
                            var childr = new BudgetScopeResource();
                            childr.Parent = r;
                            childr.ResourceType = Model.ResourceType.Subscription;
                            childr.ResourceId = child.Id;
                            childr.Name = child.Name;
                            childr.DisplayName = child.DisplayName;

                            if (_budgetScopeResources.Any(r => r.ResourceId == childr.ResourceId) == false)
                            {
                                _budgetScopeResources.Add(childr);
                            }

                            ResourceScanned?.Invoke(this, new ResourceScannedEventArgs() { Resource = childr });
                        }
                    }
                }
            }

            var subscriptions = _client.GetSubscriptions().ToList();

            foreach (var s in subscriptions)
            {
                var sr = new BudgetScopeResource();
                sr.ResourceType = Model.ResourceType.Subscription;
                sr.ResourceId = s.Id.ToString();
                sr.Name = s.Data.Id.Name;
                sr.DisplayName = s.Data.DisplayName;

                if (_budgetScopeResources.Any(r => sr.ResourceId == item.Id) == false)
                {
                    _budgetScopeResources.Add(sr);
                    ResourceScanned?.Invoke(this, new ResourceScannedEventArgs() { Resource = sr });
                }

            }

            ResourceScanned?.Invoke(this, new ResourceScannedEventArgs() { Resource = r });
        }

        // Get all resource groups
        var subs = _budgetScopeResources.Where(r => r.ResourceType == Model.ResourceType.Subscription).ToList();
        foreach (var s in subs)
        {
            var sub = _client.GetSubscriptionResource(new Azure.Core.ResourceIdentifier(s.ResourceId));
            var rgs = sub.GetResourceGroups();

            foreach (ResourceGroupResource rg in rgs)
            {
                var childr = new BudgetScopeResource();
                childr.Parent = s;
                childr.ResourceType = Model.ResourceType.ResourceGroup;
                childr.ResourceId = rg.Id!;
                childr.Name = rg.Data.Name;
                childr.DisplayName = rg.Data.Name;

                if (_budgetScopeResources.Any(r => r.ResourceId == childr.ResourceId) == false)
                {
                    _budgetScopeResources.Add(childr);
                }

                ResourceScanned?.Invoke(this, new ResourceScannedEventArgs() { Resource = childr });
            }
        }
    }

    private void ImportBudgets()
    {
        Parallel.ForEach(_budgetScopeResources, resource =>
        {
            BudgetImporting?.Invoke(this, new BudgetImportingEventArgs() { Resource = resource, ImportingStatus = BudgetImportingStatus.StartedImport });

            try
            {
                var budgets = _client.GetConsumptionBudgets(new ResourceIdentifier(resource.ResourceId)).ToList();

                if (budgets == null || !budgets.Any())
                {
                    BudgetImporting?.Invoke(this, new BudgetImportingEventArgs() { Resource = resource, ImportingStatus = BudgetImportingStatus.FinishedImport });
                    return;
                }

                foreach (ConsumptionBudgetResource budget in budgets)
                {
                    var b = new BudgetInfo(budget);
                    resource.Budgets.Add(b);
                }
            }
            catch (Exception)
            {
                BudgetImporting?.Invoke(this, new BudgetImportingEventArgs() { Resource = resource, ImportingStatus = BudgetImportingStatus.FailedImport });
            }

            BudgetImporting?.Invoke(this, new BudgetImportingEventArgs() { Resource = resource, ImportingStatus = BudgetImportingStatus.FinishedImport });
        });
    }
}