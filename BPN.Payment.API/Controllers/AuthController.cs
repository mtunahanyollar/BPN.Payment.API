using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BPN.Payment.API.Services.AuthService;
using System.Text.Json.Serialization;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TokenService _tokenService;

    public AuthController(IConfiguration configuration, IHttpClientFactory httpClientFactory, TokenService tokenService)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
    }

    [HttpGet("linkedin/login")]
    public IActionResult LinkedInLogin()
    {
        var clientId = _configuration["LinkedIn:ClientId"];
        var redirectUri = _configuration["LinkedIn:RedirectUri"];
        var state = "random_state_string"; // CSRF koruması için
        var scope = "openid profile email";

        var linkedInAuthUrl = $"https://www.linkedin.com/oauth/v2/authorization?response_type=code&client_id={clientId}&redirect_uri={redirectUri}&scope={scope}&state={state}";

        return Redirect(linkedInAuthUrl);
    }

    [HttpGet("linkedin/callback")]
    public async Task<IActionResult> LinkedInCallback([FromQuery] string code, [FromQuery] string state)
    {
        if (string.IsNullOrEmpty(code))
        {
            return BadRequest("Authorization code is missing.");
        }

        var client = _httpClientFactory.CreateClient();
        var tokenRequest = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("redirect_uri", _configuration["LinkedIn:RedirectUri"]),
            new KeyValuePair<string, string>("client_id", _configuration["LinkedIn:ClientId"]),
            new KeyValuePair<string, string>("client_secret", _configuration["LinkedIn:ClientSecret"])
        });

        var tokenResponse = await client.PostAsync("https://www.linkedin.com/oauth/v2/accessToken", tokenRequest);
        var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
        try
        {
            var tokenData = JsonSerializer.Deserialize<LinkedInTokenResponse>(tokenJson);

            if (tokenData == null || string.IsNullOrEmpty(tokenData.AccessToken))
            {
                Console.WriteLine("❌ Deserialization failed! The object is null.");
                return BadRequest("Failed to retrieve access token from LinkedIn.");
            }

            Console.WriteLine($"✅ Access Token: {tokenData.AccessToken}");
            Console.WriteLine($"✅ ID Token: {tokenData.IdToken}");

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(tokenData.IdToken);

            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var name = jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value;

            var jwt = _tokenService.GenerateToken(email, "User");

            return Ok(new { token = jwt });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error during JSON deserialization: {ex.Message}");
            return StatusCode(500, "Error parsing LinkedIn response.");
        }
    }
}

public class LinkedInTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("id_token")]
    public string IdToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }
}