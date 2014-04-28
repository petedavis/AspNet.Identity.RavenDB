using System.ComponentModel.Composition.Hosting;
using System.Security.Claims;
using AspNet.Identity.RavenDB.Entities;
using Autofac;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Raven.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Raven.Client.Extensions;
using Raven.Client.Document;
using Autofac.Integration.Mvc;
using AspNet.Identity.RavenDB.Stores;
using AspNet.Identity.RavenDB.Sample.Mvc.Models;
using Microsoft.AspNet.Identity;
using System.Reflection;
using Raven.Client.Indexes;

namespace AspNet.Identity.RavenDB.Sample.Mvc
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

        }
    }

    internal static class DocumentStoreExtentions
    {
        public static void InitializeWithDefaults(this IDocumentStore documentStore)
        {
            documentStore.InitializeWithDefaults(true);
        }

        public static void InitializeWithDefaults(this IDocumentStore documentStore, bool isDataToBeSeeded)
        {
            if (documentStore == null) throw new ArgumentNullException("documentStore");

            // Default initializtion;
            documentStore.Initialize();

            // Create our Seed Data (if required).
            // NOTE: This would be handled differently if it was a -real- production system.
            //       Like, wrapping this called in a #if RELEASE #endif, for example.
            if (isDataToBeSeeded)
            {
                HelperUtilities.CreateSeedData(documentStore);
            }

            // Now lets check to make sure there are now errors.
            //documentStore.AssertDocumentStoreErrors();
        }
    }

    public static class HelperUtilities
    {
        public static void CreateSeedData(IDocumentStore documentStore)
        {
            if (documentStore == null) throw new ArgumentNullException("documentStore");
            
            using (var documentSession = documentStore.OpenAsyncSession())
            {
                documentSession.Advanced.UseOptimisticConcurrency = true;
                var userManager = new ApplicationUserManager(new RavenUserStore<ApplicationUser>(documentSession, false));
                var roleManager = new ApplicationRoleManager(new RoleStore<RavenRole>(documentSession));
                const string name = "admin@admin.com";
                const string password = "Admin@123456";
                const string roleName = "Admin";


                //Create Role Admin if it does not exist
                var role = roleManager.FindByName(roleName);
                if (role == null)
                {
                    role = new RavenRole(roleName);
                    var roleresult = roleManager.Create(role);
                }

                var user = userManager.FindByName(name);
                if (user == null)
                {
                    user = new ApplicationUser(name, name);
                    var result = userManager.Create(user, password);
                    result = userManager.SetLockoutEnabled(user.Id, false);
                }

                var roleClaim = new RavenUserClaim(ClaimTypes.Role, roleName);

                // Add user admin to Role Admin if not already added
                if (!user.Claims.Contains(roleClaim))
                {
                    user.AddClaim(roleClaim);
                    userManager.UpdateAsync(user).Wait();
                }

                documentSession.SaveChangesAsync().Wait();

                // Make sure all our indexes are not stale.
                //documentStore.WaitForStaleIndexesToComplete();
            }
        }
    }
}
