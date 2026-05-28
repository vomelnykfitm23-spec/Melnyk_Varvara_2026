using AuctionApp.Data;
using AuctionApp.DTOs;
using AuctionApp.Models;
using AuctionApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionApp.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext db, JwtService jwt) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest req)
    {
        if (await db.Users.AnyAsync(u => u.Email == req.Email))
            return BadRequest(new { message = "Email already in use" });

        if (await db.Users.AnyAsync(u => u.Username == req.Username))
            return BadRequest(new { message = "Username already taken" });

        var user = new User
        {
            Username     = req.Username,
            Email        = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            RoleId       = 2, // User
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        // Load role for JWT
        await db.Entry(user).Reference(u => u.Role).LoadAsync();

        var token = jwt.Generate(user);
        return Ok(new AuthResponse(token, ToDto(user)));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest req)
    {
        var user = await db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == req.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return BadRequest(new { message = "Invalid email or password" });

        var token = jwt.Generate(user);
        return Ok(new AuthResponse(token, ToDto(user)));
    }

    private static UserDto ToDto(User u) =>
        new(u.Id, u.Username, u.Email, u.Role.Name);
}
