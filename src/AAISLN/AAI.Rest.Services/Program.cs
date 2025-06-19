using AAI.Core;
using AAI.Data.Services;
using AAI.Interfaces;
using AAI.Rest.Services.Options;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOptions<StorageOptions>()
    .Bind(builder.Configuration.GetSection(StorageOptions.AppSettingsName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
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
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
var storageOptions = builder.Configuration.GetSection(StorageOptions.AppSettingsName).Get<StorageOptions>()!;
builder.Services.AddScoped<ISettingsService, CosmosDbSettingsService>(_ =>
    new(storageOptions.DatabaseName, storageOptions.SettingsContainer, storageOptions.ConnectionString));
var dataOptions = builder.Configuration.GetSection(DataOptions.DataSettingsName).Get<DataOptions>()!;
var cosmosDbChatRepository =
    new CosmosDbChatRepository(dataOptions.DatabaseName, dataOptions.ChatContainer, dataOptions.ConnectionString);
builder.Services.AddScoped<IChatRepository, CosmosDbChatRepository>(_ => cosmosDbChatRepository);
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
app.UseCors("AllowLocalHostAndProduction");
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks($"/{GeneralRoutes.HealthRoute}", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
}).AllowAnonymous();
app.Run();