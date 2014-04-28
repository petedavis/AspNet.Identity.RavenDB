using System;
using System.Security.Claims;
using Raven.Imports.Newtonsoft.Json;

namespace AspNet.Identity.RavenDB.Entities
{
    public class RavenUserClaim : IEquatable<RavenUserClaim>
    {
        public RavenUserClaim(Claim claim)
        {
            if (claim == null) throw new ArgumentNullException("claim");

            ClaimType = claim.Type;
            ClaimValue = claim.Value;
        }

        [JsonConstructor]
        public RavenUserClaim(string claimType, string claimValue)
        {
            if (claimType == null) throw new ArgumentNullException("claimType");
            if (claimValue == null) throw new ArgumentNullException("claimValue");

            ClaimType = claimType;
            ClaimValue = claimValue;
        }

        public string ClaimType { get; private set; }
        public string ClaimValue { get; private set; }

        public bool Equals(RavenUserClaim other)
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
            return Equals((RavenUserClaim) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ClaimType.GetHashCode()*397) ^ ClaimValue.GetHashCode();
            }
        }

        public static bool operator ==(RavenUserClaim left, RavenUserClaim right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(RavenUserClaim left, RavenUserClaim right)
        {
            return !Equals(left, right);
        }
    }
}