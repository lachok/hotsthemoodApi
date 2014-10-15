﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GoogleMapsApi.Entities.Common;
using GoogleMapsApi.Entities.Places.Request;
using GoogleMapsApi.Entities.Places.Response;

namespace hotsthemoodApi.Modules.Situation
{
    public interface IGooglePlacesApi
    {
        List<Result> GetPlacesByLocation(double lat, double lng);
    }

    public class GooglePlacesApi : IGooglePlacesApi
    {
        private const string ApiKey = "AIzaSyCSdO-ewgYLb2prwz8h6k1mq3NfjvG3ThY";
        private const string PlaceTypes = "food|bar|night_club";

        public List<Result> GetPlacesByLocation(double lat, double lng)
        {
            var request = new PlacesRequest()
            {
                ApiKey = ApiKey,
                Location = new Location(lat, lng),
                RankBy = RankBy.Distance,
                Sensor = true,
                Types = PlaceTypes
            };

            PlacesResponse response = GoogleMapsApi.GoogleMaps.Places.Query(request);
            return response.Results.ToList();
        }
    }
}