using Elevate.Models.Contracts;
using Elevate.Models.Enums;

namespace Elevate.Models.Models
{
    public abstract class BaseElevator : IElevator
    {
        protected const int MinFloor = 1;
        protected const int MaxFloor = 10;

        public int Id { get; }

        protected BaseElevator(int id)
        {
            Id = id;
        }

        protected int CurrentFloor { get; set; }

        protected ElevatorDirectionType Direction { get; set; }

        public abstract int CalculateCost(ElevatorRequest request);

        public abstract Task EnqueueRequest(ElevatorRequest request, CancellationToken cancellationToken);

        public virtual bool CanEnqueue(ElevatorRequest elevatorRequest)
        {
            if (elevatorRequest == null)
                return false;

            return elevatorRequest.From >= MinFloor &&
                   elevatorRequest.To >= MinFloor &&
                   elevatorRequest.From <= MaxFloor &&
                   elevatorRequest.To <= MaxFloor &&
                   elevatorRequest.From != elevatorRequest.To;
        }
    }
}
