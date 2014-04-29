using AspNet.Identity.RavenDB.Sample.Mvc.Infrastructure.AutoMapper.Profiles;
using AutoMapper;

namespace AspNet.Identity.RavenDB.Sample.Mvc.Infrastructure.AutoMapper
{
    public class AutoMapperConfiguration
    {
        public static void Configure()
        {
            Mapper.AddProfile(new UsersViewModelMapperProfile());
        }
    }
}