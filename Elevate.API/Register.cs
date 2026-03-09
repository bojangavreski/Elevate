
using Elevate.Models.Contracts;
using Elevate.Serices.Contracts;
using Elevate.Serices.Services;

namespace Elevate.API
{
    public static class Register
    {

        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            return services.AddSingleton<IElevatorManager, ElevatorManager>()
                            .AddSingleton<IDelayProvider, DelayProvider>()
                            .AddSingleton<INotificationService, NotificationService>()
                            .RegisterElevatorServices();
        }
        

        public static IServiceCollection RegisterElevatorServices(this IServiceCollection services)
        {

            for (int i = 1; i <= 4; i++)
            {
                int elevatorId = i;

                services.AddSingleton<IElevator>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<SimpleElevator>>();
                    var delayProvider = sp.GetRequiredService<IDelayProvider>();
                    var serviceScopeFactory = sp.GetRequiredService<INotificationService>();

                    return new SimpleElevator(elevatorId, logger, delayProvider, serviceScopeFactory);
                });
            }

            return services;
        }
    }
}
