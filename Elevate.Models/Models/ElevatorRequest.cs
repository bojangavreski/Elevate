namespace Elevate.Models.Models
{
    public class ElevatorRequest
    {
        public int From { get; set; }

        public int To { get; set; }

        public bool IsHandled { get; set; } = false;

        public Guid Uid { get; set; } = Guid.NewGuid();
    }
}
