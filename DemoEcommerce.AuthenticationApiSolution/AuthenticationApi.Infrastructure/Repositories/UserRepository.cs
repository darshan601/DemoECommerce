using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Authentication.Infrastructure.Data;
using AuthenticationApi.Application.DTOs;
using AuthenticationApi.Application.Interfaces;
using AuthenticationApi.Domain.Entities;
using eCommerce.SharedLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Authentication.Infrastructure.Repositories;

public class UserRepository(AuthenticationDbContext context, IConfiguration config) : IUser
{
    private async Task<AppUser> GetUserByEmail(String email)
    {
        var user = await context.Users.FirstOrDefaultAsync(u=>u.Email == email);
        return user is null ? null! : user!;
    }
    
    public async Task<Response> Register(AppUserDTO appUserDto)
    {
        var getUser = await GetUserByEmail(appUserDto.Email);
        
        if(getUser is not null)
            return new Response(false, $"You cannot use this email for registration: {appUserDto.Email}");

        var result = context.Users.Add(new AppUser()
        {
            Name = appUserDto.Name,
            Email = appUserDto.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(appUserDto.Password),
            TelephoneNumber = appUserDto.TelephoneNumber,
            Address = appUserDto.Address,
            Role = appUserDto.Role
        });

        await context.SaveChangesAsync();

        return result.Entity.Id > 0
            ? new Response(true, "User Registered successfully")
            : new Response(false, "Invalid data provided");
    }

    public async Task<Response> Login(LoginDTO loginDto)
    {
        var getUser =await GetUserByEmail(loginDto.Email);

        if (getUser is null) return new Response(false, "Invalid Credentials");

        bool verifyPassword = BCrypt.Net.BCrypt.Verify(loginDto.Password, getUser.Password);

        if (!verifyPassword)
            return new Response(false, "Invalid Password");

        string token = GenerateToken(getUser);
        return new Response(true, token);
    }

    private string GenerateToken(AppUser user)
    {
        var key = Encoding.UTF8.GetBytes(config.GetSection("Authentication:Key").Value!);
        var securityKey = new SymmetricSecurityKey(key);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Name!),
            new(ClaimTypes.Email, user.Email!)
            // new(ClaimTypes.Role, user.Role!)
        };
        if(!string.IsNullOrEmpty(user.Role)|| !Equals("string", user.Role))
            claims.Add(new (ClaimTypes.Role, user.Role!));

        var token = new JwtSecurityToken(
            issuer: config["Authentication:Issuer"],
            audience: config["Authentication:Audience"],
            claims: claims,
            expires: null,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<GetUserDTO> GetUser(int userId)
    {
        var user = await context.Users.FindAsync(userId);

        return user is not null
            ? new GetUserDTO(user.Id,
                user.Name!,
                user.TelephoneNumber!,
                user.Address!,
                user.Email!,
                user.Role!)
            : null!;
    }
}