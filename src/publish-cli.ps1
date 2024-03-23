### Run the following command to enable execution of unsigned scripts in current sessions.
### Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope Process
###


$publishingfolder = "c:\apps\Azure utils"
$singlefile = "true"

dotnet publish `
    .\Azure.BudgetExporter.Cli\Azure.BudgetExporter.Cli.csproj `
    -p:PublishSingleFile=$singlefile `
    --output $publishingfolder `
    --runtime win-x64 `
    --configuration "Release" `
    --no-self-contained