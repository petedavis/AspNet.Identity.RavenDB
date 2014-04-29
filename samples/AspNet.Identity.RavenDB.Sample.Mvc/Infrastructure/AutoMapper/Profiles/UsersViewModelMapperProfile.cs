using AspNet.Identity.RavenDB.Sample.Mvc.Infrastructure.AutoMapper.Profiles.Resolvers;
using AspNet.Identity.RavenDB.Sample.Mvc.Models;
using AutoMapper;

namespace AspNet.Identity.RavenDB.Sample.Mvc.Infrastructure.AutoMapper.Profiles
{
    public class UsersViewModelMapperProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<ApplicationUser, UsersViewModel.UserSummary>()
                .ForMember(x => x.Id, o => o.MapFrom(m => RavenIdResolver.Resolve(m.Id)));
        }
    }
}