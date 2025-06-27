using System.Net;
using AAI.Core;
using AAI.Data.Services;
using AAI.GenericChatInterface.Options;
using AAI.GenericChatInterface.Services;
using AAI.Interfaces;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

// options
builder.Services.AddOptions<DataStorageOptions>()
    .Bind(builder.Configuration.GetSection(DataStorageOptions.DataStorageOptionsName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<GeneralOptions>()
    .Bind(builder.Configuration.GetSection(GeneralOptions.GeneralWebName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<AuthApiOptions>()
    .Bind(builder.Configuration.GetSection(AuthApiOptions.AuthOptionsSectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// asp.net core features
builder.Services.AddHealthChecks();
//builder.Services.AddHttpsRedirection(options => options.HttpsPort = 443);
builder.Services.Configure<ForwardedHeadersOptions>(options=>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                               ForwardedHeaders.XForwardedProto;
    // options.KnownNetworks.Clear();
    // options.KnownProxies.Clear();
});
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddAuthorization(options => { options.FallbackPolicy = options.DefaultPolicy; });
builder.Services.AddRazorPages().AddRazorPagesOptions(options =>
        options.Conventions.AddPageRoute("/Info/Index", ""))
    .AddMicrosoftIdentityUI();
builder.Services.AddTransient<ILogger>(p =>
{
    var loggerFactory = p.GetRequiredService<ILoggerFactory>();
    return loggerFactory.CreateLogger("agents-to-agents");
});

//services
var storageOptions = builder.Configuration.GetSection(DataStorageOptions.DataStorageOptionsName).Get<DataStorageOptions>()!;
builder.Services.AddScoped<ISettingsService, CosmosDbSettingsService>(_ =>
    new(storageOptions.DatabaseName, storageOptions.SettingsContainer, storageOptions.ConnectionString));
builder.Services.AddHttpClient<ChatHttpService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler($"/{GeneralRoutes.GeneralRoute}/{GeneralRoutes.ErrorRoute}");
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseForwardedHeaders();
app.UseExceptionHandler(options =>
{
    options.Run(async context =>
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.ContentType = "application/json";
        var exception = context.Features.Get<IExceptionHandlerFeature>();
        if (exception != null)
        {
            var message = $"{exception.Error.Message}";
            await context.Response.WriteAsync(message).ConfigureAwait(false);
        }
    });
});
app.MapHealthChecks($"/{GeneralRoutes.HealthRoute}", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
}).AllowAnonymous();
app.MapRazorPages();
app.Run();