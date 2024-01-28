using AuthenticationMicroservice.Extensions;
using AuthenticationMicroservice.Infrastructure.Repository;
using AuthenticationMicroservice.Models;
using AuthenticationMicroservice.Services;
using AuthenticationMicroservice.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationMicroservice.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly ILogger<AuthenticationController> _logger;
    private readonly IAuth _auth;
    private readonly IUnitOfWork uow;

    public AuthenticationController(ILogger<AuthenticationController> logger, IAuth auth, IUnitOfWork uow)
    {
        _logger = logger;
        _auth = auth;
        this.uow = uow;
    }

    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var login = await _auth.Authenticate(model.Username, model.Password);
        if (login is null)
        {
            return BadRequest(new
            {
                Succeeded = false,
                Message = "User not found"
            });
        }
        return Ok(new
        {
            Result = login,
            Succeeded = true,
            Message = "User logged in successfully"
        });
    }

    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> SignUp([FromBody] SignUpModel model)
    {

        await uow.Repository<User>().Add(new User
        {
            Username = model.Username,
            Password = model.Password.Hash(),

        });
        await uow.SaveChangesAsync();

        return Ok(new
        {
            Succeeded = true,
            Message = "User created successfully"
        });
    }
}
