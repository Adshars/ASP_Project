using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using WarehouseAPI.Model;
using WarehouseAPI.Controllers;
using WarehouseAPI.DTO;

namespace AuthControllerTests.Tests.Unit;

public class AuthControllerTests
{
    private AuthController GetController(
        Mock<UserManager<User>> userManagerMock,
        Mock<SignInManager<User>> signInManagerMock,
        Mock<JwtService> jwtServiceMock)
    {
        var loggerMock = new Mock<ILogger<AuthController>>();

        return new AuthController(
            userManagerMock.Object,
            signInManagerMock.Object,
            jwtServiceMock.Object,
            loggerMock.Object
        );
    }

    //Rejestracja użytkownika, poprawny użytkownik

    [Fact]
    public async Task Register_ValidUser_ReturnsOk()
    {
        // Arrange
        var dto = new RegisterDto { Username = "testuser", Email = "test@email.com", Password = "Password123!" };
        var userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
        userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var signInManagerMock = new Mock<SignInManager<User>>(
            userManagerMock.Object, Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<User>>(), null, null, null, null);

        var jwtServiceMock = new Mock<JwtService>(null);

        var controller = GetController(userManagerMock, signInManagerMock, jwtServiceMock);

        // Act
        var result = await controller.Register(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("User registered", okResult.Value);
    }

    //Rejestracja użytkownika, niepoprawny użytkownik

    [Fact]
    public async Task Register_InvalidUser_ReturnsBadRequest()
    {
        // Arrange
        var dto = new RegisterDto { Username = "baduser", Email = "bad@email.com", Password = "123" };
        var userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);

        userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Błąd" }));

        var signInManagerMock = new Mock<SignInManager<User>>(
            userManagerMock.Object, Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<User>>(), null, null, null, null);

        var jwtServiceMock = new Mock<JwtService>(null);

        var controller = GetController(userManagerMock, signInManagerMock, jwtServiceMock);

        // Act
        var result = await controller.Register(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    //Logowanie, użytkownik nie istnieje 

    [Fact]
    public async Task Login_UserDoesNotExist_ReturnsUnauthorized()
    {
        // Arrange
        var dto = new LoginDto { Username = "nouser", Password = "x" };
        var userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
        userManagerMock.Setup(um => um.FindByNameAsync(dto.Username))
            .ReturnsAsync((User)null);

        var signInManagerMock = new Mock<SignInManager<User>>(
            userManagerMock.Object, Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<User>>(), null, null, null, null);

        var jwtServiceMock = new Mock<JwtService>(null);

        var controller = GetController(userManagerMock, signInManagerMock, jwtServiceMock);

        // Act
        var result = await controller.Login(dto);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    //Logowanie użytkownika, niepoprawne hasło

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var dto = new LoginDto { Username = "testuser", Password = "wrongpass" };
        var user = new User { UserName = dto.Username, Email = "x@x.com" };

        var userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
        userManagerMock.Setup(um => um.FindByNameAsync(dto.Username))
            .ReturnsAsync(user);

        var signInManagerMock = new Mock<SignInManager<User>>(
            userManagerMock.Object, Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<User>>(), null, null, null, null);

        signInManagerMock.Setup(sm => sm.CheckPasswordSignInAsync(user, dto.Password, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        var jwtServiceMock = new Mock<JwtService>(null);

        var controller = GetController(userManagerMock, signInManagerMock, jwtServiceMock);

        // Act
        var result = await controller.Login(dto);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    //Logowanie użytkownika, poprawne dane

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var dto = new LoginDto { Username = "gooduser", Password = "GoodPass123" };
        var user = new User { UserName = dto.Username, Email = "good@email.com", Id = "testid123" };

        var userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
        userManagerMock.Setup(um => um.FindByNameAsync(dto.Username))
            .ReturnsAsync(user);

        var signInManagerMock = new Mock<SignInManager<User>>(
            userManagerMock.Object, Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<User>>(), null, null, null, null);

        signInManagerMock.Setup(sm => sm.CheckPasswordSignInAsync(user, dto.Password, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        // Konfiguracja testowa JWT
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(cfg => cfg[It.IsAny<string>()]).Returns((string key) =>
        {
            return key switch
            {
                "JwtSettings:SecretKey" => "THIS_IS_A_32_BYTE_SECRET_KEY_FOR_TESTS!", // >=32 znaki!
                "JwtSettings:Issuer" => "TestIssuer",
                "JwtSettings:Audience" => "TestAudience",
                "JwtSettings:ExpiryMinutes" => "60",
                _ => null
            };
        });
        var jwtService = new JwtService(configMock.Object);

        var loggerMock = new Mock<ILogger<AuthController>>();
        var controller = new AuthController(
            userManagerMock.Object,
            signInManagerMock.Object,
            jwtService,
            loggerMock.Object
        );

        // Act
        var result = await controller.Login(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var tokenObj = okResult.Value;

        // Refleksja na anonimowy obiekt
        var prop = tokenObj.GetType().GetProperty("Token");
        var token = prop?.GetValue(tokenObj) as string;

        Assert.NotNull(token);
        Assert.IsType<string>(token);
        Assert.False(string.IsNullOrWhiteSpace(token));
    }
}