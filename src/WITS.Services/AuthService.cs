// ----------------------------------------------------------------------------------
//  Created by: Nathan Zwelibanzi Khupe
//  � 2025 Nathan Zwelibanzi Khupe. All rights reserved.
//  Unauthorized copying, distribution, modification, or use strictly prohibited.
//  ----------------------------------------------------------------------------------

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WITS.Common;
using WITS.Data.Common;
using WITS.Data.Contracts;
using WITS.Data.Entity;
using WITS.Models;
using static WITS.Services.IAuthService;

namespace WITS.Services;

public record CredentialResult(bool Success, string? Token, string Message = "");
public record RegistrationResult(bool Success, string Message = "");

public interface IAuthService
{
    string GenerateToken(UserTokenModel userToken);
    Task<CredentialResult> ValidateCredentialsAsync(string username, string password);
    Task<RegistrationResult> RegisterUserAsync(RegisterRequest user);
}

public class AuthService : IAuthService
{
    private readonly IGenericRepository<User, long> _userRepository;
    private readonly JwtSettings _jwtSettings;

    public AuthService(IGenericRepository<User, long> userRepository, IOptions<JwtSettings> jwtSettings)
    {
        _userRepository = userRepository;
        _jwtSettings = jwtSettings.Value;
    }

    public string GenerateToken(UserTokenModel userToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecurityKey);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, userToken.Username),
            new(ClaimTypes.Email, userToken.EmailAddress),
            new(ClaimTypes.Role, userToken.RoleLevel.ToString()),
            new("OrganizationId", userToken.OrganisationId.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<CredentialResult> ValidateCredentialsAsync(string username, string password)
    {
        var filter = new GenericFilter
        {
            PageNumber = 1,
            PageSize = 1,
            Filters = new Dictionary<string, FilterCriteria>
            {
                { "Username", new FilterCriteria(username) }
            }
        };

        var result = await _userRepository.QueryAsync(filter);
        var user = result.Items.FirstOrDefault();

        if (user == null)
        {
            return new CredentialResult(false, null, "User account not found!");
        }

        if (!VerifyPasswordHash(password, user.Password) && !VerifyPassword(password, user.Password))
        {
            return new CredentialResult(false, null, "Invalid credentials");
        }

        var token = GenerateToken(new UserTokenModel()
        {
            Username = username,
            EmailAddress = user.EmailAddress,
            RoleLevel = user.RoleLevel,
            OrganisationId = user.OrganisationId,
        });
        return new CredentialResult(true, token);
    }

    private bool VerifyPassword(string password1, string password2)
    {
        if (string.IsNullOrEmpty(password1) || string.IsNullOrEmpty(password2)) return false;

        return password1.Trim().Equals(password2.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    public async Task<RegistrationResult> RegisterUserAsync(RegisterRequest user)
    {
        // Check if user exists
        var filter = new GenericFilter
        {
            PageNumber = 1,
            PageSize = 1,
            Filters = new Dictionary<string, FilterCriteria>
            {
                { "UserName", new FilterCriteria(user.Username)}
            }
        };

        var existing = await _userRepository.QueryAsync(filter);
        if (existing.Items.Any())
        {
            return new RegistrationResult(false, "Username already exists");
        }

        // Hash password
        user.Password = HashPassword(user.Password);
        user.Created = DateTime.UtcNow;

        int id = await _userRepository.InsertAsync(new User(){
            Id=0,
            Username=user.Username,
            FullName=user.FullName,
            EmailAddress=user.EmailAddress,
            Password=user.Password,
            Created=DateTime.UtcNow,
            OrganisationId=user.OrganisationId,
            TelephoneNos = user.TelephoneNos,
            Organisation = user.Organisation,
            RoleLevel = user.RoleLevel,
            Status = user.Status,
        });
        return new RegistrationResult(true, $"User registered successfully with id {id}.");
    }

    private string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password)) return string.Empty;
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private bool VerifyPasswordHash(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash)) return false;
        var hashedInput = HashPassword(password);
        return hashedInput == storedHash;
    }
}
