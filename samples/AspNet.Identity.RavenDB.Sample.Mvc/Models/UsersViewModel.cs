using System.Collections.Generic;

namespace AspNet.Identity.RavenDB.Sample.Mvc.Models
{
    public class UsersViewModel
    {
        public List<UserSummary> Users { get; set; }
        public class UserSummary
        {
            public int Id { get; set; }

            public string UserName { get; set; }
        }
    }
}