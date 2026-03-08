using Elevate.Models.Contracts;

namespace Elevate.Unit.Tests.Fakes
{
    public class FakeDelayProvider : IDelayProvider
    {
        public async Task Delay(TimeSpan delay, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
        }
    }
}
