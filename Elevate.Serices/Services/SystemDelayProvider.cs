using Elevate.Models.Contracts;

namespace Elevate.Serices.Services
{
    public class SystemDelayProvider : IDelayProvider
    {
        public Task Delay(TimeSpan delay, CancellationToken cancellationToken = default)
        {
            return Task.Delay(delay, cancellationToken);
        }
    }
}
