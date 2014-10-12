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
            var checkinDto = Mapper.Map<CheckinDto>(checkinRequest);
            _checkinRepository.Insert(checkinDto);

            return new CheckinResponse();
        }
    }

    public interface ICheckinRepository
    {
        void Insert(CheckinDto checkinDto);
        RatedLocationDto[] GetRatedLocations(LocationDto[] locations);
    }

    public class CheckinRepository : ICheckinRepository
    {
        private readonly IDocumentSession _session;

        public CheckinRepository(IDocumentSession session)
        {
            _session = session;
        }

        public void Insert(CheckinDto checkinDto)
        {
            _session.Store(checkinDto);
        }

        public RatedLocationDto[] GetRatedLocations(LocationDto[] locations)
        {
            var checkins = _session.Advanced.LuceneQuery<CheckinDto>()
                .WhereIn(x => x.LocationReferenceId, locations.Select(l => l.reference))
                .ToList();

            var locationRatings = new Dictionary<string, int>();

            foreach (var location in locations)
            {
                if (!locationRatings.ContainsKey(location.reference))
                {
                    locationRatings.Add(location.reference, 0);
                }

                var checkin = checkins.FirstOrDefault(c => c.LocationReferenceId == location.reference);

                if(checkin == null)
                    continue;
                
                if (checkin.Mood == Mood.Happy)
                    locationRatings[location.reference]++;
                else
                    locationRatings[location.reference]--;
            }

            return (from locationReference in locationRatings.Keys
                    join location in locations on locationReference equals location.reference
                    select new RatedLocationDto
                    {
                        name = location.name,
                        photoUrl = location.photoUrl,
                        reference = location.reference,
                        vicinity = location.vicinity,
                        Rating = locationRatings[locationReference]
                    }).ToArray();

        }
    }
}