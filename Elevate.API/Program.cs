
using Elevate.Models.Contracts;
using Elevate.Serices.Contracts;
using Elevate.Serices.Services;

namespace Elevate.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddSingleton<IElevator>(sp => 
                    new SimpleElevator(1, sp.GetRequiredService<ILogger<SimpleElevator>>()));
            builder.Services.AddSingleton<IElevator>(sp =>
                   new SimpleElevator(2, sp.GetRequiredService<ILogger<SimpleElevator>>()));
            builder.Services.AddSingleton<IElevator>(sp =>
                   new SimpleElevator(3, sp.GetRequiredService<ILogger<SimpleElevator>>()));
            builder.Services.AddSingleton<IElevator>(sp =>
                   new SimpleElevator(4, sp.GetRequiredService<ILogger<SimpleElevator>>()));

            builder.Services.AddSingleton<IElevatorManager, ElevatorManager>();

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build(); 

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi(); 
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
