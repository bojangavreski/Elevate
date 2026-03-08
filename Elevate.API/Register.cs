
using Elevate.Models.Contracts;
using Elevate.Serices.Services;

namespace Elevate.API
{
    public static class Register
    {

        public static IServiceCollection RegisterElevatorServices(this IServiceCollection services)
        {

            for (int i = 1; i <= 4; i++)
            {
                services.AddSingleton<IElevator>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<SimpleElevator>>();
                    var delayProvider = sp.GetRequiredService<IDelayProvider>();

                    return new SimpleElevator(i, logger, delayProvider);
                });
            }

            return services;
        }
    }
}
