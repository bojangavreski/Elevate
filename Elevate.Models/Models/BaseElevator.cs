using Elevate.Models.Contracts;
using Elevate.Models.Enums;

namespace Elevate.Models.Models
{
    public abstract class BaseElevator : IElevator
    {
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
            return elevatorRequest.From >= 1 &&
                   elevatorRequest.To > 1 &&
                   elevatorRequest.From <= 10 && elevatorRequest.To <= 10 &&
                   elevatorRequest.From != elevatorRequest.To;
        }
    }
}
