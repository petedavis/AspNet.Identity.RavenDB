using System.Reflection;
using System.Web.Mvc;
using AspNet.Identity.RavenDB.Sample.Mvc.Infrastructure.AutoMapper;
using Autofac;
using Autofac.Integration.Mvc;
using Microsoft.Owin;
using Owin;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Extensions;
using Raven.Client.UniqueConstraints;

[assembly: OwinStartupAttribute(typeof(AspNet.Identity.RavenDB.Sample.Mvc.Startup))]
namespace AspNet.Identity.RavenDB.Sample.Mvc
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);


            const string RavenDefaultDatabase = "AspNetIdentitySample";
            ContainerBuilder builder = new ContainerBuilder();
            builder.Register(c =>
            {
                var store = new DocumentStore
                {
                    Url = "http://localhost:8080",
                    DefaultDatabase = RavenDefaultDatabase
                };

                store.RegisterListener(new UniqueConstraintsStoreListener());
                store.Initialize();

                store.DatabaseCommands.EnsureDatabaseExists(RavenDefaultDatabase);

                store.InitializeWithDefaults();

                return store;

            }).As<IDocumentStore>().SingleInstance();
            
            builder.Register(c =>
            {
                var session = c.Resolve<IDocumentStore>().OpenAsyncSession();
                session.Advanced.UseOptimisticConcurrency = true;
                return session;
            }).As<IAsyncDocumentSession>().InstancePerHttpRequest();
            
            builder.RegisterControllers(Assembly.GetExecutingAssembly());


            app.UseAutofacMiddleware(builder.Build());
            app.UseAutofacMvc();

            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationRoleManager>(ApplicationRoleManager.Create);

            AutoMapperConfiguration.Configure();
        }
    }
}
