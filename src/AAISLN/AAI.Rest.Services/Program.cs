using AAI.Core;
using AAI.Data.Services;
using AAI.Interfaces;
using AAI.Rest.Services.Options;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOptions<DataOptions>()
    .Bind(builder.Configuration.GetSection(DataOptions.DataSettingsName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
var app = builder.Build();
var dataOptions = builder.Configuration.GetSection(DataOptions.DataSettingsName).Get<DataOptions>()!;
builder.Services.AddScoped<IChatRepository, CosmosDbChatRepository>(_ =>
    new(dataOptions.DatabaseName, dataOptions.ChatContainer, dataOptions.ConnectionString));

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler($"/{GeneralRoutes.GeneralRoute}/{GeneralRoutes.ErrorRoute}");

app.UseForwardedHeaders();
app.MapOpenApi();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks($"/{GeneralRoutes.HealthRoute}", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
}).AllowAnonymous();
app.Run();