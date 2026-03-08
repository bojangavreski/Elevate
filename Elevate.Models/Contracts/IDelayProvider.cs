namespace Elevate.Models.Contracts
{
    public interface IDelayProvider
    {
        Task Delay(TimeSpan delay, CancellationToken cancellationToken = default);
    }
}
