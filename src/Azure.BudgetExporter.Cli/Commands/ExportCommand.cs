using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.ManagementGroups;
using Azure.ResourceManager.Resources;
using CsvHelper;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Azure.BudgetExporter.Cli.Commands
{
    internal class ExportCommand : Command<ExportCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            //[CommandArgument(0, "[Filename]")]
            //[DefaultValue("")]
            //public string FileName { get; set; }

            [CommandOption("-f|--filename")]
            [DefaultValue("export.csv")]
            [Description("Output filename")]
            public string Filename { get; set; } = "export.csv";

            [CommandOption("-o|--output")]
            [DefaultValue("csv")]
            [Description("Output format, accepted values: csv, table")]
            public string Output { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            AnsiConsole.MarkupLine($"Exporting budgets in format: [blue]{settings.Output}[/]");
            AnsiConsole.WriteLine();

            var client = new ArmClient(new AzureCliCredential());
            var importer = new BudgetImporter(client);
            importer.ResourceScanned += Importer_ResourceScanned;

            AnsiConsole.Status()
                .Start("Scanning Management Groups...", ctx =>
                {
                    var mg = client.GetManagementGroups();

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

                    AnsiConsole.MarkupLine($"Export done to file [blue]{settings.Filename}[/]...");
                });

            return 0;
        }

        private void Importer_ResourceScanned(object? sender, ResourceScannedEventArgs e)
        {
            AnsiConsole.MarkupLine($"{e.ResourceType} {e.ResourceName} scanned...");
        }
    }
}
