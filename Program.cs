using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Azure.Monitor.OpenTelemetry.Exporter;
using BaselineFunctionApp;

var builder = WebApplication.CreateBuilder(args);

// Ensure the application strictly honors localhost connections.
// Because the Event-Manager-Sidecar operates within the very same
// Kubernetes Pod network namespace, it communicates with localhost seamlessly.
// This completely blocks any external Pods from reaching this API!
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(8080);
});

var appInsightsConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
if (!string.IsNullOrEmpty(appInsightsConnectionString))
{
    builder.Services.AddOpenTelemetry()
        .WithTracing(tracing => tracing.AddAzureMonitorTraceExporter())
        .WithMetrics(metrics => metrics.AddAzureMonitorMetricExporter());

    builder.Logging.AddOpenTelemetry(options =>
    {
        options.AddAzureMonitorLogExporter();
    });
}


// Register the business logic
builder.Services.AddSingleton<IAppLogicService, AppLogicService>();

var app = builder.Build();

// Map the Minimal API for sidecar events
app.MapSidecarProxyEndpoints();

app.Run();
