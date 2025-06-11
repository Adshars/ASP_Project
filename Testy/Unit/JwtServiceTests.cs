using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WarehouseAPI.Model;

namespace Testy;

public class JwtServiceTests
{
    private readonly JwtService _jwtService;
    private readonly IConfiguration _configuration;

    // Test ustawieñ Jwt
    private const string TestSecretKey = "TestSecretKey1234567890TestSecretKey1234567890"; // Minimum 32 znaki
    private const string TestIssuer = "TestWarehouseAPI";
    private const string TestAudience = "TestWarehouseClients";
    private const string TestExpiryMinutes = "30";

    public JwtServiceTests()
    {
        // Konfiguracja
        var configurationData = new Dictionary<string, string>
        {
            ["JwtSettings:SecretKey"] = TestSecretKey,
            ["JwtSettings:Issuer"] = TestIssuer,
            ["JwtSettings:Audience"] = TestAudience,
            ["JwtSettings:ExpiryMinutes"] = TestExpiryMinutes
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData!)
            .Build();

        _jwtService = new JwtService(_configuration);
    }

    [Fact]
    public void GenerateToken_WithValidUser_ReturnsValidJwtToken()
    {
        // Arrange
        var user = CreateTestUser("testuser", "test@example.com", "user123");

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        // Weryfikacja formatu (3 czêœci oddzielone kropka)
        var parts = token.Split('.');
        Assert.Equal(3, parts.Length);
    }

    [Fact]
    public void GenerateToken_WithValidUser_ContainsCorrectClaims()
    {
        // Arrange
        var user = CreateTestUser("testuser", "test@example.com", "user123");

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        // Weryfikacja claimu
        var subClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        Assert.NotNull(subClaim);
        Assert.Equal("testuser", subClaim.Value);

        // Weryfikacja NameIdentifier (user ID)
        var nameIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        Assert.NotNull(nameIdClaim);
        Assert.Equal("user123", nameIdClaim.Value);

        // Weryfikacja JTI claim (token unikatowy ID)
        var jtiClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
        Assert.NotNull(jtiClaim);
        Assert.NotEmpty(jtiClaim.Value);

        // Weryfikacja czy GUID poprawny
        Assert.True(Guid.TryParse(jtiClaim.Value, out _));
    }

    [Fact]
    public void GenerateToken_WithValidUser_HasCorrectIssuerAndAudience()
    {
        // Arrange
        var user = CreateTestUser("testuser", "test@example.com", "user123");

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        Assert.Equal(TestIssuer, jsonToken.Issuer);
        Assert.Contains(TestAudience, jsonToken.Audiences);
    }

    [Fact]
    public void GenerateToken_WithValidUser_HasCorrectExpiration()
    {
        // Arrange
        var user = CreateTestUser("testuser", "test@example.com", "user123");
        var beforeGeneration = DateTime.UtcNow;

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        var expectedExpiry = beforeGeneration.AddMinutes(Convert.ToDouble(TestExpiryMinutes));
        var actualExpiry = jsonToken.ValidTo;

        // Ignorowanie ma³ych ró¿nic czasu (min 1 minuta)
        var timeDifference = Math.Abs((expectedExpiry - actualExpiry).TotalMinutes);
        Assert.True(timeDifference < 1, $"Koniec tokena w niespodziewanym czasie. Oczekiwano: {expectedExpiry}, Mamy: {actualExpiry}");
    }

    [Fact]
    public void GenerateToken_ForDifferentUsers_GeneratesDifferentTokens()
    {
        // Arrange
        var user1 = CreateTestUser("user1", "user1@example.com", "id1");
        var user2 = CreateTestUser("user2", "user2@example.com", "id2");

        // Act
        var token1 = _jwtService.GenerateToken(user1);
        var token2 = _jwtService.GenerateToken(user2);

        // Assert
        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void GenerateToken_ForSameUser_GeneratesDifferentTokens()
    {
        // Arrange
        var user = CreateTestUser("testuser", "test@example.com", "user123");

        // Act
        var token1 = _jwtService.GenerateToken(user);
        var token2 = _jwtService.GenerateToken(user);

        // Assert
        // Tokeny z osobnym JTI i timestampem
        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void GenerateToken_TokenCanBeValidatedWithSameKey()
    {
        // Arrange
        var user = CreateTestUser("testuser", "test@example.com", "user123");

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert - Czy mo¿na walidowaæ tym samym kluczem
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecretKey));

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = TestIssuer,
            ValidAudience = TestAudience,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.Zero
        };

        // Bez exception
        var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

        Assert.NotNull(principal);
        Assert.NotNull(validatedToken);
    }

    [Fact]
    public void GenerateToken_WithNullUser_ThrowsException()
    {
        // Arrange
        User? user = null;

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => _jwtService.GenerateToken(user!));
    }

    [Fact]
    public void GenerateToken_WithUserWithNullUserName_ThrowsException()
    {
        // Arrange
        var user = CreateTestUser(null, "test@example.com", "user123");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _jwtService.GenerateToken(user));
    }

    [Fact]
    public void GenerateToken_WithUserWithNullId_ThrowsException()
    {
        // Arrange
        var user = CreateTestUser("testuser", "test@example.com", null);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _jwtService.GenerateToken(user));
    }

    [Fact]
    public void GenerateToken_WithMissingSecretKey_ThrowsException()
    {
        // Arrange
        var configWithoutSecretKey = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["JwtSettings:Issuer"] = TestIssuer,
                ["JwtSettings:Audience"] = TestAudience,
                ["JwtSettings:ExpiryMinutes"] = TestExpiryMinutes
            })
            .Build();

        var jwtService = new JwtService(configWithoutSecretKey);
        var user = CreateTestUser("testuser", "test@example.com", "user123");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => jwtService.GenerateToken(user));
    }

    [Fact]
    public void GenerateToken_WithInvalidExpiryMinutes_UsesDefaultOrThrows()
    {
        // Arrange
        var configWithInvalidExpiry = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["JwtSettings:SecretKey"] = TestSecretKey,
                ["JwtSettings:Issuer"] = TestIssuer,
                ["JwtSettings:Audience"] = TestAudience,
                ["JwtSettings:ExpiryMinutes"] = "invalid"
            })
            .Build();

        var jwtService = new JwtService(configWithInvalidExpiry);
        var user = CreateTestUser("testuser", "test@example.com", "user123");

        // Act & Assert
        // Mo¿e zwróciæ FormatException przez Convert.ToDouble
        Assert.Throws<FormatException>(() => jwtService.GenerateToken(user));
    }

    [Theory]
    [InlineData("user1", "user1@example.com", "id1")]
    [InlineData("admin", "admin@company.com", "admin123")]
    [InlineData("test.user", "test.user@domain.org", "testid456")]
    public void GenerateToken_WithVariousValidUsers_GeneratesValidTokens(string username, string email, string userId)
    {
        // Arrange
        var user = CreateTestUser(username, email, userId);

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        var subClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        Assert.Equal(username, subClaim?.Value);

        var nameIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        Assert.Equal(userId, nameIdClaim?.Value);
    }

    [Fact]
    public void GenerateToken_VerifyTokenNotExpiredImmediately()
    {
        // Arrange
        var user = CreateTestUser("testuser", "test@example.com", "user123");

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        Assert.True(jsonToken.ValidTo > DateTime.UtcNow);
    }

    private static User CreateTestUser(string? username, string email, string? id)
    {
        return new User
        {
            Id = id!,
            UserName = username,
            Email = email
        };
    }
}
