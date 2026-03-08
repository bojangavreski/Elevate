using Elevate.Models.Contracts;
using Elevate.Serices.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Elevate.Serices.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<ElevatorHub> _elevatorHubContext;

        public NotificationService(IHubContext<ElevatorHub> elevatorHubContext)
        {
            _elevatorHubContext = elevatorHubContext;
        }

        public async Task RequestEnqueued(int elevatorId, int from, int to)
        {
            await _elevatorHubContext.Clients.All.SendAsync("EnqueueRequest", elevatorId, from, to);
        }

        public async Task Move(int elevatorId, string direction)
        {
            await _elevatorHubContext.Clients.All.SendAsync("MoveElevator", elevatorId, direction);
        }

        public async Task Stop(int elevatorId)
        {
            await _elevatorHubContext.Clients.All.SendAsync("StopElevator", elevatorId);
        }

        public async Task SetIdle(int elevatorId)
        {
            await _elevatorHubContext.Clients.All.SendAsync("SetIdle", elevatorId);
        }
    }
}
