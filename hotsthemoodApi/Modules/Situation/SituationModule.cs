using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GoogleMapsApi.Entities.Places.Response;
using hotsthemoodApi.ModuleExtensions;
using hotsthemoodApi.Modules.HappinessQuery;
using Nancy;

namespace hotsthemoodApi.Modules.Situation
{
    public class SituationModule : NancyModule
    {
        private readonly IGooglePlacesApi _api;

        public SituationModule(IGooglePlacesApi api)
        {
            _api = api;

            Put["/situation/"] = _ => this.RunHandler<SituationRequest, SituationResponse>(QuerySituation);
        }

        private SituationResponse QuerySituation(SituationRequest request)
        {

            var results = _api.GetPlacesByLocation(request.Latitude, request.Longitude);
            var situations =  AutoMapper.Mapper.Map<IList<Result>, IList<Location>>(results);
            return new SituationResponse()
            {
                Locations = situations.ToArray()
            };
        }

    }

    public class SituationResponse
    {
        public Location[] Locations { get; set; }
    }

    public class SituationRequest
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}