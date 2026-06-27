using AutoMapper;
using PlayByte.Application.Users.Queries.GetUserById;

namespace PlayByte.Application.Users;

/// <summary>Profile do AutoMapper para a fatia de Users (read model -> response).</summary>
public sealed class UsersMappingProfile : Profile
{
    public UsersMappingProfile()
    {
        CreateMap<UserReadModel, UserResponse>();
    }
}
