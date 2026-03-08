using Elevate.Models.Contracts;

namespace Elevate.Unit.Tests.Fakes
{
    public class FakeDelayProvider : IDelayProvider
    {
        public Task Delay(TimeSpan delay, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
