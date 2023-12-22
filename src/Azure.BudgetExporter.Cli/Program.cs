// See https://aka.ms/new-console-template for more information
using Azure.BudgetExporter;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.ManagementGroups;
using Azure.ResourceManager.Resources;
using CsvHelper;
using System.Globalization;

Console.WriteLine("Starting budget export!");

var client = new ArmClient(new AzureCliCredential());
var importer = new BudgetImporter(client);

var tenant = client.GetTenants().First();
Console.WriteLine($"Tenant: {tenant.Data.DisplayName}");

var mg = client.GetManagementGroups();

foreach (ManagementGroupResource r in mg)
{
    Console.WriteLine($"Management Group: {r.Data.Name}");
    importer.ImportManagementGroup(r);
}

var subs = client.GetSubscriptions();

foreach (SubscriptionResource s in subs)
{
    importer.ImportSubscription(s);
}

try
{
    importer.ParseBudgetResources();
}
catch (Exception ex)
{
    Console.WriteLine($"{ex.Message}");
}

using (var writer = new StreamWriter("export.csv"))

{
    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
    {
        try
        {
            //csv.Context.RegisterClassMap<FooMap>();
            csv.WriteRecords(importer.Budgets);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

    }
}