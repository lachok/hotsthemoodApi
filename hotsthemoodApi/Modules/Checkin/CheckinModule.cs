using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using hotsthemoodApi.Models;
using hotsthemoodApi.ModuleExtensions;
using hotsthemoodApi.Modules.HappinessQuery;
using Nancy;
using Raven.Client;

namespace hotsthemoodApi.Modules.Checkin
{
    public class CheckinModule : NancyModule
    {
        private readonly ICheckinRepository _checkinRepository;

        public CheckinModule(ICheckinRepository checkinRepository)
        {
            _checkinRepository = checkinRepository;

            Put["/checkin/"] = _ => this.RunHandler<CheckinRequest, CheckinResponse>(CheckIn);
        }

        private CheckinResponse CheckIn(CheckinRequest checkinRequest)
        {
            _checkinRepository.Insert(checkinRequest.Checkin);

            return new CheckinResponse();
        }
    }

    public interface ICheckinRepository
    {
        void Insert(Checkin checkin);
        RatedLocation[] GetRatedLocations(Location[] locations);
        Checkin[] GetAllByDeviceId(string deviceId);
    }

    public class CheckinRepository : ICheckinRepository
    {
        private readonly IDocumentSession _session;

        public CheckinRepository(IDocumentSession session)
        {
            _session = session;
        }

        public void Insert(Checkin checkin)
        {
            _session.Store(checkin);
        }

        public RatedLocation[] GetRatedLocations(Location[] locations)
        {
            var checkins = _session.Advanced.LuceneQuery<Checkin>()
                .WhereIn(x => x.LocationReferenceId, locations.Select(l => l.Reference))
                .ToList();

            var locationRatings = new Dictionary<string, int>();

            foreach (var location in locations)
            {
                if (!locationRatings.ContainsKey(location.Reference))
                {
                    locationRatings.Add(location.Reference, 0);
                }

                var happyCheckins = checkins.Count(c => c.LocationReferenceId == location.Reference && c.Mood == Mood.Happy);
                var sadCheckins = checkins.Count(c => c.LocationReferenceId == location.Reference && c.Mood == Mood.Sad);

                locationRatings[location.Reference] = happyCheckins - sadCheckins;
            }

            return (from locationReference in locationRatings.Keys
                    join location in locations on locationReference equals location.Reference
                    select new RatedLocation
                    {
                        Name = location.Name,
                        PhotoUrl = location.PhotoUrl,
                        Reference = location.Reference,
                        Vicinity = location.Vicinity,
                        Rating = locationRatings[locationReference]
                    })
                    .OrderByDescending(t => t.Rating)
                    .ToArray();

        }

        public Checkin[] GetAllByDeviceId(string deviceId)
        {
            var checkins = _session.Query<Checkin>()
                .Where(x => x.DeviceId == deviceId)
                .OrderByDescending(x => x.Timestamp)
                .ToList();

            return checkins.ToArray();
        }
    }
}