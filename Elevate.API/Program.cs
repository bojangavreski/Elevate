
using Elevate.Models.Contracts;
using Elevate.Serices.Contracts;
using Elevate.Serices.Hubs;
using Elevate.Serices.Services;

namespace Elevate.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSingleton<IDelayProvider, DelayProvider>()
                            .AddScoped<INotificationService, NotificationService>()
                            .RegisterElevatorServices()
                            .AddSingleton<IElevatorManager, ElevatorManager>();

            builder.Services.AddSignalR();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("SignalRPolicy", policy =>
                {
                    policy.WithOrigins("http://localhost:5173")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });


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

            app.UseCors("SignalRPolicy");

            app.MapHub<ElevatorHub>("/elevatorHub");

            app.Run();
        }
    }
}
