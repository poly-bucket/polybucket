using Api.Controllers.Users.GetUser.Domain;
using AutoMapper;
using Core.Models.Users;

namespace Api.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, GetUserByIdResponse>();
        }
    }
}