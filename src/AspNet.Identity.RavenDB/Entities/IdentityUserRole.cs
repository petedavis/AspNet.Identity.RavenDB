using System;
using Microsoft.AspNet.Identity;
using Raven.Imports.Newtonsoft.Json;

namespace AspNet.Identity.RavenDB.Entities
{
    public class IdentityUserRole : IRole<string>
    {
        [JsonConstructor]
        public IdentityUserRole(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            Id = GenerateKey(name);
            Name = name;
        }

        public string Id { get; private set; }
        public string Name { get; set; }

        internal static string GenerateKey(string roleName)
        {
            return string.Format(Constants.IdentityUserRolesKeyTemplate, roleName);
        }
    }
}