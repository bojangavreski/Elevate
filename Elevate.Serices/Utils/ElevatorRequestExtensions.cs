using Elevate.Models.Enums;
using Elevate.Models.Models;

namespace Elevate.Serices.Utils
{
    public static class ElevatorRequestExtensions
    {
        
        public static ElevatorDirectionType GetDirection(this ElevatorRequest request)
        {
            if (request.To > request.From)
            {
                return ElevatorDirectionType.Up;
            }
            
            return ElevatorDirectionType.Down;
        }
    }
}
