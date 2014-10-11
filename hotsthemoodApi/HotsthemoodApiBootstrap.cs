using AutoMapper;
using hotsthemoodApi.Modules.Checkin;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;

namespace hotsthemoodApi
{
    public class HotsthemoodApiBootstrap : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            Mapper.CreateMap<CheckinRequest, CheckinDto>();
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            //var store = new EmbeddableDocumentStore()
            //{
            //    ConnectionStringName = "RavenDB"
            //};

            //store.Initialize();

            //container.Register(store);
        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);

            //var store = container.Resolve();
            //var documentSession = store.OpenSession();

            //container.Register(documentSession);
        }

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            base.RequestStartup(container, pipelines, context);

            pipelines.AfterRequest.AddItemToEndOfPipeline((ctx) =>
            {
                //var documentSession = container.Resolve();

                //if (ctx.Response.StatusCode != HttpStatusCode.InternalServerError)
                //{
                //    documentSession.SaveChanges();
                //}

                //documentSession.Dispose();
            });
        }
    }
}