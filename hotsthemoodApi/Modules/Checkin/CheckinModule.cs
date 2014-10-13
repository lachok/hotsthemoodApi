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
                .WhereIn(x => x.LocationReferenceId, locations.Select(l => l.Reference))
                .ToList();

            var locationRatings = new Dictionary<string, int>();

            foreach (var location in locations)
            {
                if (!locationRatings.ContainsKey(location.Reference))
                {
                    locationRatings.Add(location.Reference, 0);
                }

                var checkin = checkins.FirstOrDefault(c => c.LocationReferenceId == location.Reference);

                if(checkin == null)
                    continue;
                
                if (checkin.Mood == Mood.Happy)
                    locationRatings[location.Reference]++;
                else
                    locationRatings[location.Reference]--;
            }

            return (from locationReference in locationRatings.Keys
                    join location in locations on locationReference equals location.Reference
                    select new RatedLocationDto
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
    }
}