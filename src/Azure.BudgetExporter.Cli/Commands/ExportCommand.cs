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
