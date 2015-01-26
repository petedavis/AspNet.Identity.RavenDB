using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspNet.Identity.RavenDB.Entities;
using Microsoft.AspNet.Identity;
using Raven.Client;

namespace AspNet.Identity.RavenDB.Stores
{
    public class RoleStore<TRole> : IQueryableRoleStore<TRole, string> where TRole : class, IRole<string>
    {
        private readonly IAsyncDocumentSession _documentSession;

        public RoleStore(IAsyncDocumentSession documentSession)
        {
            if (documentSession == null) throw new ArgumentNullException("documentSession");
            _documentSession = documentSession;
        }

        public void Dispose()
        {
            
        }

        public async Task CreateAsync(TRole role)
        {
            if (role == null) throw new ArgumentNullException("role");

            await _documentSession.StoreAsync(role).ConfigureAwait(false);
            await _documentSession.SaveChangesAsync().ConfigureAwait(false);
        }

        public Task UpdateAsync(TRole role)
        {
            if (role == null) throw new ArgumentNullException("role");

            return _documentSession.SaveChangesAsync();
        }

        public Task DeleteAsync(TRole role)
        {
            if (role == null) throw new ArgumentNullException("role");
            _documentSession.Delete(role);
            return _documentSession.SaveChangesAsync();
        }

        public Task<TRole> FindByIdAsync(string roleId)
        {
            if (roleId == null) throw new ArgumentNullException("roleId");

            return _documentSession.LoadAsync<TRole>(roleId);
        }

        public Task<TRole> FindByNameAsync(string roleName)
        {
            if (roleName == null) throw new ArgumentNullException("roleName");

            return _documentSession.LoadAsync<TRole>(IdentityRole.GenerateKey(roleName));
        }

        public IQueryable<TRole> Roles
        {
            get
            {
                return _documentSession.Query<TRole>();
            }
        }
    }
}
