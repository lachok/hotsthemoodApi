using hotsthemoodApi.Models;

namespace hotsthemoodApi.Modules.Checkin
{
    public class CheckinRequest
    {
        public string DeviceId { get; set; }
        public string LocationReferenceId { get; set; }
        public Mood Mood { get; set; }
    }

    public class CheckinDto
    {
        public string DeviceId { get; set; }
        public string LocationReferenceId { get; set; }
        public Mood Mood { get; set; }
    }
}