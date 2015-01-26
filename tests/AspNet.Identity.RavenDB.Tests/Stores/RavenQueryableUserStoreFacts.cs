using AspNet.Identity.RavenDB.Entities;
using AspNet.Identity.RavenDB.Stores;
using Raven.Client;
using System.Threading.Tasks;
using Xunit;

namespace AspNet.Identity.RavenDB.Tests.Stores
{
    public class RavenQueryableUserStoreFacts : TestBase
    {
        [Fact]
        public async Task IdentityUserStore_Users_Should_Expose_IQueryable_Over_IRavenQueryable()
        {
            using (IDocumentStore store = CreateEmbeddableStore())
            {
                const string userName = "Tugberk";
                const string userNameToSearch = "TugberkUgurlu";

                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IdentityUser user = new IdentityUser(userName);
                    IdentityUser userToSearch = new IdentityUser(userNameToSearch);
                    await ses.StoreAsync(user);
                    await ses.StoreAsync(userToSearch);
                    await ses.SaveChangesAsync();
                }

                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    // Act
                    ses.Advanced.UseOptimisticConcurrency = true;
                    UserStore<IdentityUser> userStore = new UserStore<IdentityUser>(ses);
                    IdentityUser retrievedUser = await userStore.Users.FirstOrDefaultAsync(user => user.UserName == userNameToSearch);

                    // Assert
                    Assert.NotNull(retrievedUser);
                    Assert.Equal(userNameToSearch, retrievedUser.UserName);
                }
            }
        }
    }
}