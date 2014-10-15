using System;
using System.Collections.Generic;
using System.Linq;
using hotsthemoodApi.Contracts;
using hotsthemoodApi.ExternalApi;
using hotsthemoodApi.Modules.HappinessQuery;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;

namespace hotsthemoodApi.Modules.Checkin
{
    public class CheckinRepository : ICheckinRepository
    {
        private readonly MongoCollection<Checkin> _checkins;

        public CheckinRepository(MongoCollection<Checkin> checkins)
        {
            _checkins = checkins;
        }

        public RatedLocation[] GetRatedLocations(Location[] locations)
        {
            var checkins = _checkins.AsQueryable()
                    .Where(c => locations.Contains(c.Location));

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
            var query = new QueryDocument("DeviceId", deviceId);
            var checkins = _checkins.Find(query);

            return checkins.OrderByDescending(x => x.Timestamp)
                .ToArray();
        }

        public IEnumerable<Checkin> GetAll()
        {
            return _checkins.FindAll();
        }

        public Checkin Get(string id)
        {
            IMongoQuery query = Query.EQ("_id", id);
            return _checkins.Find(query).FirstOrDefault();
        }

        public Checkin Add(Checkin item)
        {
            item.Id = ObjectId.GenerateNewId().ToString();
            _checkins.Insert(item);
            return item;
        }

        public bool Remove(string id)
        {
            IMongoQuery query = Query.EQ("_id", id);
            WriteConcernResult result = _checkins.Remove(query);
            return result.DocumentsAffected == 1;
        }

        public bool Update(string id, Checkin item)
        {
            throw new NotImplementedException();
        }
    }
}