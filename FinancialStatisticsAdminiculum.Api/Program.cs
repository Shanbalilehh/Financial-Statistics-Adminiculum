using FinancialStatisticsAdminiculum.Core.Interfaces;
using FinancialStatisticsAdminiculum.Infrastructure.Repositories;
using FinancialStatisticsAdminiculum.Infrastructure.Persistence;
using FinancialStatisticsAdminiculum.Infrastructure.AI;
using FinancialStatisticsAdminiculum.Infrastructure.ExceptionHandling;
using FinancialStatisticsAdminiculum.Application.AI;
using FinancialStatisticsAdminiculum.Application.AI.Tools;
using Microsoft.EntityFrameworkCore;
using FinancialStatisticsAdminiculum.Application.Services;
using FinancialStatisticsAdminiculum.Application.Interfaces;
using FinancialStatisticsAdminiculum.Api.Infrastructure;
using FinancialStatisticsAdminiculum.Api.Extensions;
using Castle.DynamicProxy;
using Serilog;
using Serilog.Formatting.Compact;

namespace FinancialStatisticsAdminiculum.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()                         // picks up LogContext.PushProperty(...)
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .WriteTo.Console(new CompactJsonFormatter())     // structured JSON to stdout
                .WriteTo.File(
                    new CompactJsonFormatter(),
                    path: "logs/fsa-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 14
                )
                .CreateLogger();
            try
            {
                var builder = WebApplication.CreateBuilder(args);
                builder.Host.UseSerilog();

                //Connection string appsetting.json
                var connectionString = builder.Configuration.GetConnectionString("LocalConnection");
                // Add services to the container.

                //Dbcontext
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(connectionString));
                builder.Services.AddControllers();

                // Register the Generic Repository
                // "Scoped" is correct because DbContext is Scoped.
                builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

                // Register Unit of Work
                builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

                //AI Service Registration with Dynamic JSON Schema  
                // 1. Build the massive JSON string dynamically at startup
                string dynamicToolsJson = AiSchemaAggregator.BuildCombinedToolJson();

                // 2. Pass it directly into your Singleton AI Service
                builder.Services.AddSingleton<INlpEngine>(sp => 
                    new GemmaOnnxService(@"/home/chi/models/functiongemma_oga", dynamicToolsJson));

                // 3. Register your execution handlers
                builder.Services.AddKeyedScoped<IAiToolHandler, SmaToolHandler>(SmaToolHandler.ToolName);

                //Register Application Services handled by proxyExtension
                //builder.Services.AddScoped<OrchestratorService>();
                //builder.Services.AddScoped<TrendAnalysisService>();

                // Register the Database Seeder
                builder.Services.AddScoped<DatabaseSeeder>();

                //Exception Handling
                // 1. Register the ASP.NET Core Global Exception Handler
                builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
                builder.Services.AddProblemDetails();

                // 2. Register Castle DynamicProxy Mechanics
                builder.Services.AddSingleton<ProxyGenerator>();
                builder.Services.AddTransient<SecurityExceptionInterceptor>();

                // 3. Register the Diagnostic & Repair (D&R) Experts using Keyed DI
                builder.Services.AddKeyedScoped<IDiagnosticExpert, NlpDiagnosticExpert>("NlpCommunity");
                // builder.Services.AddKeyedScoped<IDiagnosticExpert, PersistenceDiagnosticExpert>("PersistenceCommunity");

                // 4. Register Proxied Application Services
                // Assuming you created the ProxyExtensions class we discussed earlier.
                builder.Services.AddProxiedScoped<IOrchestratorService, OrchestratorService, SecurityExceptionInterceptor>();
                builder.Services.AddProxiedScoped<ITrendAnalysisService, TrendAnalysisService, SecurityExceptionInterceptor>();


                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                var app = builder.Build();

                using (var scope = app.Services.CreateScope())
                {
                    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
                    await seeder.SeedAsync();
                }

                app.UseExceptionHandler();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();

                app.UseAuthorization();


                app.MapControllers();

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();                             
            }
        }
    }
}
