using HotelStay.Application.DTOs.Auth;
using HotelStay.Application.Interfaces;
using HotelStay.Application.Services;
using HotelStay.Domain.Entities;
using HotelStay.Domain.Exceptions;
using Moq;
using FluentAssertions;
using Xunit;

namespace HotelStay.Tests.Unit;

public class AuthServiceTests
{
    private static AuthService CreateService(
        IUserStore? userStore = null,
        IJwtService? jwtService = null,
        IPasswordHasher? hasher = null)
    {
        userStore ??= new Mock<IUserStore>().Object;
        jwtService ??= new Mock<IJwtService>().Object;
        hasher ??= new Mock<IPasswordHasher>().Object;
        return new AuthService(userStore, jwtService, hasher);
    }

    [Fact]
    public void Register_NewEmail_SavesUserAndReturnsTokens()
    {
        var storeMock = new Mock<IUserStore>();
        storeMock.Setup(s => s.GetByEmail(It.IsAny<string>())).Returns((User?)null);

        var jwtMock = new Mock<IJwtService>();
        jwtMock.Setup(j => j.GenerateAccessToken(It.IsAny<User>())).Returns("access");
        jwtMock.Setup(j => j.GenerateRefreshToken()).Returns("refresh");

        var hasherMock = new Mock<IPasswordHasher>();
        hasherMock.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed");

        var sut = CreateService(storeMock.Object, jwtMock.Object, hasherMock.Object);

        var result = sut.Register(new RegisterDto("user@test.com", "testuser", "Password1"));

        result.AccessToken.Should().Be("access");
        result.RefreshToken.Should().Be("refresh");
        storeMock.Verify(s => s.Save(It.Is<User>(u =>
            u.Email == "user@test.com" &&
            u.Username == "testuser" &&
            u.PasswordHash == "hashed" &&
            u.Role == UserRole.User)), Times.Once);
    }

    [Fact]
    public void Register_DuplicateEmail_ThrowsEmailAlreadyExistsException()
    {
        var existing = new User(Guid.NewGuid(), "user@test.com", "testuser", "hash", UserRole.User, DateTimeOffset.UtcNow);
        var storeMock = new Mock<IUserStore>();
        storeMock.Setup(s => s.GetByEmail("user@test.com")).Returns(existing);

        var sut = CreateService(storeMock.Object);

        sut.Invoking(s => s.Register(new RegisterDto("user@test.com", "testuser", "Password1")))
            .Should().Throw<EmailAlreadyExistsException>();
    }

    [Fact]
    public void Login_ValidCredentials_ReturnsTokens()
    {
        var user = new User(Guid.NewGuid(), "user@test.com", "testuser", "hashed", UserRole.User, DateTimeOffset.UtcNow);
        var storeMock = new Mock<IUserStore>();
        storeMock.Setup(s => s.GetByEmail("user@test.com")).Returns(user);

        var hasherMock = new Mock<IPasswordHasher>();
        hasherMock.Setup(h => h.Verify("Password1", "hashed")).Returns(true);

        var jwtMock = new Mock<IJwtService>();
        jwtMock.Setup(j => j.GenerateAccessToken(user)).Returns("access");
        jwtMock.Setup(j => j.GenerateRefreshToken()).Returns("refresh");

        var sut = CreateService(storeMock.Object, jwtMock.Object, hasherMock.Object);

        var result = sut.Login(new LoginDto("user@test.com", "Password1"));

        result.AccessToken.Should().Be("access");
    }

    [Fact]
    public void Login_WrongPassword_ThrowsInvalidCredentialsException()
    {
        var user = new User(Guid.NewGuid(), "user@test.com", "testuser", "hashed", UserRole.User, DateTimeOffset.UtcNow);
        var storeMock = new Mock<IUserStore>();
        storeMock.Setup(s => s.GetByEmail("user@test.com")).Returns(user);

        var hasherMock = new Mock<IPasswordHasher>();
        hasherMock.Setup(h => h.Verify(It.IsAny<string>(), "hashed")).Returns(false);

        var sut = CreateService(storeMock.Object, hasher: hasherMock.Object);

        sut.Invoking(s => s.Login(new LoginDto("user@test.com", "WrongPass")))
            .Should().Throw<InvalidCredentialsException>();
    }

    [Fact]
    public void Login_UnknownEmail_ThrowsInvalidCredentialsException()
    {
        var storeMock = new Mock<IUserStore>();
        storeMock.Setup(s => s.GetByEmail(It.IsAny<string>())).Returns((User?)null);

        var sut = CreateService(storeMock.Object);

        sut.Invoking(s => s.Login(new LoginDto("no@one.com", "Password1")))
            .Should().Throw<InvalidCredentialsException>();
    }

    [Fact]
    public void Refresh_ActiveToken_RotatesTokenAndReturnsNew()
    {
        var userId = Guid.NewGuid();
        var user = new User(userId, "u@t.com", "tester", "h", UserRole.User, DateTimeOffset.UtcNow);
        var oldToken = new RefreshToken("old-token", userId, DateTimeOffset.UtcNow.AddDays(1));

        var storeMock = new Mock<IUserStore>();
        storeMock.Setup(s => s.GetRefreshToken("old-token")).Returns(oldToken);
        storeMock.Setup(s => s.GetById(userId)).Returns(user);

        var jwtMock = new Mock<IJwtService>();
        jwtMock.Setup(j => j.GenerateAccessToken(user)).Returns("new-access");
        jwtMock.Setup(j => j.GenerateRefreshToken()).Returns("new-refresh");

        var sut = CreateService(storeMock.Object, jwtMock.Object);

        var result = sut.Refresh("old-token");

        result.AccessToken.Should().Be("new-access");
        result.RefreshToken.Should().Be("new-refresh");
        storeMock.Verify(s => s.RevokeRefreshToken("old-token"), Times.Once);
    }

    [Fact]
    public void Refresh_ExpiredToken_ThrowsInvalidRefreshTokenException()
    {
        var expired = new RefreshToken("old", Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(-1));
        var storeMock = new Mock<IUserStore>();
        storeMock.Setup(s => s.GetRefreshToken("old")).Returns(expired);

        var sut = CreateService(storeMock.Object);

        sut.Invoking(s => s.Refresh("old"))
            .Should().Throw<InvalidRefreshTokenException>();
    }

    [Fact]
    public void Refresh_UnknownToken_ThrowsInvalidRefreshTokenException()
    {
        var storeMock = new Mock<IUserStore>();
        storeMock.Setup(s => s.GetRefreshToken(It.IsAny<string>())).Returns((RefreshToken?)null);

        var sut = CreateService(storeMock.Object);

        sut.Invoking(s => s.Refresh("ghost"))
            .Should().Throw<InvalidRefreshTokenException>();
    }

    [Fact]
    public void Logout_RevokesToken()
    {
        var storeMock = new Mock<IUserStore>();
        var sut = CreateService(storeMock.Object);

        sut.Logout("some-token");

        storeMock.Verify(s => s.RevokeRefreshToken("some-token"), Times.Once);
    }
}
