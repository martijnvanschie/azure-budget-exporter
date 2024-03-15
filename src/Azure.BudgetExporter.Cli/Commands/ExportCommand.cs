using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.ManagementGroups;
using Azure.ResourceManager.Resources;
using CsvHelper;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Globalization;

namespace Azure.BudgetExporter.Cli.Commands
{
    internal class ExportCommand : Command<ExportCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-f|--filename")]
            [DefaultValue("export.csv")]
            [Description("Output filename")]
            public string Filename { get; set; } = "export.csv";

            [CommandOption("-o|--output")]
            [DefaultValue("csv")]
            [Description("Output format, accepted values: csv, table")]
            public string Output { get; set; }

            [CommandOption("-d|--dimensiontables")]
            [DefaultValue(false)]
            [Description("Export the dimension tables into a seperate file.")]
            public bool ExportDimensionTables { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            AnsiConsole.MarkupLine($"Exporting budgets in format: [blue]{settings.Output}[/]");
            AnsiConsole.WriteLine();

            var client = new ArmClient(new AzureCliCredential());
            var importer = new BudgetImporter();
            importer.ResourceScanned += Importer_ResourceScanned;
            importer.BudgetImporting += Importer_BudgetImporting;
            importer.StartImport();

            return 0;

            foreach (var item in importer.BudgetScopeResources)
            {
                switch (item.ResourceType)
                {
                    case Model.ResourceType.ResourceGroup:
                        AnsiConsole.MarkupLine($"[grey]{item.Name} {item.DisplayName} {item.ResourceType}[/]");
                        break;
                    case Model.ResourceType.ManagementGroup:
                        AnsiConsole.MarkupLine($"[blue]{item.Name} {item.DisplayName} {item.ResourceType}[/]");
                        break;
                    case Model.ResourceType.Subscription:
                        AnsiConsole.MarkupLine($"[yellow]{item.Name} {item.DisplayName} {item.ResourceType}[/]");
                        break;
                    default:
                        break;
                }
            }

            return 0;

            importer.ResourceScanned += Importer_ResourceScanned;

            AnsiConsole.Status()
                .Start("Scanning Management Groups...", ctx =>
                {
                    var mg = client.GetManagementGroups().ToList();

                    foreach (ManagementGroupResource r in mg)
                    {
                        AnsiConsole.MarkupLine($"Management Group: [blue]{r.Data.Name}[/]");
                        importer.ImportManagementGroup(r);
                    }

                    AnsiConsole.MarkupLine("Management Groups scanned...");

                    // Update the status and spinner
                    ctx.Status("Scanning Subscriptions and Resource Groups");
                    ctx.Spinner(Spinner.Known.Star);
                    ctx.SpinnerStyle(Style.Parse("yellow"));

                    var subs = client.GetSubscriptions();

                    foreach (SubscriptionResource s in subs)
                    {
                        importer.ImportSubscription(s);
                    }

                    AnsiConsole.MarkupLine("Subscriptions scanned...");

                    ctx.Status("Parsing imports");
                    ctx.Spinner(Spinner.Known.Star);
                    ctx.SpinnerStyle(Style.Parse("gray"));

                    try
                    {
                        importer.ParseBudgetResources();
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine($"{ex.Message}");
                    }

                    AnsiConsole.MarkupLine("Imports parsed...");

                    ctx.Status("Exporting budgets");

                    using (var writer = new StreamWriter(settings.Filename))

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
                                AnsiConsole.MarkupLine(ex.Message);
                            }

                        }
                    }

                    if (settings.ExportDimensionTables)
                    {
                        ExportDimensionTables(importer);
                    }

                    AnsiConsole.MarkupLine($"Export done to file [blue]{settings.Filename}[/]...");
                });

            return 0;
        }


        private void ExportDimensionTables(BudgetImporter importer)
        {
            using (var writer = new StreamWriter("managementgroups.csv"))

            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    try
                    {
                        //csv.Context.RegisterClassMap<FooMap>();
                        csv.WriteRecords(importer.ManagemenetGroups);
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine(ex.Message);
                    }

                }
            }
        }

        private void Importer_ResourceScanned(object? sender, ResourceScannedEventArgs e)
        {
            AnsiConsole.MarkupLine($"{e.Resource?.ResourceType} {e.Resource?.Name} scanned...");
        }

        private void Importer_BudgetImporting(object? sender, BudgetImportingEventArgs e)
        {
            switch (e.ImportingStatus)
            {
                case BudgetImportingStatus.StartedImport:
                    AnsiConsole.MarkupLine($"Importing budget for {e.Resource?.ResourceType} {e.Resource?.Name} ...");
                    break;
                case BudgetImportingStatus.FailedImport:
                    AnsiConsole.MarkupLine($"[red]Importing budget for {e.Resource?.ResourceType} {e.Resource?.Name} ...[/]");
                    break;
                case BudgetImportingStatus.FinishedImport:
                    AnsiConsole.MarkupLineInterpolated($"[green]Finished importing budget(s) for {e.Resource?.ResourceType} {e.Resource?.Name} ...[/]");
                    break;
                default:
                    break;
            }

        }
    }
}
