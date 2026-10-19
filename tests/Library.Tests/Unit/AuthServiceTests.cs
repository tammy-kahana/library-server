using Library.Application.Common;
using Library.Application.DTOs.Auth;
using Library.Application.Interfaces;
using Library.Application.Services;
using Library.Domain.Entities;
using Moq;

namespace Library.Tests.Unit;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _service = new AuthService(_users.Object, _hasher.Object, Mock.Of<ITokenService>(), TestMapper.Create());
    }

    [Fact]
    public async Task Register_EmailAlreadyExists_ReturnsConflictWithoutAdding()
    {
        _users.Setup(r => r.EmailExistsAsync("reader1@library.local", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var request = new RegisterRequest { FullName = "חדשה", Email = " Reader1@Library.local ", Password = "Password1!" };

        var result = await _service.RegisterAsync(request, CancellationToken.None);

        Assert.Equal(ErrorType.Conflict, result.Error?.Type);
        _users.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var user = new User { Id = 1, Email = "reader1@library.local", PasswordHash = "hash" };
        _users.Setup(r => r.GetByEmailAsync("reader1@library.local", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("wrong", "hash")).Returns(false);

        var result = await _service.LoginAsync(new LoginRequest { Email = "reader1@library.local", Password = "wrong" },
            CancellationToken.None);

        Assert.Equal(ErrorType.Unauthorized, result.Error?.Type);
    }

    [Fact]
    public async Task Login_UnknownEmailAndWrongPassword_ReturnTheSameMessage()
    {
        // a@b.c לא קיים (ה-Mock מחזיר null). d@e.f קיים, אבל הסיסמה שגויה (Verify מחזיר false)
        _users.Setup(r => r.GetByEmailAsync("d@e.f", It.IsAny<CancellationToken>()))
              .ReturnsAsync(new User { PasswordHash = "hash" });

        var unknownEmail = await _service.LoginAsync(new LoginRequest { Email = "a@b.c", Password = "x" }, CancellationToken.None);
        var wrongPassword = await _service.LoginAsync(new LoginRequest { Email = "d@e.f", Password = "x" }, CancellationToken.None);

        Assert.Equal(ErrorType.Unauthorized, unknownEmail.Error?.Type);
        Assert.Equal(unknownEmail.Error!.Message, wrongPassword.Error!.Message);
    }
}
