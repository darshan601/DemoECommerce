using AuthenticationApi.Application.DTOs;
using AuthenticationApi.Application.Interfaces;
using eCommerce.SharedLibrary.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationApi.Presentation.Controllers;


[Route("api/authentication")]
[ApiController]
[AllowAnonymous]
public class AuthenticationController(IUser userInterface):ControllerBase
{

    [HttpPost("register")]
    public async Task<ActionResult<Response>> Register(AppUserDTO appUser)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        var result = await userInterface.Register(appUser);

        return result.Flag ? Ok(result) : BadRequest(result);
    }
    
    [HttpPost("login")]
    public async Task<ActionResult<Response>> Login(LoginDTO loginDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        var result = await userInterface.Login(loginDto);

        return result.Flag ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<GetUserDTO>> GetUser(int id)
    {
        if (id <= 0) return BadRequest("Invalid user id");

        var user = await userInterface.GetUser(id);
        
        return user.Id>0 ? Ok(user) : BadRequest(user);
    }
}