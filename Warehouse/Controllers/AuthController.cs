using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WarehouseAPI.Model;
using Microsoft.Extensions.Logging;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly JwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, JwtService jwtService, ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var user = new User { UserName = dto.Username, Email = dto.Email };
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Nieudana próba rejestracji użytkownika: {Username}. Błędy: {Errors}", dto.Username, string.Join(", ", result.Errors.Select(e => e.Description)));

            return BadRequest(result.Errors);
        }
        _logger.LogInformation("Zarejestrowano nowego użytkownika: {Username}, email: {Email}", dto.Username, dto.Email);

        return Ok("User registered");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.Username);
        if (user == null)
        {
            _logger.LogWarning("Nieudane logowanie. Użytkownik {Username} nie istnieje.", dto.Username);

            return Unauthorized("Invalid credentials");
        }
        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Nieudane logowanie dla użytkownika {Username} - nieprawidłowe hasło.", dto.Username);

            return Unauthorized("Invalid credentials");
        }


        var token = _jwtService.GenerateToken(user);
        _logger.LogInformation("Użytkownik {Username} zalogował się pomyślnie.", dto.Username);

        return Ok(new { Token = token });
    }
}
