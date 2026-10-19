using AutoMapper;
using Library.Application.Common;
using Library.Application.DTOs.Auth;
using Library.Application.DTOs.Users;
using Library.Application.Interfaces;
using Library.Domain.Entities;

namespace Library.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public AuthService(IUserRepository users, IPasswordHasher passwordHasher, ITokenService tokenService, IMapper mapper)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var email = NormalizeEmail(request.Email);
        if (await _users.EmailExistsAsync(email, ct))
        {
            return Error.Conflict("כבר קיים חשבון עם המייל הזה");
        }

        var user = _mapper.Map<User>(request);
        user.Email = email;
        user.PasswordHash = _passwordHasher.Hash(request.Password);
        user.CreatedAtUtc = DateTime.UtcNow;

        _users.Add(user);
        await _users.SaveChangesAsync(ct);

        return CreateAuthResponse(user);
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var user = await _users.GetByEmailAsync(NormalizeEmail(request.Email), ct);

        // אותה הודעה למייל שלא קיים ולסיסמה שגויה, כדי שאי אפשר יהיה לגלות אילו מיילים רשומים
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Error.Unauthorized("מייל או סיסמה שגויים");
        }

        return CreateAuthResponse(user);
    }

    /// <summary>מיילים נשמרים ונבדקים באותיות קטנות, כך ש-Reader1@... ו-reader1@... הם אותו חשבון.</summary>
    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private AuthResponse CreateAuthResponse(User user)
    {
        var token = _tokenService.CreateToken(user);
        return new AuthResponse
        {
            Token = token.Token,
            ExpiresAtUtc = token.ExpiresAtUtc,
            User = _mapper.Map<UserResponse>(user)
        };
    }
}
