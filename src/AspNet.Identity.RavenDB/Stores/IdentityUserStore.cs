using AspNet.Identity.RavenDB.Entities;
using Microsoft.AspNet.Identity;
using Raven.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AspNet.Identity.RavenDB.Stores
{
    public class IdentityUserStore<TUser> : IUserStore<TUser>,
        IUserLoginStore<TUser>,
        IUserClaimStore<TUser>,
        IUserPasswordStore<TUser>,
        IUserSecurityStampStore<TUser>,
        IQueryableUserStore<TUser>,
        IUserTwoFactorStore<TUser, string>,
        IUserLockoutStore<TUser, string>,
        IUserEmailStore<TUser>,
        IUserPhoneNumberStore<TUser>,
        IUserRoleStore<TUser>,
        IDisposable where TUser : IdentityUser
    {
        private readonly bool _disposeDocumentSession;
        private readonly IAsyncDocumentSession _documentSession;

        public IdentityUserStore(IAsyncDocumentSession documentSession)
            : this(documentSession, true)
        {
        }

        public IdentityUserStore(IAsyncDocumentSession documentSession, bool disposeDocumentSession)
        {
            if (documentSession == null)
            {
                throw new ArgumentNullException("documentSession");
            }

            if (documentSession.Advanced.UseOptimisticConcurrency == false)
            {
                throw new NotSupportedException("Optimistic concurrency disabled 'IAsyncDocumentSession' instance is not supported because the uniqueness of the username and the e-mail needs to ensured. Please enable optimistic concurrency by setting the 'Advanced.UseOptimisticConcurrency' property on the 'IAsyncDocumentSession' instance and leave the optimistic concurrency enabled on the session till the end of its lifetime. Otherwise, you will have a chance of ending up overriding an existing user's data if a new user tries to register with the username of that existing user.");
            }

            _documentSession = documentSession;
            _disposeDocumentSession = disposeDocumentSession;
        }

        // IQueryableUserStore

        public IQueryable<TUser> Users
        {
            get
            {
                return _documentSession.Query<TUser>();
            }
        }

        // IUserStore

        /// <remarks>
        /// This method doesn't perform uniquness. That's the responsibility of the session provider.
        /// </remarks>
        public async Task CreateAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (user.UserName == null)
            {
                throw new InvalidOperationException("Cannot create user as the 'UserName' property is null on user parameter.");
            }

            await _documentSession.StoreAsync(user).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(user.Email))
            {
                // Store the email address to ensure it is unique.
                var userEmail = new IdentityUserEmail(user.Email, user.Id);
                await _documentSession.StoreAsync(userEmail).ConfigureAwait(false);
            }
            
            await _documentSession.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Loads the user by id from the current document session.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <remarks>
        /// This will handle both full document id "RaventUser/1" or just the ducument id "1".
        /// </remarks>
        public Task<TUser> FindByIdAsync(string userId)
        {
            if (userId == null) throw new ArgumentNullException("userId");

            if(userId.IndexOf("/", StringComparison.OrdinalIgnoreCase) > 0)
                return _documentSession.LoadAsync<TUser>(userId);

            var id = int.Parse(userId);
            return _documentSession.LoadAsync<TUser>(id);
        }

        public Task<TUser> FindByNameAsync(string userName)
        {
            if (userName == null) throw new ArgumentNullException("userName");

            //return _documentSession.LoadAsync<TUser>(IdentityUser.GenerateKey(userName));
            return _documentSession.Query<TUser>().FirstOrDefaultAsync(x => x.UserName == userName);
        }

        /// <remarks>
        /// This method assumes that incomming TUser parameter is tracked in the session. So, this method literally behaves as SaveChangeAsync
        /// </remarks>
        public Task UpdateAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return _documentSession.SaveChangesAsync();
        }

        public async Task DeleteAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            _documentSession.Delete(user);

            if (!string.IsNullOrEmpty(user.Email))
            {
                // Delete the associated email document.
                var key = IdentityUserEmail.GenerateKey(user.Email);
                var identityEmail = await _documentSession.LoadAsync<IdentityUserEmail>(key).ConfigureAwait(false);
                _documentSession.Delete(identityEmail);
            }

            await _documentSession.SaveChangesAsync().ConfigureAwait(false);
        }

        // IUserLoginStore

        public Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user)
        {
            if (user == null) throw new ArgumentNullException("user");

            return Task.FromResult<IList<UserLoginInfo>>(
                user.Logins.Select(login => 
                    new UserLoginInfo(login.LoginProvider, login.ProviderKey)).ToList());
        }

        public async Task<TUser> FindAsync(UserLoginInfo login)
        {
            if (login == null) throw new ArgumentNullException("login");

            string keyToLookFor = IdentityUserLogin.GenerateKey(login.LoginProvider, login.ProviderKey);
            IdentityUserLogin identityUserLogin = await _documentSession
                .Include<IdentityUserLogin, TUser>(usrLogin => usrLogin.UserId)
                .LoadAsync(keyToLookFor)
                .ConfigureAwait(false);

            return (identityUserLogin != null)
                ? await _documentSession.LoadAsync<TUser>(identityUserLogin.UserId).ConfigureAwait(false)
                : default(TUser);
        }

        public async Task AddLoginAsync(TUser user, UserLoginInfo login)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (login == null) throw new ArgumentNullException("login");

            IdentityUserLogin identityUserLogin = new IdentityUserLogin(user.Id, login);
            await _documentSession.StoreAsync(identityUserLogin).ConfigureAwait(false);
            user.Logins.Add(identityUserLogin);
        }

        public async Task RemoveLoginAsync(TUser user, UserLoginInfo login)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (login == null) throw new ArgumentNullException("login");

            string keyToLookFor = IdentityUserLogin.GenerateKey(login.LoginProvider, login.ProviderKey);
            IdentityUserLogin identityUserLogin = await _documentSession.LoadAsync<IdentityUserLogin>(keyToLookFor).ConfigureAwait(false);
            if (identityUserLogin != null)
            {
                _documentSession.Delete(identityUserLogin);
            }

            IdentityUserLogin userLogin = user.Logins.FirstOrDefault(lgn => lgn.Id.Equals(keyToLookFor, StringComparison.InvariantCultureIgnoreCase));
            if (userLogin != null)
            {
                user.Logins.Remove(userLogin);
            }
        }

        // IUserClaimStore

        public Task<IList<Claim>> GetClaimsAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return Task.FromResult<IList<Claim>>(user.Claims.Select(clm => new Claim(clm.ClaimType, clm.ClaimValue)).ToList());
        }

        public Task AddClaimAsync(TUser user, Claim claim)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (claim == null) throw new ArgumentNullException("claim");

            user.Claims.Add(new IdentityUserClaim(claim));
            return Task.FromResult(0);
        }

        public Task RemoveClaimAsync(TUser user, Claim claim)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (claim == null) throw new ArgumentNullException("claim");

            IdentityUserClaim userClaim = user.Claims
                .FirstOrDefault(clm => clm.ClaimType == claim.Type && clm.ClaimValue == claim.Value);

            if (userClaim != null)
            {
                user.Claims.Remove(userClaim);
            }

            return Task.FromResult(0);
        }

        // IUserPasswordStore

        public Task<string> GetPasswordHashAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return Task.FromResult<string>(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return Task.FromResult<bool>(user.PasswordHash != null);
        }

        public Task SetPasswordHashAsync(TUser user, string passwordHash)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        // IUserSecurityStampStore

        public Task<string> GetSecurityStampAsync(TUser user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return Task.FromResult<string>(user.SecurityStamp);
        }

        public Task SetSecurityStampAsync(TUser user, string stamp)
        {
            if (user == null) throw new ArgumentNullException("user");
            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        // IUserTwoFactorStore

        public Task<bool> GetTwoFactorEnabledAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return Task.FromResult(user.TwoFactorEnabled);
        }

        public Task SetTwoFactorEnabledAsync(TUser user, bool enabled)
        {
            if (user == null) throw new ArgumentNullException("user");

            user.TwoFactorEnabled = enabled;

            return Task.FromResult(0);
        }

        // IUserEmailStore

        public async Task<TUser> FindByEmailAsync(string email)
        {
            if (email == null)
            {
                throw new ArgumentNullException("email");
            }

            string keyToLookFor = IdentityUserEmail.GenerateKey(email);
            IdentityUserEmail identityUserEmail = await _documentSession
                .Include<IdentityUserEmail, TUser>(usrEmail => usrEmail.UserId)
                .LoadAsync(keyToLookFor)
                .ConfigureAwait(false);

            return (identityUserEmail != null)
                ? await _documentSession.LoadAsync<TUser>(identityUserEmail.UserId).ConfigureAwait(false)
                : default(TUser);
        }

        public Task<string> GetEmailAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return Task.FromResult(user.Email);
        }

        public async Task<bool> GetEmailConfirmedAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (user.Email == null)
            {
                throw new InvalidOperationException("Cannot get the confirmation status of the e-mail because user doesn't have an e-mail.");
            }

            ConfirmationRecord confirmation = await GetUserEmailConfirmationAsync(user.Email)
                .ConfigureAwait(false);

            return confirmation != null;
        }

        public Task SetEmailAsync(TUser user, string email)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (email == null) throw new ArgumentNullException("email");

            user.Email = email;
            IdentityUserEmail IdentityUserEmail = new IdentityUserEmail(email, user.Id);

            return _documentSession.StoreAsync(IdentityUserEmail);
        }

        public async Task SetEmailConfirmedAsync(TUser user, bool confirmed)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (user.Email == null)
            {
                throw new InvalidOperationException("Cannot set the confirmation status of the e-mail because user doesn't have an e-mail.");
            }

            IdentityUserEmail userEmail = await GetUserEmailAsync(user.Email).ConfigureAwait(false);
            if (userEmail == null)
            {
                throw new InvalidOperationException("Cannot set the confirmation status of the e-mail because user doesn't have an e-mail as IdentityUserEmail document.");
            }

            user.EmailConfirmed = confirmed;
            if (confirmed)
            {
                userEmail.SetConfirmed();
            }
            else
            {
                userEmail.SetUnconfirmed();
            }
        }

        // IUserPhoneNumberStore

        public Task<string> GetPhoneNumberAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return Task.FromResult(user.PhoneNumber);
        }

        public async Task<bool> GetPhoneNumberConfirmedAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (user.PhoneNumber == null)
            {
                throw new InvalidOperationException("Cannot get the confirmation status of the phone number because user doesn't have a phone number.");
            }

            ConfirmationRecord confirmation = await GetUserPhoneNumberConfirmationAsync(user.PhoneNumber)
                .ConfigureAwait(false);

            return confirmation != null;
        }

        public Task SetPhoneNumberAsync(TUser user, string phoneNumber)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (phoneNumber == null) throw new ArgumentNullException("phoneNumber");

            user.PhoneNumber = phoneNumber;
            IdentityUserPhoneNumber IdentityUserPhoneNumber = new IdentityUserPhoneNumber(phoneNumber, user.Id);

            return _documentSession.StoreAsync(IdentityUserPhoneNumber);
        }

        public async Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (user.PhoneNumber == null)
            {
                throw new InvalidOperationException("Cannot set the confirmation status of the phone number because user doesn't have a phone number.");
            }

            IdentityUserPhoneNumber userPhoneNumber = await GetUserPhoneNumberAsync(user.Email).ConfigureAwait(false);
            if (userPhoneNumber == null)
            {
                throw new InvalidOperationException("Cannot set the confirmation status of the phone number because user doesn't have a phone number as IdentityUserPhoneNumber document.");
            }

            if (confirmed)
            {
                userPhoneNumber.SetConfirmed();
            }
            else
            {
                userPhoneNumber.SetUnconfirmed();
            }
        }

        // IUserLockoutStore

        public Task<DateTimeOffset> GetLockoutEndDateAsync(TUser user)
        {
            if (user == null) throw new ArgumentNullException("user");

            return
                Task.FromResult(user.LockoutEndDateUtc.HasValue
                    ? user.LockoutEndDateUtc.Value
                    : new DateTimeOffset());
        }

        public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset lockoutEnd)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user.LockoutEndDateUtc = lockoutEnd;
            return Task.FromResult(0);
        }

        public Task<int> IncrementAccessFailedCountAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            // NOTE: Not confortable to do this like below but this will work out for the intended scenario
            //       + RavenDB doesn't have a reliable solution for $inc update as MongoDB does.
            user.AccessFailedCount++;
            return Task.FromResult(user.AccessFailedCount);
        }

        public Task ResetAccessFailedCountAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user.AccessFailedCount = 0;
            return Task.FromResult(0);
        }

        public Task<int> GetAccessFailedCountAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return Task.FromResult(user.AccessFailedCount);
        }

        public Task<bool> GetLockoutEnabledAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return Task.FromResult(user.LockoutEnabled);
        }

        public Task SetLockoutEnabledAsync(TUser user, bool enabled)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user.LockoutEnabled = enabled;

            return Task.FromResult(0);
        }

        // Dispose

        protected void Dispose(bool disposing)
        {
            if (_disposeDocumentSession && disposing && _documentSession != null)
            {
                _documentSession.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // privates

        private Task<IdentityUserEmail> GetUserEmailAsync(string email)
        {
            string keyToLookFor = IdentityUserEmail.GenerateKey(email);
            return _documentSession.LoadAsync<IdentityUserEmail>(keyToLookFor);
        }

        private Task<IdentityUserPhoneNumber> GetUserPhoneNumberAsync(string phoneNumber)
        {
            string keyToLookFor = IdentityUserPhoneNumber.GenerateKey(phoneNumber);
            return _documentSession.LoadAsync<IdentityUserPhoneNumber>(keyToLookFor);
        }

        private async Task<ConfirmationRecord> GetUserEmailConfirmationAsync(string email)
        {
            IdentityUserEmail userEmail = await GetUserEmailAsync(email).ConfigureAwait(false);

            return (userEmail != null)
                ? userEmail.ConfirmationRecord
                : null;
        }

        private async Task<ConfirmationRecord> GetUserPhoneNumberConfirmationAsync(string phoneNumber)
        {
            IdentityUserPhoneNumber userPhoneNumber = await GetUserPhoneNumberAsync(phoneNumber).ConfigureAwait(false);

            return (userPhoneNumber != null)
                ? userPhoneNumber.ConfirmationRecord
                : null;
        }

        public Task AddToRoleAsync(TUser user, string roleName)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (roleName == null) throw new ArgumentNullException("roleName");
            var role = new IdentityUserRole(roleName);
            if (!user.Roles.Contains(role))
            {
                user.Roles.Add(role);
            }
            return Task.FromResult(0);
        }

        public Task RemoveFromRoleAsync(TUser user, string roleName)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (roleName == null) throw new ArgumentNullException("roleName");

            var role = new IdentityUserRole(roleName);
            user.Roles.Remove(role);

            return Task.FromResult(0);
        }

        public Task<IList<string>> GetRolesAsync(TUser user)
        {
            return Task.FromResult((IList<string>)user.Roles.Select(x => x.Name).ToList());
        }

        public Task<bool> IsInRoleAsync(TUser user, string roleName)
        {
            string lowerInvariant = roleName.ToLowerInvariant();
            return Task.FromResult(user.Roles.Any(x => x.Name.ToLowerInvariant().Equals(lowerInvariant)));
        }
    }
}