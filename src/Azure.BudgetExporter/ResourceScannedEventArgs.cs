using Azure.BudgetExporter.Model;

namespace Azure.BudgetExporter;

public class ResourceScannedEventArgs
{
    [Obsolete($"Use the new {nameof(Resource)} property to get details about the resource.")]
    public string ResourceType { get; set; }
    
    [Obsolete($"Use the new {nameof(Resource)} property to get details about the resource.")]
    public string ResourceName { get; set; }

    public BudgetScopeResource? Resource { get; set; }
}

public class BudgetImportingEventArgs
{
    public BudgetScopeResource? Resource { get; set; }

    public BudgetImportingStatus? ImportingStatus { get; set; } = null;
}

public enum BudgetImportingStatus
{
    StartedImport,
    FinishedImport,
    FailedImport
}