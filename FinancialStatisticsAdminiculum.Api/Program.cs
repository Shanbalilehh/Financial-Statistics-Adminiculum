using FinancialStatisticsAdminiculum.Core.Interfaces;
using FinancialStatisticsAdminiculum.Infrastructure.Repositories;
using FinancialStatisticsAdminiculum.Infrastructure.Persistence;
using FinancialStatisticsAdminiculum.Infrastructure.AI;
using FinancialStatisticsAdminiculum.Application.AI;
using FinancialStatisticsAdminiculum.Application.AI.Tools;
using Microsoft.EntityFrameworkCore;
using FinancialStatisticsAdminiculum.Application.Services;

namespace FinancialStatisticsAdminiculum.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //Connection string appsetting.json
            var connectionString = builder.Configuration.GetConnectionString("LocalConnection");
            // Add services to the container.

            //Dbcontext
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));
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
                new GemmaOnnxService(@"C:\Users\holit\Downloads\FunctionGemmaPytorch\functiongemma_onnx_genai", dynamicToolsJson));

            // 3. Register your execution handlers
            builder.Services.AddKeyedScoped<IAiToolHandler, SmaToolHandler>(SmaToolHandler.ToolName);

            builder.Services.AddScoped<OrchestratorService>();
            builder.Services.AddScoped<TrendAnalysisService>();

            // Register the Database Seeder
            builder.Services.AddScoped<DatabaseSeeder>();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
                await seeder.SeedAsync();
            }

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
    }
}
