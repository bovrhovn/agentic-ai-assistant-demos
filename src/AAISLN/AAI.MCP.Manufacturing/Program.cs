using AAI.Data.Services;
using AAI.Interfaces;
using AAI.MCP.Manufacturing.Options;
using AAI.MCP.Manufacturing.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<WebOptions>(builder.Configuration.GetSection(WebOptions.WebSettingsName));
builder.Services.Configure<MachineDataOptions>(
    builder.Configuration.GetSection(MachineDataOptions.MachineDataSettingsName));
builder.Services
    .AddMcpServer()
    .WithHttpTransport(o => o.Stateless = true)
    .WithResourcesFromAssembly()
    .WithToolsFromAssembly();
var storageOptions = builder.Configuration.GetSection(MachineDataOptions.MachineDataSettingsName)
    .Get<MachineDataOptions>()!;
builder.Services.AddScoped<IStorageService, MachineStorageService>(_ =>
    new(storageOptions.ContainerName, storageOptions.ConnectionString));
builder.Services.AddHttpClient<MachineService>();
builder.Services.AddHttpClient();
var app = builder.Build();
app.MapMcp("/mcp");
app.Run();