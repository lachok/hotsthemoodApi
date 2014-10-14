using System;
using hotsthemoodApi.Models;
using hotsthemoodApi.Modules.HappinessQuery;
using MongoDB.Bson.Serialization.Attributes;

namespace hotsthemoodApi.Modules.Checkin
{
    public class Checkin
    {
        [BsonId]
        public string Id { get; set; }
        public string DeviceId { get; set; }
        public string LocationReferenceId { get; set; }
        public Mood Mood { get; set; }
        public DateTime Timestamp { get; set; }
        public Location Location { get; set; }

        public Checkin()
        {
            Timestamp = DateTime.Now;
        }
    }
}