using AspNet.Identity.RavenDB.Entities;
using AspNet.Identity.RavenDB.Stores;
using Microsoft.AspNet.Identity;
using Raven.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AspNet.Identity.RavenDB.Tests.Stores
{
    public class IdentityUserLoginStoreFacts : TestBase
    {
        [Fact]
        public async Task Add_Should_Add_New_Login_If_User_Exists()
        {
            const string userName = "Tugberk";
            const string loginProvider = "Twitter";
            const string providerKey = "12345678";

            using (IDocumentStore store = CreateEmbeddableStore())
            {
                string userId;
                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IUserLoginStore<IdentityUser, string> userLoginStore = new IdentityUserStore<IdentityUser>(ses);
                    IdentityUser user = new IdentityUser(userName);
                    await ses.StoreAsync(user);
                    await ses.SaveChangesAsync();
                    userId = user.Id;
                }

                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IUserLoginStore<IdentityUser, string> userLoginStore = new IdentityUserStore<IdentityUser>(ses);
                    IdentityUser user = await ses.LoadAsync<IdentityUser>(userId);

                    // Act
                    UserLoginInfo loginToAdd = new UserLoginInfo(loginProvider, providerKey);
                    await userLoginStore.AddLoginAsync(user, loginToAdd);
                    await ses.SaveChangesAsync();

                    // Assert
                    IdentityUserLogin foundLogin = await ses.LoadAsync<IdentityUserLogin>(IdentityUserLogin.GenerateKey(loginProvider, providerKey));
                    Assert.Equal(1, user.Logins.Count());
                    Assert.NotNull(foundLogin);
                }
            }
        }

        [Fact]
        public async Task Add_Should_Add_New_Login_Just_After_UserManager_CreateAsync_Get_Called()
        {
            const string userName = "Tugberk";
            const string loginProvider = "Twitter";
            const string providerKey = "12345678";

            using (IDocumentStore store = CreateEmbeddableStore())
            {
                string userId;
                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IdentityUserStore<IdentityUser> userStore = new IdentityUserStore<IdentityUser>(ses);
                    UserManager<IdentityUser> userManager = new UserManager<IdentityUser>(userStore);

                    IdentityUser user = new IdentityUser(userName);
                    UserLoginInfo loginToAdd = new UserLoginInfo(loginProvider, providerKey);
                    await userManager.CreateAsync(user);
                    await userManager.AddLoginAsync(user.Id, loginToAdd);
                    await ses.SaveChangesAsync();
                    userId = user.Id;
                }

                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IUserLoginStore<IdentityUser, string> userLoginStore = new IdentityUserStore<IdentityUser>(ses);
                    IdentityUser user = await ses.LoadAsync<IdentityUser>(userId);
                    IdentityUserLogin foundLogin = await ses.LoadAsync<IdentityUserLogin>(IdentityUserLogin.GenerateKey(loginProvider, providerKey));

                    // Assert
                    Assert.Equal(1, user.Logins.Count());
                    Assert.NotNull(foundLogin);
                }
            }
        }

        [Fact]
        public async Task FindAsync_Should_Find_The_User_If_Login_Exists()
        {
            const string userName = "Tugberk";
            const string loginProvider = "Twitter";
            const string providerKey = "12345678";

            using (IDocumentStore store = CreateEmbeddableStore())
            {
                // Arrange
                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IUserLoginStore<IdentityUser, string> userLoginStore = new IdentityUserStore<IdentityUser>(ses);
                    IdentityUser user = new IdentityUser(userName);
                    await ses.StoreAsync(user);
                    IdentityUserLogin userLogin = new IdentityUserLogin(user.Id, new UserLoginInfo(loginProvider, providerKey));
                    user.Logins.Add(userLogin);
                    await ses.StoreAsync(userLogin);
                    await ses.SaveChangesAsync();
                }

                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IUserLoginStore<IdentityUser, string> userLoginStore = new IdentityUserStore<IdentityUser>(ses);

                    // Act
                    UserLoginInfo loginInfo = new UserLoginInfo(loginProvider, providerKey);
                    IdentityUser foundUser = await userLoginStore.FindAsync(loginInfo);

                    // Assert
                    Assert.NotNull(foundUser);
                    Assert.Equal(userName, foundUser.UserName);
                }
            }
        }
    }
}