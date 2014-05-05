using System;
using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using Raven.Client.UniqueConstraints;

namespace AspNet.Identity.RavenDB.Entities
{
    /// <summary>
    ///     Default RavenDB IUser implementation
    /// </summary>
    public class IdentityUser : IdentityUser<IdentityUserLogin, IdentityUserRole, IdentityUserClaim>, IUser
    {
        /// <summary>
        ///     Constructor which creates a new Guid for the Id
        /// </summary>
        public IdentityUser()
        {
        }

        /// <summary>
        ///     Constructor that takes a userName
        /// </summary>
        /// <param name="userName"></param>
        public IdentityUser(string userName)
            : this()
        {
            UserName = userName;
        }

        /// <summary>
        ///     Constructor that takes a userName and email
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="email"></param>
        public IdentityUser(string userName, string email)
            : this(userName)
        {
            Email = email;
        }
    }

    /// <summary>
    ///     Default RavenDB IUser implementation
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TLogin"></typeparam>
    /// <typeparam name="TRole"></typeparam>
    /// <typeparam name="TClaim"></typeparam>
    public class IdentityUser<TLogin, TRole, TClaim> : IUser<string>
        where TLogin : IdentityUserLogin
        where TRole : IdentityUserRole
        where TClaim : IdentityUserClaim
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public IdentityUser()
        {
            Claims = new List<TClaim>();
            Roles = new List<TRole>();
            Logins = new List<TLogin>();
        }

        /// <summary>
        ///     Email
        /// </summary>
        [UniqueConstraint(CaseInsensitive = true)]
        public virtual string Email { get; set; }

        /// <summary>
        ///     True if the email is confirmed, default is false
        /// </summary>
        public virtual bool EmailConfirmed { get; set; }

        /// <summary>
        ///     The salted/hashed form of the user password
        /// </summary>
        public virtual string PasswordHash { get; set; }

        /// <summary>
        ///     A random value that should change whenever a users credentials have changed (password changed, login removed)
        /// </summary>
        public virtual string SecurityStamp { get; set; }

        /// <summary>
        ///     PhoneNumber for the user
        /// </summary>
        public virtual string PhoneNumber { get; set; }

        /// <summary>
        ///     True if the phone number is confirmed, default is false
        /// </summary>
        public virtual bool PhoneNumberConfirmed { get; set; }

        /// <summary>
        ///     Is two factor enabled for the user
        /// </summary>
        public virtual bool TwoFactorEnabled { get; set; }

        /// <summary>
        ///     DateTime in UTC when lockout ends, any time in the past is considered not locked out.
        /// </summary>
        public virtual DateTimeOffset? LockoutEndDateUtc { get; set; }

        /// <summary>
        ///     Is lockout enabled for this user
        /// </summary>
        public virtual bool LockoutEnabled { get; set; }

        /// <summary>
        ///     Used to record failures for the purposes of lockout
        /// </summary>
        public virtual int AccessFailedCount { get; set; }

        /// <summary>
        ///     Navigation property for user roles
        /// </summary>
        public virtual ICollection<TRole> Roles { get; private set; }

        /// <summary>
        ///     Navigation property for user claims
        /// </summary>
        public virtual ICollection<TClaim> Claims { get; private set; }

        /// <summary>
        ///     Navigation property for user logins
        /// </summary>
        public virtual ICollection<TLogin> Logins { get; private set; }

        /// <summary>
        ///     User ID (Primary Key)
        /// </summary>
        public virtual string Id { get; set; }

        /// <summary>
        ///     User name
        /// </summary>
        [UniqueConstraint(CaseInsensitive = true)]
        public virtual string UserName { get; set; }
    }
}