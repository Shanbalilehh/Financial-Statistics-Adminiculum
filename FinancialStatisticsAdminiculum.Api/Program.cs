using FinancialStatisticsAdminiculum.Core.Interfaces;
using FinancialStatisticsAdminiculum.Infrastructure.Repositories;
using FinancialStatisticsAdminiculum.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinancialStatisticsAdminiculum.Api
{
    public class Program
    {
        public static void Main(string[] args)
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

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

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
