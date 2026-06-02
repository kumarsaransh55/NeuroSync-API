using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NeuroSync.Api.Data;
using NeuroSync.Api.DTOs;
using NeuroSync.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NeuroSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest(new { message = "An account with this email already exists." });

        var user = new User
        {
            Email = request.Email,
            FullName = request.FullName,
            // Hash the password using BCrypt
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new { message = "User registered successfully." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        // Check if user exists AND if the password matches the hash
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password." });

        // Generate the VIP Pass (JWT Token)
        var token = GenerateJwtToken(user);

        // Set an HttpOnly cookie for same-site web clients (best XSS protection).
        // SameSite=None (with Secure) lets the token also reach a cross-origin SPA.
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        Response.Cookies.Append("neurosync_jwt", token, cookieOptions);

        // ALSO return the token in the body so a cross-origin SPA can store it and
        // send it as an "Authorization: Bearer <token>" header on later requests.
        return Ok(new { userName = user.FullName, token });
    }

    private string GenerateJwtToken(User user)
    {
        var jwtHandler = new JwtSecurityTokenHandler();

        // 1. Get the Secret Key from appsettings
        var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);

        // 2. Define what is INSIDE the token (Claims)
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName)
        }),
            Expires = DateTime.UtcNow.AddDays(7),
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            // 3. This creates the MISSING HEADER and SIGNATURE
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        // 4. Create the token object
        var token = jwtHandler.CreateToken(tokenDescriptor);

        // 5. Turn it into the final 3-part string (2 dots!)
        return jwtHandler.WriteToken(token);
    }
}
