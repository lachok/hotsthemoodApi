using hotsthemoodApi.Models;

namespace hotsthemoodApi.Modules.Checkin
{
    public class CheckinDto
    {
        public int Id { get; set; }
        public string DeviceId { get; set; }
        public string LocationReferenceId { get; set; }
        public Mood Mood { get; set; }
    }
}