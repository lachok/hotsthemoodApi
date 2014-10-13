using AutoMapper;
using hotsthemoodApi.ModuleExtensions;
using hotsthemoodApi.Modules.Checkin;
using Nancy;

namespace hotsthemoodApi.Modules.HappinessQuery
{
    public class HappinessQueryModule : NancyModule
    {
        private readonly ICheckinRepository _checkinRepository;

        public HappinessQueryModule(ICheckinRepository checkinRepository)
        {
            _checkinRepository = checkinRepository;

            Put["/happinessquery/"] = _ => this.RunHandler<HappinessQueryRequest, HappinessQueryResponse>(QueryCheckins);
        }

        private HappinessQueryResponse QueryCheckins(HappinessQueryRequest query)
        {
            var ratedLocations = _checkinRepository.GetRatedLocations(query.Locations);
            return new HappinessQueryResponse()
                {
                    Locations = ratedLocations
                };
        }
    }

    public class HappinessQueryResponse
    {
        public RatedLocationDto[] Locations { get; set; }
    }

    public class HappinessQueryRequest
    {
        public LocationDto[] Locations { get; set; }
    }

    public class LocationDto
    {
        public string Name { get; set; }
        public string Reference { get; set; }
        public string Vicinity { get; set; }
        public string PhotoUrl { get; set; }
    }

    public class RatedLocationDto : LocationDto
    {
        public int Rating { get; set; }
    }
}