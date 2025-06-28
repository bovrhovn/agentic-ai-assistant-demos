using AAI.GraphConnector.Services;
using Spectre.Console;

Console.WriteLine("Create connection to Microsoft Graph...");

var clientId = Environment.GetEnvironmentVariable("CLIENTID");
ArgumentException.ThrowIfNullOrEmpty(clientId);
var clientSecret = Environment.GetEnvironmentVariable("CLIENTSECRET");
ArgumentException.ThrowIfNullOrEmpty(clientSecret);
var tenantId = Environment.GetEnvironmentVariable("TENANTID");
ArgumentException.ThrowIfNullOrEmpty(tenantId);

AnsiConsole.Write(new Markup("[green]TenantID: [/][bold]" + tenantId + "[/]"));
AnsiConsole.WriteLine();
AnsiConsole.Write(new Markup("[green]ClientID: [/][bold]" + clientId + "[/]"));
AnsiConsole.WriteLine();

var graphService = new GraphService(tenantId, clientId, clientSecret);
var connections = await graphService.GetConnectionListAsync();
if (connections == null || connections.Count == 0)
{
    Console.WriteLine("No connections found, creating a new one...");
    var connection = await graphService.CreateConnectionAsync("compmachines",
        "Company Machines",
        "Company machines data");
    if (connection == null)
    {
        AnsiConsole.WriteException(new Exception("Failed to create connection."));
        return;
    }

    connections = await graphService.GetConnectionListAsync();
}

string[] choices = connections.Select(c => c.Name).ToArray()!;
AnsiConsole.Write(new Markup("[green]Found [/][bold]" + connections.Count + "[/] connections."));
var connectionName = AnsiConsole.Prompt(
    new SelectionPrompt<string>().Title("Select [green]connection[/]?")
        .MoreChoicesText("[grey](Move up and down to reveal more connections)[/]")
        .AddChoices(choices));
AnsiConsole.Write(new Markup("[green]Selected connection: [/][bold]" + connectionName + "[/]"));
AnsiConsole.WriteLine();
var selectedConnection = connections.FirstOrDefault(c => c.Name == connectionName);
var graphSchemaManager = new GraphSchemaManager(tenantId, clientId, clientSecret);
var schema = await graphSchemaManager.GetSchemaAsync(selectedConnection?.Id);
if (schema == null)
{
    AnsiConsole.WriteLine("No schema found for the selected connection. Registering a new schema...");
    await graphSchemaManager.RegisterSchemaAsync(selectedConnection?.Id, graphSchemaManager.GetDefaultSchema());
    AnsiConsole.WriteLine("Schema registered successfully.");
}

var dataService = new GraphDataService(tenantId, clientId, clientSecret);
AnsiConsole.WriteLine("Ready to add or update items in the selected connection.");
var numberOfItems = AnsiConsole.Prompt(new TextPrompt<int>("Enter number of items to generate:").DefaultValue(10));
await dataService.PopulateWithRandomDataAsync(selectedConnection?.Id ?? string.Empty, numberOfItems);
AnsiConsole.WriteLine("Items added or updated successfully, check it online.");