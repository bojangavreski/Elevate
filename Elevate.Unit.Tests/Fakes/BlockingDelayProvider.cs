using Elevate.Models.Contracts;

namespace Elevate.Unit.Tests.Fakes
{
    public class BlockingDelayProvider : IDelayProvider
    {
        private readonly TaskCompletionSource<bool> _blockingTcs = new TaskCompletionSource<bool>();

        public Task Delay(TimeSpan delay, CancellationToken cancellationToken = default)
        {
            return _blockingTcs.Task.WaitAsync(cancellationToken);
        }
    }
}
