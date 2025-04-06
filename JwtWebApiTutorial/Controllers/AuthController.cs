using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace JwtWebApiTutorial.Controllers;

[ApiController]
[Route("/api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public static User user = new User();
    [HttpPost("register")]
    public async Task<IActionResult> Register(UserDto userDto)
    {
        try
        {
            CreatePasswordHash(userDto.Password, out byte[] passwordHash, out byte[] passwordSalt);
            user.Username = userDto.Username;
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            return CreatedAtAction("register", user);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(UserDto userDto)
    {
        try
        {
            if (user.Username != userDto.Username)
            {
                throw new Exception("User not found");
            }

            if (!VerifyPassword(userDto.Password, user.PasswordHash, user.PasswordSalt))
            {
                throw new Exception("Wrong password");
            }

            var jwtToken = CreateToken(user);
            return Ok(jwtToken);
        }
        catch(Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private string? CreateToken(User user)
    {
        List<Claim> claims = new List<Claim>()
        {
            new Claim(ClaimTypes.Name,user.Username),
            new Claim(ClaimTypes.Role,"Noob")
        };
        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:SecurityKey"]));
        
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            claims: claims, expires: DateTime.Now.AddHours(1), signingCredentials: creds);
        var jwt =  new JwtSecurityTokenHandler().WriteToken(token);
        return jwt;
    }

    private bool VerifyPassword(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
        {
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(passwordHash);
        }
    }

    private void CreatePasswordHash(string userDtoPassword, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using (var hmac = new System.Security.Cryptography.HMACSHA512())
        {
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(userDtoPassword));
                
        }
    }
}