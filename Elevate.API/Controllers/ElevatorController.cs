using Elevate.Models.Models;
using Elevate.Serices.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace Elevate.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElevatorController : ControllerBase
    {
        private readonly IElevatorManager _elevatorManager;

        public ElevatorController(IElevatorManager elevatorManager)
        {
            _elevatorManager = elevatorManager;
        }


        [HttpPost("loop")]
        public async Task Loop(CancellationToken cancellationToken)
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));

                var request = GenerateRandomRequest();

                await _elevatorManager.RequestElevator(request, cancellationToken);
            }
        }

        private ElevatorRequest GenerateRandomRequest()
        {
            ElevatorRequest request;

            do
            {
                request = new ElevatorRequest
                {
                    From = RandomNumberGenerator.GetInt32(1, 11),
                    To = RandomNumberGenerator.GetInt32(1, 11),
                };
            }
            while (request.From == request.To);

            return request;
        }
    }
}
