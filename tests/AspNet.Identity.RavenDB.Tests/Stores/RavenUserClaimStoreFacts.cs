﻿using AspNet.Identity.RavenDB.Entities;
using AspNet.Identity.RavenDB.Stores;
using Microsoft.AspNet.Identity;
using Raven.Client;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace AspNet.Identity.RavenDB.Tests.Stores
{
    public class IdentityUserClaimStoreFacts : TestBase
    {
        [Fact]
        public async Task GetUserClaims_Should_Retrieve_Correct_Claims_For_User()
        {
            string userName = "Tugberk";

            using (IDocumentStore store = CreateEmbeddableStore())
            {
                IdentityUser user = new IdentityUser(userName);
                IEnumerable<IdentityUserClaim> claims = new List<IdentityUserClaim>
                {
                    new IdentityUserClaim("Scope", "Read"),
                    new IdentityUserClaim("Scope", "Write")
                };

                foreach (IdentityUserClaim claim in claims)
                {
                    user.Claims.Add(claim);
                }

                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IUserClaimStore<IdentityUser> userClaimStore = new UserStore<IdentityUser>(ses);

                    await ses.StoreAsync(user);
                    await ses.SaveChangesAsync();
                }

                using (IAsyncDocumentSession ses = store.OpenAsyncSession())
                {
                    ses.Advanced.UseOptimisticConcurrency = true;
                    IUserClaimStore<IdentityUser> userClaimStore = new UserStore<IdentityUser>(ses);
                    IEnumerable<Claim> retrievedClaims = await userClaimStore.GetClaimsAsync(user);

                    Assert.Equal(2, claims.Count());
                    Assert.Equal("Read", claims.ElementAt(0).ClaimValue);
                    Assert.Equal("Write", claims.ElementAt(1).ClaimValue);
                }
            }
        }

        [Fact]
        public async Task GetUserClaims_Should_Not_Return_Null_If_User_Has_No_Claims()
        {
            string userName = "Tugberk";

            using (IDocumentStore store = CreateEmbeddableStore())
            using (IAsyncDocumentSession ses = store.OpenAsyncSession())
            {
                ses.Advanced.UseOptimisticConcurrency = true;
                IUserClaimStore<IdentityUser> userClaimStore = new UserStore<IdentityUser>(ses, false);
                IdentityUser user = new IdentityUser(userName);

                await ses.StoreAsync(user);
                await ses.SaveChangesAsync();

                // Act
                IEnumerable<Claim> retrievedClaims = await userClaimStore.GetClaimsAsync(user);

                // Assert
                Assert.Equal(0, retrievedClaims.Count());
            }
        }

        [Fact]
        public async Task AddClaimAsync_Should_Add_The_Claim_Into_The_User_Claims_Collection()
        {
            string userName = "Tugberk";

            using (IDocumentStore store = base.CreateEmbeddableStore())
            using(IAsyncDocumentSession ses = store.OpenAsyncSession())
            {
                ses.Advanced.UseOptimisticConcurrency = true;
                IUserClaimStore<IdentityUser> userClaimStore = new UserStore<IdentityUser>(ses, false);
                IdentityUser user = new IdentityUser(userName);

                await ses.StoreAsync(user);
                await ses.SaveChangesAsync();

                Claim claimToAdd = new Claim(ClaimTypes.Role, "Customer");
                await userClaimStore.AddClaimAsync(user, claimToAdd);

                Assert.Equal(1, user.Claims.Count());
                Assert.Equal(claimToAdd.Value, user.Claims.FirstOrDefault().ClaimValue);
                Assert.Equal(claimToAdd.Type, user.Claims.FirstOrDefault().ClaimType);
            }
        }

        [Fact]
        public async Task RemoveClaimAsync_Should_Remove_Claim_From_The_User_Claims_Collection()
        {
            string userName = "Tugberk";

            using (IDocumentStore store = CreateEmbeddableStore())
            using(IAsyncDocumentSession ses = store.OpenAsyncSession())
            {
                // Arrange
                ses.Advanced.UseOptimisticConcurrency = true;
                IUserClaimStore<IdentityUser> userClaimStore = new UserStore<IdentityUser>(ses, false);
                IdentityUser user = new IdentityUser(userName);

                Claim claimToAddAndRemove = new Claim(ClaimTypes.Role, "Customer");
                user.Claims.Add(new IdentityUserClaim(claimToAddAndRemove));

                await ses.StoreAsync(user);
                await ses.SaveChangesAsync();

                // Act
                await userClaimStore.RemoveClaimAsync(user, claimToAddAndRemove);

                // Assert
                Assert.Equal(0, user.Claims.Count());
            }
        }
    }
}