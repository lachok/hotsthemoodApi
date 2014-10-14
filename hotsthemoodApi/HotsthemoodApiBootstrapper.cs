using System;
using AutoMapper;
using hotsthemoodApi.Contracts;
using hotsthemoodApi.Modules.Checkin;
using MongoDB.Driver;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;

namespace hotsthemoodApi
{
    public class HotsthemoodApiBootstrapper : DefaultNancyBootstrapper
    {
        private const string MongoDbConnection = "mongodb://localhost:27017";

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            Mapper.CreateMap<CheckinRequest, Checkin>();
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            var db = GetMongoDatabase(MongoDbConnection);

            container.Register(GetMongoCollection<Checkin>(db, "checkins"));
        }

        private MongoCollection<T> GetMongoCollection<T>(MongoDatabase db, string collectionName)
        {
            MongoCollection<T> collection = db.GetCollection<T>(collectionName);
            return collection;
        }

        private MongoDatabase GetMongoDatabase(string connection)
        {
            var client = new MongoClient(connection);

            var server = client.GetServer();
            var database = server.GetDatabase("Contacts", SafeMode.True);
            return database;
        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);

            container.Register<ICheckinRepository, CheckinRepository>();
            container.Register<CheckinModule>();
        }

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            base.RequestStartup(container, pipelines, context);

            pipelines.AfterRequest.AddItemToEndOfPipeline(ctx => ctx.Response.WithHeaders(
                new Tuple<string, string>("Access-Control-Allow-Origin", "*"),
                new Tuple<string, string>("Access-Control-Allow-Methods", "PUT, GET"),
                new Tuple<string, string>("Access-Control-Allow-Headers", "Content-Type")
                ));
        }
    }
}