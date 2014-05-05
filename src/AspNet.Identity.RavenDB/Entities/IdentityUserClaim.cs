using System;
using System.Security.Claims;
using Raven.Imports.Newtonsoft.Json;

namespace AspNet.Identity.RavenDB.Entities
{
    public class IdentityUserClaim : IEquatable<IdentityUserClaim>
    {
        public IdentityUserClaim(Claim claim)
        {
            if (claim == null) throw new ArgumentNullException("claim");

            ClaimType = claim.Type;
            ClaimValue = claim.Value;
        }

        [JsonConstructor]
        public IdentityUserClaim(string claimType, string claimValue)
        {
            if (claimType == null) throw new ArgumentNullException("claimType");
            if (claimValue == null) throw new ArgumentNullException("claimValue");

            ClaimType = claimType;
            ClaimValue = claimValue;
        }

        public string ClaimType { get; private set; }
        public string ClaimValue { get; private set; }

        public bool Equals(IdentityUserClaim other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(ClaimType, other.ClaimType) && string.Equals(ClaimValue, other.ClaimValue);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IdentityUserClaim) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ClaimType.GetHashCode()*397) ^ ClaimValue.GetHashCode();
            }
        }

        public static bool operator ==(IdentityUserClaim left, IdentityUserClaim right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(IdentityUserClaim left, IdentityUserClaim right)
        {
            return !Equals(left, right);
        }
    }
}