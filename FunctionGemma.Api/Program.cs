using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using FunctionGemma.Api.Interfaces;
using FunctionGemma.Api.Messaging.Services;
using FunctionGemma.Api.Middleware;
using FunctionGemma.Api.Services;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

string modelPath = builder.Configuration.GetValue<string>("Paths:modelPath") ?? throw new InvalidOperationException(
    "Missing required configuration key 'Paths:modelPath'."
);


// use serilog as logger and appsettings.json to configure serilog
builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

// use OpenTelemetry
builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("FSA.Api"))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();
        
        tracing.AddOtlpExporter();
    });

// Register GemmaModelFactory DI
builder.Services.AddSingleton(sp =>
    new GemmaModelFactory(
        modelPath,
        sp.GetRequiredService<ILogger<GemmaModelFactory>>()
));


builder.Services.AddHostedService<RabbitMQMessageConsumer>();
builder.Services.AddScoped<IScopedProcessingService, GemmaOnnxService>();

// Api healthcheck
builder.Services.AddHealthChecks()
    .AddCheck<GemmaModelHealthCheck>("gemma_model_check");

// NSwag setup
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "FunctionGemma.Api";
    config.Title = "FunctionGemma.Api v1";
    config.Version = "v1";
});

var app = builder.Build();

// add correlationId per http request context
app.UseMiddleware<RequestLogContextMiddleware>();

// add Healthcheck
app.MapHealthChecks("/health");

app.Services.GetRequiredService<GemmaModelFactory>();

// Http request logging
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    //swagger setup
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "FunctionGemma.Api";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}


app.Run();

