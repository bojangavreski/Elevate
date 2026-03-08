namespace Elevate.Models.Contracts
{
    public interface INotificationService
    {
        Task RequestEnqueued(int elevatorId, int from, int to, string uid);

        Task Move(int elevatorId, string direction);

        Task Stop(int elevatorId);

        Task SetIdle(int elevatorId);

        Task RemoveRequests(IEnumerable<Guid> requestUids);
    }
}
