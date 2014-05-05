using System.Runtime.Remoting;
using AspNet.Identity.RavenDB.Entities;
using AspNet.Identity.RavenDB.Stores;
using Microsoft.AspNet.Identity;
using Raven.Abstractions.Exceptions;
using Raven.Client;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AspNet.Identity.RavenDB.Tests.Stores
{
    public class IdentityUserEmailStoreFacts : TestBase
    {
        // FindByEmailAsync

        [Fact]
        public async Task FindByEmailAsync_Should_Return_The_Correct_User_If_Available()
        {
            const string userName = "Tugberk";
            const string userId = "IdentityUsers/1";
            const string email = "tugberk@example.com";

            using (IDocumentStore store = CreateEmbeddableStore())
            {
                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IdentityUser user = new IdentityUser(userName, email);
                    await ses.StoreAsync(user);
                    IdentityUserEmail userEmail = new IdentityUserEmail(email, user.Id);
                    await ses.StoreAsync(userEmail);
                    await ses.SaveChangesAsync();
                }

                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IUserEmailStore<IdentityUser> userEmailStore = new IdentityUserStore<IdentityUser>(ses);
                    IdentityUser user = await userEmailStore.FindByEmailAsync(email);

                    Assert.NotNull(user);
                    Assert.Equal(userId, user.Id);
                    Assert.Equal(userName, user.UserName);
                }
            }
        }

        [Fact]
        public async Task FindByEmailAsync_Should_Return_Null_If_User_Is_Not_Available()
        {
            const string userName = "Tugberk";
            const string email = "tugberk@example.com";
            const string emailToLookFor = "foobar@example.com";

            using (IDocumentStore store = CreateEmbeddableStore())
            {
                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IdentityUser user = new IdentityUser(userName, email);
                    await ses.StoreAsync(user);

                    IdentityUserEmail userEmail = new IdentityUserEmail(email, user.Id);
                    await ses.StoreAsync(userEmail);
                    await ses.SaveChangesAsync();
                }

                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IUserEmailStore<IdentityUser> userEmailStore = new IdentityUserStore<IdentityUser>(ses);
                    IdentityUser user = await userEmailStore.FindByEmailAsync(emailToLookFor);

                    Assert.Null(user);
                }
            }
        }

        // GetEmailAsync

        [Fact]
        public async Task GetEmailAsync_Should_Return_User_Email_If_Available()
        {
            const string userName = "Tugberk";
            const string userId = "IdentityUsers/1";
            const string email = "tugberk@example.com";

            using (IDocumentStore store = CreateEmbeddableStore())
            {
                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IdentityUser user = new IdentityUser(userName, email);
                    await ses.StoreAsync(user);
                    IdentityUserEmail userEmail = new IdentityUserEmail(email, user.Id);
                    await ses.StoreAsync(userEmail);
                    await ses.SaveChangesAsync();
                }

                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IUserEmailStore<IdentityUser> userEmailStore = new IdentityUserStore<IdentityUser>(ses);
                    IdentityUser user = await ses.LoadAsync<IdentityUser>(userId);
                    Assert.NotNull(user);
                    string userEmail = await userEmailStore.GetEmailAsync(user);

                    Assert.NotNull(userEmail);
                    Assert.Equal(email, userEmail);
                }
            }
        }

        [Fact]
        public async Task GetEmailAsync_Should_Return_Null_If_User_Email_Is_Not_Available()
        {
            const string userName = "Tugberk";
            const string userId = "IdentityUsers/1";

            using (IDocumentStore store = CreateEmbeddableStore())
            {
                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IdentityUser user = new IdentityUser(userName);
                    await ses.StoreAsync(user);
                    await ses.SaveChangesAsync();
                }

                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IUserEmailStore<IdentityUser> userEmailStore = new IdentityUserStore<IdentityUser>(ses);
                    IdentityUser identityUser = await ses.LoadAsync<IdentityUser>(userId);
                    Assert.NotNull(identityUser);
                    string userEmail = await userEmailStore.GetEmailAsync(identityUser);

                    Assert.Null(userEmail);
                }
            }
        }

        // GetEmailConfirmedAsync

        [Fact]
        public async Task GetEmailConfirmedAsync_Should_Return_True_If_Email_Confirmed()
        {
            const string userName = "Tugberk";
            const string userId = "IdentityUsers/1";
            const string email = "tugberk@example.com";

            using (IDocumentStore store = CreateEmbeddableStore())
            {
                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IdentityUser user = new IdentityUser(userName, email);
                    await ses.StoreAsync(user);
                    IdentityUserEmail userEmail = new IdentityUserEmail(email, user.Id);
                    userEmail.SetConfirmed();
                    await ses.StoreAsync(userEmail);
                    await ses.SaveChangesAsync();
                }

                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IUserEmailStore<IdentityUser> userEmailStore = new IdentityUserStore<IdentityUser>(ses);
                    IdentityUser IdentityUser = await ses.LoadAsync<IdentityUser>(userId);
                    bool isConfirmed = await userEmailStore.GetEmailConfirmedAsync(IdentityUser);

                    Assert.True(isConfirmed);
                }
            }
        }

        [Fact]
        public async Task GetEmailConfirmedAsync_Should_Return_False_If_Email_Is_Not_Confirmed()
        {
            const string userName = "Tugberk";
            const string userId = "IdentityUsers/1";
            const string email = "tugberk@example.com";

            using (IDocumentStore store = CreateEmbeddableStore())
            {
                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IdentityUser user = new IdentityUser(userName, email);
                    await ses.StoreAsync(user);
                    IdentityUserEmail userEmail = new IdentityUserEmail(email, user.Id);
                    await ses.StoreAsync(userEmail);
                    await ses.SaveChangesAsync();
                }

                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IUserEmailStore<IdentityUser> userEmailStore = new IdentityUserStore<IdentityUser>(ses);
                    IdentityUser IdentityUser = await ses.LoadAsync<IdentityUser>(userId);
                    bool isConfirmed = await userEmailStore.GetEmailConfirmedAsync(IdentityUser);

                    Assert.False(isConfirmed);
                }
            }
        }

        [Fact]
        public async Task GetEmailConfirmedAsync_Should_Throw_InvalidOperationException_If_Email_Is_Not_Available()
        {
            const string userName = "Tugberk";
            const string userId = "IdentityUsers/1";

            using (IDocumentStore store = CreateEmbeddableStore())
            {
                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IdentityUser user = new IdentityUser(userName) { UserName = userName };
                    await ses.StoreAsync(user);
                    await ses.SaveChangesAsync();
                }

                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IUserEmailStore<IdentityUser> userEmailStore = new IdentityUserStore<IdentityUser>(ses);
                    IdentityUser IdentityUser = await ses.LoadAsync<IdentityUser>(userId);
                    
                    await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                    {
                        bool isConfirmed = await userEmailStore.GetEmailConfirmedAsync(IdentityUser);
                    });
                }
            }
        }

        // SetEmailAsync

        [Fact]
        public async Task SetEmailAsync_Should_Set_The_Email_Correctly()
        {
            const string userName = "Tugberk";
            const string userId = "IdentityUsers/1";
            const string emailToSave = "tugberk@example.com";

            using (IDocumentStore store = CreateEmbeddableStore())
            {
                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IdentityUser user = new IdentityUser(userName) { UserName = userName };
                    await ses.StoreAsync(user);
                    await ses.SaveChangesAsync();
                }

                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IUserEmailStore<IdentityUser> userEmailStore = new IdentityUserStore<IdentityUser>(ses);
                    IdentityUser IdentityUser = await ses.LoadAsync<IdentityUser>(userId);
                    await userEmailStore.SetEmailAsync(IdentityUser, emailToSave);
                    await ses.SaveChangesAsync();
                }

                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    string keyToLookFor = IdentityUserEmail.GenerateKey(emailToSave);
                    IdentityUser IdentityUser = await ses.LoadAsync<IdentityUser>(userId);
                    IdentityUserEmail userEmail = await ses.LoadAsync<IdentityUserEmail>(keyToLookFor);

                    Assert.NotNull(userEmail);
                    Assert.Equal(emailToSave, IdentityUser.Email);
                    Assert.Equal(emailToSave, userEmail.Email);
                    Assert.Equal(userId, userEmail.UserId);
                }
            }
        }

        [Fact]
        public async Task SetEmailAsync_Should_Set_Email_And_SaveChangesAsync_Should_Throw_ConcurrencyException_If_The_Email_Already_Exists()
        {
            const string userName = "Tugberk";
            const string email = "tugberk@example.com";
            const string userName2 = "Tugberk2";
            string userId2;

            using (IDocumentStore store = CreateEmbeddableStore())
            {
                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IdentityUser user = new IdentityUser(userName, email);
                    await ses.StoreAsync(user);
                    IdentityUser user2 = new IdentityUser(userName2);
                    await ses.StoreAsync(user2);
                    userId2 = user2.Id;
                    IdentityUserEmail userEmail = new IdentityUserEmail(email, user.Id);
                    await ses.StoreAsync(userEmail);
                    await ses.SaveChangesAsync();
                }

                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IUserEmailStore<IdentityUser> userEmailStore = new IdentityUserStore<IdentityUser>(ses);
                    IdentityUser IdentityUser = await ses.LoadAsync<IdentityUser>(userId2);
                    await userEmailStore.SetEmailAsync(IdentityUser, email);

                    await Assert.ThrowsAsync<ConcurrencyException>(async () =>
                    {
                        await ses.SaveChangesAsync();
                    });
                }

                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    IdentityUser IdentityUser = await ses.LoadAsync<IdentityUser>(userId2);
                    Assert.Null(IdentityUser.Email);
                }
            }
        }

        // SetEmailConfirmedAsync

        [Fact]
        public async Task SetEmailConfirmedAsync_With_Confirmed_Param_True_Should_Set_The_Email_As_Confirmed_If_Not_Confirmed_Already()
        {
            const string userName = "Tugberk";
            const string userId = "IdentityUsers/1";
            const string email = "tugberk@example.com";

            using (IDocumentStore store = CreateEmbeddableStore())
            {
                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IdentityUser user = new IdentityUser(userName, email);
                    await ses.StoreAsync(user);
                    IdentityUserEmail userEmail = new IdentityUserEmail(email, user.Id);
                    await ses.StoreAsync(userEmail);
                    await ses.SaveChangesAsync();
                }

                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IUserEmailStore<IdentityUser> userEmailStore = new IdentityUserStore<IdentityUser>(ses);
                    IdentityUser IdentityUser = await ses.LoadAsync<IdentityUser>(userId);
                    await userEmailStore.SetEmailConfirmedAsync(IdentityUser, confirmed: true);
                    await ses.SaveChangesAsync();
                }

                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    string keyToLookFor = IdentityUserEmail.GenerateKey(email);
                    IdentityUserEmail userEmail = await ses.LoadAsync<IdentityUserEmail>(keyToLookFor);

                    Assert.NotNull(userEmail.ConfirmationRecord);
                    Assert.NotEqual(default(DateTimeOffset), userEmail.ConfirmationRecord.ConfirmedOn);
                }
            }
        }

        [Fact]
        public async Task SetEmailConfirmedAsync_With_Confirmed_Param_False_Should_Set_The_Email_As_Not_Confirmed_If_Confirmed_Already()
        {
            const string userName = "Tugberk";
            const string userId = "IdentityUsers/1";
            const string email = "tugberk@example.com";

            using (IDocumentStore store = CreateEmbeddableStore())
            {
                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IdentityUser user = new IdentityUser(userName, email);
                    await ses.StoreAsync(user);
                    IdentityUserEmail userEmail = new IdentityUserEmail(email, user.Id);
                    userEmail.SetConfirmed();
                    await ses.StoreAsync(userEmail);
                    await ses.SaveChangesAsync();
                }

                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IUserEmailStore<IdentityUser> userEmailStore = new IdentityUserStore<IdentityUser>(ses);
                    IdentityUser IdentityUser = await ses.LoadAsync<IdentityUser>(userId);
                    await userEmailStore.SetEmailConfirmedAsync(IdentityUser, confirmed: false);
                    await ses.SaveChangesAsync();
                }

                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    string keyToLookFor = IdentityUserEmail.GenerateKey(email);
                    IdentityUserEmail userEmail = await ses.LoadAsync<IdentityUserEmail>(keyToLookFor);

                    Assert.Null(userEmail.ConfirmationRecord);
                }
            }
        }

        [Fact]
        public async Task SetEmailConfirmedAsync_Should_Throw_InvalidOperationException_If_User_Email_Property_Is_Not_Available()
        {
            const string userName = "Tugberk";
            const string userId = "IdentityUsers/1";
            const string email = "tugberk@example.com";

            using (IDocumentStore store = CreateEmbeddableStore())
            {
                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IdentityUser user = new IdentityUser(userName);
                    await ses.StoreAsync(user);
                    IdentityUserEmail userEmail = new IdentityUserEmail(email, user.Id);
                    await ses.StoreAsync(userEmail);
                    await ses.SaveChangesAsync();
                }

                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IUserEmailStore<IdentityUser> userEmailStore = new IdentityUserStore<IdentityUser>(ses);
                    IdentityUser IdentityUser = await ses.LoadAsync<IdentityUser>(userId);

                    await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                    {
                        await userEmailStore.SetEmailConfirmedAsync(IdentityUser, confirmed: true);
                    });
                }
            }
        }

        [Fact]
        public async Task SetEmailConfirmedAsync_Should_Throw_InvalidOperationException_If_User_Email_Property_Is_Available_But_UserEmail_Document_Not()
        {
            const string userName = "Tugberk";
            const string userId = "IdentityUsers/1";
            const string email = "tugberk@example.com";

            using (IDocumentStore store = CreateEmbeddableStore())
            {
                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IdentityUser user = new IdentityUser(userName, email);
                    await ses.StoreAsync(user);
                    await ses.SaveChangesAsync();
                }

                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IUserEmailStore<IdentityUser> userEmailStore = new IdentityUserStore<IdentityUser>(ses);
                    IdentityUser IdentityUser = await ses.LoadAsync<IdentityUser>(userId);

                    await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                    {
                        await userEmailStore.SetEmailConfirmedAsync(IdentityUser, confirmed: true);
                    });
                }
            }
        }
    }
}