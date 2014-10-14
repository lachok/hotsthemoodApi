using hotsthemoodApi.Modules.Checkin;
using hotsthemoodApi.Modules.HappinessQuery;

namespace hotsthemoodApi.Contracts
{
    public interface ICheckinRepository : IRepository<Checkin>
    {
        RatedLocation[] GetRatedLocations(Location[] locations);
        Checkin[] GetAllByDeviceId(string deviceId);

    }
}