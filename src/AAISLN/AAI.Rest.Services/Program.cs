using AAI.Core;
using AAI.Data.Services;
using AAI.Interfaces;
using AAI.Rest.Services.Data;
using AAI.Rest.Services.Options;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOptions<DataStorageOptions>()
    .Bind(builder.Configuration.GetSection(DataStorageOptions.DataStorageOptionsName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<MachineStorageOptions>()
    .Bind(builder.Configuration.GetSection(MachineStorageOptions.StorageSettingsName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddMemoryCache();
builder.Services.AddTransient<ILogger>(p =>
{
    var loggerFactory = p.GetRequiredService<ILoggerFactory>();
    return loggerFactory.CreateLogger("AI assistant data API");
});
builder.Services.AddOptions<DataOptions>()
    .Bind(builder.Configuration.GetSection(DataOptions.DataSettingsName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalHostAndProduction",
        policy =>
        {
            policy.WithOrigins("https://localhost:5009", "https://chat.vrhovnik.cloud")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});
builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "AI assistant data API",
            Version = "v1",
            Description = "API for demo purposes to use different approaches.",
            Contact = new()
            {
                Name = "API Support",
                Email = "api-assistant@thisiscool.com"
            }
        };
        var localServer = new OpenApiServer
        {
            Description = "Local server for development purposes",
            Url = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "https://localhost:5009"
        };
        document.Servers.Add(localServer);
        var prodServer = new OpenApiServer
        {
            Description = "Production server",
            Url = Environment.GetEnvironmentVariable("PROD_URL") ?? "https://chat.vrhovnik.cloud"
        };
        document.Servers.Add(prodServer);
        return Task.CompletedTask;
    });
});

builder.Services.AddHealthChecks();
var storageOptions = builder.Configuration.GetSection(DataStorageOptions.DataStorageOptionsName).Get<DataStorageOptions>()!;
builder.Services.AddScoped<ISettingsService, CosmosDbSettingsService>(_ =>
    new(storageOptions.DatabaseName, storageOptions.SettingsContainer, storageOptions.ConnectionString));

var objectStorageOptions = builder.Configuration.GetSection(MachineStorageOptions.StorageSettingsName)
    .Get<MachineStorageOptions>()!;
builder.Services.AddScoped<IStorageService, MachineStorageService>(_ =>
    new(objectStorageOptions.LogContainer,objectStorageOptions.StorageUri));
var dataOptions = builder.Configuration.GetSection(DataOptions.DataSettingsName).Get<DataOptions>()!;
var cosmosDbChatRepository =
    new CosmosDbChatRepository(dataOptions.DatabaseName, dataOptions.ChatContainer, dataOptions.ConnectionString);
builder.Services.AddScoped<IChatRepository, CosmosDbChatRepository>(_ => cosmosDbChatRepository);
builder.Services.AddSingleton<FakeDataGenerator>();
builder.Services.AddScoped<IAzureOpenAIBotService, AzureOpenAIChatService>(_ =>
    new(cosmosDbChatRepository, dataOptions.AzureOpenAIBaseURI, dataOptions.DeploymentName));

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler($"/{GeneralRoutes.GeneralRoute}/{GeneralRoutes.ErrorRoute}");
    app.UseHttpsRedirection();
}

app.UseForwardedHeaders();
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options
        .WithTitle("AI assistant data API")
        .WithDownloadButton(true)
        .WithTheme(ScalarTheme.BluePlanet)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});
app.UseCors("AllowLocalHostAndProduction");
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks($"/{GeneralRoutes.HealthRoute}", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
}).AllowAnonymous();
app.Run();