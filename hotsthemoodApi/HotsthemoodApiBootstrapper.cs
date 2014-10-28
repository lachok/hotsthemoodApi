using System;
using System.Configuration;
using AutoMapper;
using EventbriteApiClient;
using EventbriteApiClient.Entities;
using GoogleMapsApi.Entities.Places.Response;
using hotsthemoodApi.Contracts;
using hotsthemoodApi.Modules.Auth;
using hotsthemoodApi.Modules.Checkin;
using hotsthemoodApi.Modules.HappinessQuery;
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
            
            SetupAutomapper();

        }

        private void SetupAutomapper()
        {
            {
                Mapper.CreateMap<CheckinRequest, Checkin>();
                Mapper.CreateMap<Result, Location>()
                    .ForMember(dest => dest.PhotoUrl,
                        opts => opts.MapFrom(src => src.Icon)).
                    ForMember(dest => dest.Reference,
                        opts => opts.MapFrom(src => src.ID));

                Mapper.CreateMap<Event, Location>()
                    .ForMember(dest => dest.Name,
                        opts => opts.MapFrom(src => src.Name.Text))
                    .ForMember(dest => dest.Reference,
                        opts => opts.MapFrom(src => src.Id))
                    .ForMember(dest => dest.PhotoUrl,
                        opts => opts.MapFrom(src => src.LogoUrl));

            }
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
            var databasename = MongoUrl.Create(connection).DatabaseName;

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
            container.Register(new EventBriteApiContext("I4SKDVPHLU6PFMF74NQG"));
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