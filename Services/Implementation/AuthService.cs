using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repositories.Models;
using Repositories.UnitOfWork;
using Services.BM;
using Services.Interface;

namespace Services.Implementation;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _config;
    private readonly INotificationService _notificationService;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration config, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _config = config;
        _notificationService = notificationService;
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return new ApiResponse<AuthResponse>
            {
                Success = false,
                Message = "Email hoặc mật khẩu không chính xác"
            };
        }

        string token = GenerateJwtToken(user);
        string refreshToken = GenerateRefreshToken();
        
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        _unitOfWork.UserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();
        
        return new ApiResponse<AuthResponse>
        {
            Success = true,
            Message = "Đăng nhập thành công",
            Data = new AuthResponse
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                UserId = user.UserId.ToString(),
                FullName = user.FullName
            }
        };
    }

    public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _unitOfWork.UserRepository.GetUserByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return new ApiResponse<AuthResponse>
            {
                Success = false,
                Message = "Email đã được sử dụng"
            };
        }

        var newUser = new User
        {
            UserId = Guid.NewGuid(),
            Email = request.Email,
            FullName = request.FullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            PhoneNumber = request.PhoneNumber,
            Role = 1, // 1 for Customer
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.UserRepository.AddAsync(newUser);
        await _unitOfWork.SaveChangesAsync();

        string token = GenerateJwtToken(newUser);
        string refreshToken = GenerateRefreshToken();

        newUser.RefreshToken = refreshToken;
        newUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        _unitOfWork.UserRepository.Update(newUser);
        await _unitOfWork.SaveChangesAsync();

        // Push notification to Admin
        await _notificationService.PushNotificationAsync(
            userId: null, // Admin
            title: "Khách hàng mới",
            content: $"Khách hàng {newUser.FullName} vừa đăng ký tài khoản.",
            type: "info",
            targetType: "User",
            targetId: newUser.UserId.ToString()
        );

        return new ApiResponse<AuthResponse>
        {
            Success = true,
            Message = "Đăng ký thành công",
            Data = new AuthResponse
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                UserId = newUser.UserId.ToString(),
                FullName = newUser.FullName
            }
        };
    }

    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var user = await _unitOfWork.UserRepository.GetUserByRefreshTokenAsync(request.RefreshToken);

        if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return new ApiResponse<AuthResponse>
            {
                Success = false,
                Message = "Refresh Token không hợp lệ hoặc đã hết hạn"
            };
        }

        string newAccessToken = GenerateJwtToken(user);
        string newRefreshToken = GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        _unitOfWork.UserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return new ApiResponse<AuthResponse>
        {
            Success = true,
            Message = "Làm mới Token thành công",
            Data = new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                UserId = user.UserId.ToString(),
                FullName = user.FullName
            }
        };
    }

    private string GenerateJwtToken(User user)
    {
        var keyString = _config["Jwt:Key"];
        if (string.IsNullOrEmpty(keyString))
        {
            keyString = "super_secret_jwt_key_that_is_very_long_for_hs256";
        }
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.FullName ?? ""),
            new Claim(ClaimTypes.Role, user.Role?.ToString() ?? "1")
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "myapp",
            audience: _config["Jwt:Audience"] ?? "myapp_users",
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
