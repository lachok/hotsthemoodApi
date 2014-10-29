using System;
using System.Collections.Generic;
using System.Linq;
using EventbriteApiClient;
using EventbriteApiClient.DTOs;
using EventbriteApiClient.Entities;
using hotsthemoodApi.ModuleExtensions;
using hotsthemoodApi.Modules.HappinessQuery;
using Nancy;

namespace hotsthemoodApi.Modules.Search
{
    public class SearchModule : NancyModule
    {
        private readonly EventBriteApiContext _context;


        public SearchModule(EventBriteApiContext context)
        {
            _context = context;
            Put["/search/"] = _ => this.RunHandler<SearchRequest, SearchResponse>(Search);
        }


        private SearchResponse Search(SearchRequest request)
        {

            var eventSearchRequest = new EventSearchRequest()
            {
               Latitude = request.Latitude,
               Longitude = request.Longitude,
               DateRangeStart = DateTime.Now,
               DateRangeEnd = DateTime.Now.AddDays(1),
               Range = "10mi"
             
            };

            var events = _context.GetEvents(eventSearchRequest);
            var locations = AutoMapper.Mapper.Map<IEnumerable<Event>, IEnumerable<Location>>(events);

            return new SearchResponse()
            {
                Events = locations.ToArray()
            };
        }

        
    }
}