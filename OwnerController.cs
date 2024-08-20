using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Camping_retake.Data;
using Camping_retake.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Camping_retake.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OwnerController : ControllerBase
    {
        private readonly LiteDbContext _database;
        private readonly ILogger<OwnerController> _logger;

        // Constructor to initialize database and logger dependencies
        public OwnerController(LiteDbContext database, ILogger<OwnerController> logger)
        {
            _database = database;
            _logger = logger;
        }

        // Register a new owner
        [HttpPost("register")]
        public ActionResult<Owner> Register([FromBody] Owner newOwner)
        {
            // Check if the received owner data is null
            if (newOwner == null)
            {
                return BadRequest("Owner data is null.");
            }

            // Auto-generate a new unique owner ID
            newOwner.Id = GenerateNewOwnerId();

            _database.AddOwner(newOwner);

            return CreatedAtAction(nameof(Get), new { id = newOwner.Id }, newOwner);
        }

        // Login an owner
        [HttpPost("login")]
        public ActionResult Login([FromBody] LoginRequest loginRequest)
        {
            // Check if the received login data is null
            if (loginRequest == null)
            {
                return BadRequest("Login data is null.");
            }

            // Retrieve the owner from the database using the provided username
            var owner = _database.GetOwners().FirstOrDefault(o => o.Username == loginRequest.Username);

            // Check if the owner exists and if the password matches
            if (owner == null || owner.Password != loginRequest.Password)
            {
                return Unauthorized("Invalid username or password.");
            }

            HttpContext.Session.SetInt32("OwnerId", owner.Id);
            HttpContext.Session.SetString("OwnerUsername", owner.Username);
            HttpContext.Session.SetString("OwnerRole", owner.Role);

            return Ok(new { message = "Login successful", role = owner.Role, username = owner.Username });
        }

        // Logout an owner
        [HttpPost("logout")]
        public IActionResult Logout()
        {       
            HttpContext.Session.Remove("OwnerUsername");
            HttpContext.Session.Remove("OwnerId");

            return Ok("Logged out successfully.");
        }

        // Check if the owner is logged in
        [HttpGet("check-session")]
        public IActionResult CheckSession()
        {
            var username = HttpContext.Session.GetString("OwnerUsername");

            // Check if the username is empty, indicating no active session
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized("No active session.");
            }

            return Ok($"Owner {username} is logged in.");
        }

        // Retrieve a list of all owners
        /*[HttpGet]
        public IEnumerable<Owner> Get()
        {
            // Ensure the user is authenticated by checking the session
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("OwnerUsername")))
            {
                return Enumerable.Empty<Owner>();
            }

            // Return the list of owners from the database
            return _database.GetOwners();
        }*/

        // Retrieve a specific owner by ID
        [HttpGet("{id}")]
        public ActionResult<Owner> Get(int id)
        {
            // Ensure the user is authenticated by checking the session
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("OwnerUsername")))
            {
                return Unauthorized("You must be logged in to view this.");
            }

            var owner = _database.GetOwnerById(id);

            // Check if the owner exists
            if (owner == null)
            {
                return NotFound();
            }

            return owner;
        }

        // Private method to generate a new unique owner ID
        private int GenerateNewOwnerId()
        {
            // Get the list of all owners from the database
            var owners = _database.GetOwners();

            // Check if there are no owners, if so, return 1 as the first ID
            if (!owners.Any())
            {
                return 1;
            }

            // Otherwise, return the maximum existing ID plus one
            return owners.Max(o => o.Id) + 1;
        }
    }
}
