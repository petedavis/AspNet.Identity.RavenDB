using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using AspNet.Identity.RavenDB.Entities;
using AspNet.Identity.RavenDB.Stores;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Raven.Client;
using Raven.Imports.Newtonsoft.Json;

namespace AspNet.Identity.RavenDB.Sample.Mvc.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : RavenUser
    {
        [JsonConstructor]
        public ApplicationUser(string userName)
            : base(userName)
        {
        }

        public ApplicationUser(string userName, string email)
            : base(userName, email)
        {
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    
}