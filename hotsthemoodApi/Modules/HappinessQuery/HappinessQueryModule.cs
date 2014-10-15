using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GoogleMapsApi.Entities.Places.Response;
using hotsthemoodApi.Contracts;
using hotsthemoodApi.ExternalApi;
using hotsthemoodApi.ModuleExtensions;
using hotsthemoodApi.Modules.Checkin;
using Nancy;

namespace hotsthemoodApi.Modules.HappinessQuery
{
    public class HappinessQueryModule : NancyModule
    {
        private readonly ICheckinRepository _checkinRepository;
        private readonly IGooglePlacesApi _googleApi;

        public HappinessQueryModule(ICheckinRepository checkinRepository, IGooglePlacesApi googleApi)
        {
            _checkinRepository = checkinRepository;
            _googleApi = googleApi;

            Put["/happinessquery/"] = _ => this.RunHandler<HappinessQueryRequest, HappinessQueryResponse>(QueryCheckins);
        }

        private HappinessQueryResponse QueryCheckins(HappinessQueryRequest query)
        {
            var results =_googleApi.GetPlacesByLocation(query.Latitude, query.Longitude);
            var situations = Mapper.Map<IList<Result>, IList<Location>>(results);
            var ratedLocations = _checkinRepository.GetRatedLocations(situations.ToArray());
            return new HappinessQueryResponse()
                {
                    Locations = ratedLocations
                };
        }
    }
}