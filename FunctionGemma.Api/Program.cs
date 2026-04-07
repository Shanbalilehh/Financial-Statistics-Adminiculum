using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using FunctionGemma.Api.Interfaces;
using FunctionGemma.Api.Middleware;
using FunctionGemma.Api.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

string modelPath = builder.Configuration.GetValue<string>("Paths:modelPath") ?? throw new InvalidOperationException(
    "Missing required configuration key 'Paths:modelPath'."
);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddSingleton(sp =>
    new GemmaModelFactory(
        modelPath,
        sp.GetRequiredService<ILogger<GemmaModelFactory>>()
));

builder.Services.AddScoped<IGemmaOnnxService, GemmaOnnxService>();

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

app.MapPost("/Inference", async (HttpRequest request, IGemmaOnnxService gemmaOnnxService ,CancellationToken ct) =>
    {
        using var reader = new StreamReader(request.Body);
        string prompt = await reader.ReadToEndAsync(ct);

        if(string.IsNullOrWhiteSpace(prompt))
            return Results.BadRequest(new { Error = "The prompt cannot be empty." });

        string result = await gemmaOnnxService.GenerateTokensAsync(prompt, ct);

        return Results.Ok(new { Message = result });
    }
).Accepts<string>("text/plain")
.ProducesProblem(StatusCodes.Status200OK)
.ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status500InternalServerError);



app.Run();

