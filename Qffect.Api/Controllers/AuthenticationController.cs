using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Qffect.Domain.ADM; // ApplicationUser
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Qffect.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthenticationController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly UserManager<ApplicationUser> _users;

    public AuthenticationController(IConfiguration config, UserManager<ApplicationUser> users)
    {
        _config = config;
        _users = users;
    }

    /// <summary>
    /// Gets a JWT using username/password. If 2FA is enabled for the user,
    /// returns requiresTwoFactor=true (then call /api/v1/auth/2fa).
    /// </summary>
    [HttpPost("token")]
    [AllowAnonymous]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetToken([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var user = await _users.FindByNameAsync(dto.Username!)
                   ?? await _users.FindByEmailAsync(dto.Username!);
        if (user is null) return Unauthorized();

        var validPw = await _users.CheckPasswordAsync(user, dto.Password!);
        if (!validPw) return Unauthorized();

        if (await _users.GetTwoFactorEnabledAsync(user))
        {
            // client must now call /api/v1/auth/2fa with TOTP code
            return Ok(new TwoFaRequired { RequiresTwoFactor = true });
        }

        return Ok(CreateJwtForUser(user));
    }

    /// <summary>
    /// Completes 2FA (TOTP) for users who have 2FA enabled.
    /// </summary>
    [HttpPost("2fa")]
    [AllowAnonymous]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TwoFa([FromBody] TwoFaDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var user = await _users.FindByNameAsync(dto.Username!)
                   ?? await _users.FindByEmailAsync(dto.Username!);
        if (user is null) return Unauthorized();

        var ok = await _users.VerifyTwoFactorTokenAsync(
            user, TokenOptions.DefaultAuthenticatorProvider, dto.Code!);

        if (!ok) return Unauthorized();

        return Ok(CreateJwtForUser(user));
    }

    // ----- Helpers -----

    private TokenResponse CreateJwtForUser(ApplicationUser user)
    {
        var issuer = _config["Jwt:Issuer"];
        var audience = _config["Jwt:Audience"];
        var key = _config["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException("JWT key missing (set Jwt:Key for Qffect.Api).");

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? ""),
            new("tenantId", user.TenantId.ToString())
        };

        // Optional: add roles as claims if you’re using roles
        // var roles = await _users.GetRolesAsync(user);
        // claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new TokenResponse
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            TokenType = "Bearer",
            ExpiresIn = 8 * 3600
        };
    }

    // ----- DTOs -----

    public class LoginDto
    {
        /// <example>admin@qffect.local</example>
        [Required] public string? Username { get; set; }

        /// <example>Admin#12345</example>
        [Required] public string? Password { get; set; }
    }

    public class TwoFaDto
    {
        /// <example>admin@qffect.local</example>
        [Required] public string? Username { get; set; }

        /// <summary>6-digit authenticator app code</summary>
        /// <example>123456</example>
        [Required] public string? Code { get; set; }
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; } = "";
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; }
    }

    public class TwoFaRequired
    {
        public bool RequiresTwoFactor { get; set; }
    }
}
