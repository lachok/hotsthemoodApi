using System;
using AutoMapper;
using hotsthemoodApi.Modules.Checkin;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;

namespace hotsthemoodApi
{
    public class HotsthemoodApiBootstrapper : DefaultNancyBootstrapper
    {
        private const string RavenDbDataDirectory = @"App_Data/Data";

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            Mapper.CreateMap<CheckinRequest, CheckinDto>();
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            

            container.Register<IDocumentStore>(CreateRavenDocStore());
        }

        private IDocumentStore CreateRavenDocStore()
        {
            var store = new EmbeddableDocumentStore
            {
                DataDirectory = RavenDbDataDirectory,
                UseEmbeddedHttpServer = true,
                Conventions =
                {
                    DefaultQueryingConsistency = ConsistencyOptions.AlwaysWaitForNonStaleResultsAsOfLastWrite,
                    MaxNumberOfRequestsPerSession = 10000
                },
            };

            store.Configuration.Port = 10546;

            store.Initialize();

            return store;
        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);

            container
                .Register<IDocumentSession>(CreateRavenSession(container));

            container.Register<ICheckinRepository, CheckinRepository>();
            container.Register<CheckinModule>();
        }

        private IDocumentSession CreateRavenSession(TinyIoCContainer container)
        {
            var store = container.Resolve<IDocumentStore>();
            var session = store.OpenSession();
            return session;
        }

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            base.RequestStartup(container, pipelines, context);

            pipelines.AfterRequest.AddItemToEndOfPipeline(ctx =>
            {
                var documentSession = container.Resolve<IDocumentSession>();

                if (ctx.Response.StatusCode != HttpStatusCode.InternalServerError && documentSession.Advanced.HasChanges)
                {
                    documentSession.SaveChanges();
                }

                ctx.Response.WithHeaders(
                    new Tuple<string, string>("Access-Control-Allow-Origin", "*"),
                    new Tuple<string, string>("Access-Control-Allow-Methods", "PUT, GET"),
                    new Tuple<string, string>("Access-Control-Allow-Headers", "Content-Type")
                );

                documentSession.Dispose();
            });
        }
    }
}