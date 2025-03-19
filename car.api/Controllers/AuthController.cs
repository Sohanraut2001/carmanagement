using car.api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace car.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        // In a real application, use proper identity management
        // This is simplified for the example
        private static readonly List<User> _users = new List<User>
        {
            new User
            {
                Id = 1,
                Username = "admin",
                Password = "admin123", // In production, use password hashing
                Role = "Admin"
            },
            new User
            {
                Id = 2,
                Username = "salesman1",
                Password = "sales123",
                Role = "Salesman"
            }
        };

        public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("login")]
        public ActionResult<string> Login([FromBody] LoginModel model)
        {
            var user = _users.FirstOrDefault(u =>
                u.Username == model.Username &&
                u.Password == model.Password);

            if (user == null)
                return Unauthorized("Invalid username or password");

            var token = GenerateJwtToken(user);

            return Ok(new { Token = token });
        }

        [HttpGet("menu")]
        [Authorize]
        public ActionResult<List<MenuItem>> GetMenu()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var menu = GetMenuByRole(role);
            return Ok(menu);
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private List<MenuItem> GetMenuByRole(string role)
        {
            var menuItems = new List<MenuItem>();

            // Common menu items
            menuItems.Add(new MenuItem { Id = 1, Name = "Dashboard", Url = "/dashboard", Icon = "dashboard" });

            // Role-specific menu items
            if (role == "Admin")
            {
                menuItems.Add(new MenuItem { Id = 2, Name = "Car Models", Url = "/car-models", Icon = "directions_car" });
                menuItems.Add(new MenuItem
                {
                    Id = 3,
                    Name = "Reports",
                    Url = "#",
                    Icon = "assessment",
                    Children = new List<MenuItem>
                    {
                        new MenuItem { Id = 4, Name = "Commission Report", Url = "/reports/commission", Icon = "monetization_on" }
                    }
                });
            }
            else if (role == "Salesman")
            {
                menuItems.Add(new MenuItem { Id = 5, Name = "Car Models", Url = "/car-models/view", Icon = "directions_car" });
                menuItems.Add(new MenuItem { Id = 6, Name = "My Commission", Url = "/commission", Icon = "monetization_on" });
            }

            return menuItems;
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class MenuItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Icon { get; set; }
        public List<MenuItem> Children { get; set; } = new List<MenuItem>();
    }
}