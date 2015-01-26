using AspNet.Identity.RavenDB.Entities;
using AspNet.Identity.RavenDB.Stores;
using Microsoft.AspNet.Identity;
using Raven.Client;
using System.Threading.Tasks;
using Xunit;

namespace AspNet.Identity.RavenDB.Tests.Stores
{
    public class IdentityUserTwoFactorStoreFacts : TestBase
    {
        [Fact]
        public async Task GetTwoFactorEnabledAsync_Should_Get_User_IsTwoFactorEnabled_Value()
        {
            using (IDocumentStore store = CreateEmbeddableStore())
            {
                const string userName = "Tugberk";
                const string userId = "IdentityUsers/1";

                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    var user = new IdentityUser(userName) {TwoFactorEnabled = true};
                    await ses.StoreAsync(user);
                    await ses.SaveChangesAsync();
                }

                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    // Act
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IdentityUser user = await ses.LoadAsync<IdentityUser>(userId);
                    IUserTwoFactorStore<IdentityUser, string> userTwoFactorStore = new UserStore<IdentityUser>(ses);
                    bool isTwoFactorEnabled = await userTwoFactorStore.GetTwoFactorEnabledAsync(user);

                    // Assert
                    Assert.True(isTwoFactorEnabled);
                }
            }
        }

        [Fact]
        public async Task SetTwoFactorEnabledAsync_Should_Set_IsTwoFactorEnabled_Value()
        {
            using (IDocumentStore store = CreateEmbeddableStore())
            {
                const string userName = "Tugberk";
                const string userId = "IdentityUsers/1";

                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    var user = new IdentityUser(userName) {TwoFactorEnabled = true};
                    await ses.StoreAsync(user);
                    await ses.SaveChangesAsync();
                }

                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    // Act
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IdentityUser user = await ses.LoadAsync<IdentityUser>(userId);
                    IUserTwoFactorStore<IdentityUser, string> userTwoFactorStore = new UserStore<IdentityUser>(ses);
                    await userTwoFactorStore.SetTwoFactorEnabledAsync(user, enabled: true);

                    // Assert
                    Assert.True(user.TwoFactorEnabled);
                }
            }
        }
    }
}
