using System;
using System.Configuration;
using System.Linq;
using AutoMapper;
using hotsthemoodApi.Contracts;
using hotsthemoodApi.Modules.Auth;
using hotsthemoodApi.Modules.Checkin;
using MongoDB.Driver;
using Nancy;
using Nancy.Authentication.Basic;
using Nancy.Bootstrapper;
using Nancy.Elmah;
using Nancy.TinyIoc;

namespace hotsthemoodApi
{
    public class HotsthemoodApiBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            Elmahlogging.Enable(pipelines, "elmah", new[] { "administrator" }, new[] { HttpStatusCode.NotFound, HttpStatusCode.InsufficientStorage, });

            Mapper.CreateMap<CheckinRequest, Checkin>();
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            var db = GetMongoDatabase();

            container.Register(GetMongoCollection<Checkin>(db, "checkins"));
            container.Register<IUserValidator, BasicUserValidator>();
        }

        private MongoCollection<T> GetMongoCollection<T>(MongoDatabase db, string collectionName)
        {
            MongoCollection<T> collection = db.GetCollection<T>(collectionName);
            return collection;
        }

        private MongoDatabase GetMongoDatabase()
        {
            
            var connection = ConfigurationManager.AppSettings["MONGOLAB_URI"];
            var databasename = connection.Split('/').Last();

            var client = new MongoClient(connection);

            var server = client.GetServer();
            var database = server.GetDatabase(databasename, SafeMode.True);
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

            BasicAuthentication.Enable(pipelines, new BasicAuthenticationConfiguration(
                container.Resolve<IUserValidator>(), "hotsthemood"
            ));

            pipelines.AfterRequest.AddItemToEndOfPipeline(ctx => ctx.Response.WithHeaders(
                new Tuple<string, string>("Access-Control-Allow-Origin", "*"),
                new Tuple<string, string>("Access-Control-Allow-Methods", "PUT, GET"),
                new Tuple<string, string>("Access-Control-Allow-Headers", "Content-Type")
                ));
        }
    }
}