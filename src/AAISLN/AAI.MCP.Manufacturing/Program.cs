using AAI.MCP.Manufacturing.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<WebOptions>(builder.Configuration.GetSection(WebOptions.WebSettingsName));
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithHttpTransport(o => o.Stateless = true)
    .WithToolsFromAssembly();
builder.Services.AddHttpClient();
var app = builder.Build();
app.MapMcp("/mcp");
app.Run();