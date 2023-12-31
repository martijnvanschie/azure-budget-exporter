// See https://aka.ms/new-console-template for more information
using Azure.BudgetExporter;
using Azure.BudgetExporter.Cli;
using Azure.BudgetExporter.Cli.Commands;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.ManagementGroups;
using Azure.ResourceManager.Resources;
using CsvHelper;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Globalization;

var app = new CommandApp();
//app.SetDefaultCommand<AccumulatedCostCommand>();

app.Configure(config => 
{
    config.SetApplicationName(Constants.APP_NAME);

    config.AddExample(new[] { "export" });
    //config.AddExample(new[] { "export", "-o", "csv" });
    //config.AddExample(new[] { "export", "-o", "db" });
    
    //config.AddExample(new[] { "accumulatedCost", "-o", "json" });
    //config.AddExample(new[] { "costByResource", "-s", "00000000-0000-0000-0000-000000000000", "-o", "text" });
    //config.AddExample(new[] { "dailyCosts", "--dimension", "MeterCategory" });
    //config.AddExample(new[] { "budgets", "-s", "00000000-0000-0000-0000-000000000000" });
    //config.AddExample(new[] { "detectAnomalies", "--dimension", "ResourceId", "--recent-activity-days", "4" });
    //config.AddExample(new[] { "costByTag", "--tag", "cost-center" });

#if DEBUG
    config.PropagateExceptions();
#endif

    // Add commands
    config.AddCommand<ExportCommand>("export")
    .WithDescription("Export configured budgets from your tenant to an output format.");

    config.ValidateExamples();

    config.SetExceptionHandler(ex =>
    {
        AnsiConsole.MarkupInterpolated($"[red]{ex.Message}[/]");
        return -99;
    });

});

try
{
    return await app.RunAsync(args);
}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
    return -99;
}