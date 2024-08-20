using Microsoft.AspNetCore.Mvc; 
using Microsoft.Extensions.Logging; 
using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Security.Claims; 
using Camping_retake.Data; 
using Camping_retake.Models; 
using Microsoft.AspNetCore.Http; 
using Microsoft.IdentityModel.Tokens; 
using System.IdentityModel.Tokens.Jwt; 
using System.Text; 

namespace Camping_retake.Controllers
{
    
    [ApiController]
    [Route("User")]
    public class UserController : ControllerBase
    {
        private readonly LiteDbContext _database; 
        private readonly ILogger<UserController> _logger; 

        
        public UserController(LiteDbContext database, ILogger<UserController> logger)
        {
            _database = database;
            _logger = logger;
        }

        // Endpoint to register a new user
        [HttpPost("register")]
        public ActionResult<User> Register([FromBody] User newUser)
        {
            // Check if the incoming user data is null
            if (newUser == null)
            {
                return BadRequest("User data is null.");
            }

            // Ensure that both username and password are provided
            if (string.IsNullOrWhiteSpace(newUser.Username) || string.IsNullOrWhiteSpace(newUser.Password))
            {
                return BadRequest("Username and password are required.");
            }

            // BookingIds = null when registration
            if (newUser.BookingIds == null)
            {
                newUser.BookingIds = new List<int>();
            }

            // If the user ID is not set (or is 0), generate a new ID
            if (newUser.Id == 0)
            {
                newUser.Id = GenerateNewUserId();
            }

            try
            {
                
                _database.AddUser(newUser);
            }
            catch (Exception ex)
            {
                
                _logger.LogError(ex, "Error occurred while registering the user.");
                return StatusCode(500, "Internal server error. Please try again later.");
            }

            
            return CreatedAtAction(nameof(GetUser), new { id = newUser.Id }, newUser);
        }

        // Private method to generate a new unique user ID
        private int GenerateNewUserId()
        {
            var users = _database.GetUsers();

            
            if (!users.Any())
            {
                return 1;
            }

            
            return users.Max(u => u.Id) + 1;
        }

        // Endpoint to login a user
        [HttpPost("login")]
        public ActionResult Login([FromBody] LoginRequest loginRequest)
        {
            // Check if the incoming login data is null
            if (loginRequest == null)
            {
                return BadRequest("Login data is null.");
            }

            // Find the user by username in the database
            var user = _database.GetUsers().FirstOrDefault(u => u.Username == loginRequest.Username);

            // Check if the user exists and the password matches
            if (user == null || user.Password != loginRequest.Password)
            {
                return Unauthorized("Invalid username or password.");
            }

            // Store user information in session
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);

            
            return Ok(new { message = "Login successful", role = user.Role, username = user.Username });
        }

        // Endpoint to get the currently authenticated user's information
        [HttpGet("me")]
        public ActionResult<User> GetCurrentUser()
        {
            // Retrieve the user ID from the session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized("User is not logged in.");
            }

            // Get the user from the database using the ID
            var user = _database.GetUserById(userId.Value);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            
            return Ok(user);
        }

        // Endpoint to logout a user
        [HttpPost("logout")]
        public IActionResult Logout()
        {
           
            HttpContext.Session.Clear();
            return Ok(new { message = "Logout successful" });
        }

        // Endpoint to get user information by their ID
        [HttpGet("{id}")]
        public ActionResult<User> GetUser(int id)
        {
            
            var user = _database.GetUserById(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }
            return user;
        }

        // Endpoint to update user information
        [HttpPut("update")]
        public IActionResult UpdateUser([FromBody] User updatedUser)
        {
            // Retrieve the user ID from the session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized("User is not logged in.");
            }

            // Find the user in the database by their ID
            var user = _database.Users.Find(u => u.Id == userId.Value).FirstOrDefault();
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Update the user's details with the new data (only non-null fields)
            if (!string.IsNullOrEmpty(updatedUser.Username))
                user.Username = updatedUser.Username;

            if (!string.IsNullOrEmpty(updatedUser.Email))
                user.Email = updatedUser.Email;

            if (!string.IsNullOrEmpty(updatedUser.FullName))
                user.FullName = updatedUser.FullName;

            if (!string.IsNullOrEmpty(updatedUser.Password))
                user.Password = updatedUser.Password;

            // role and bookingIds are not meant to be updated
            _database.Users.Update(user);
            return Ok(user);
        }


        // Get bookings of the currently authenticated user
        [HttpGet("bookings")]
        public ActionResult<IEnumerable<Booking>> GetUserBookings()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized("User is not logged in.");
            }

            // Retrieve the bookings 
            var bookings = _database.GetBookings().Where(b => b.UserId == userId.Value).ToList();
            if (!bookings.Any())
            {
                return Ok(new List<Booking>()); // Return an empty list if no bookings are found
            }
            return Ok(bookings);
        }

        // Placeholder for generating a JWT token (commented out)
        /*
        private string GenerateJwtToken(User user)
        {
            // Define the claims that will be included in the JWT token
            var claims = new List<Claim>
             {
             new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
             new Claim(ClaimTypes.Name, user.Username),
             new Claim(ClaimTypes.Role, user.Role)
             };

            // Ensure the key is at least 256 bits (32 characters for UTF8 encoding)
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create the token with the specified claims and signing credentials
            var token = new JwtSecurityToken(
                issuer: "yourdomain.com",
                audience: "yourdomain.com",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            // Return the token as a string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        */
    }
}
