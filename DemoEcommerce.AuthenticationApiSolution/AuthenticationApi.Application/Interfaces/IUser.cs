using AuthenticationApi.Application.DTOs;
using eCommerce.SharedLibrary.Responses;

namespace AuthenticationApi.Application.Interfaces;

public interface IUser
{
    Task<Response> Register(AppUserDTO appUserDto);

    Task<Response> Login(LoginDTO loginDto);

    Task<GetUserDTO> GetUser(int userId);
}