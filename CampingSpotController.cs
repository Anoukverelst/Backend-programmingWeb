using Camping_retake.Data;
using Camping_retake.Models;
using Microsoft.AspNetCore.Mvc;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using static Camping_retake.Data.LiteDbContext;

namespace Camping_retake.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CampingSpotController : ControllerBase
    {
        private readonly LiteDbContext _database;
        private readonly ILogger<CampingSpotController> _logger;

        public CampingSpotController(ILogger<CampingSpotController> logger)
        {
            _logger = logger;
            _database = new LiteDbContext(); 
        }

        // Retrieve a list of all camping spots
        [HttpGet]
        public IEnumerable<CampingSpot> GetAll()
        {
            return _database.GetCampingSpots(); 
        }

        // Retrieve a list of camping spots associated with the logged-in owner
        [HttpGet("owner")]
        public ActionResult<IEnumerable<CampingSpot>> GetByOwner()
        {
            var ownerId = HttpContext.Session.GetInt32("OwnerId");
            if (ownerId == null)
            {
                return Unauthorized("Owner is not logged in."); 
            }

            var campingSpots = _database.GetCampingSpots().Where(cs => cs.OwnerId == ownerId.Value);

            return Ok(campingSpots);
        }

        // Only an owner can create a new camping spot
        [HttpPost]
        public ActionResult<CampingSpot> Post([FromBody] CampingSpot newCampingSpot)
        {
            // Retrieve the ownerId from the session to ensure the user is an authenticated owner
            var ownerId = HttpContext.Session.GetInt32("OwnerId");
            if (ownerId == null)
            {
                return Unauthorized("Owner is not logged in."); // Return a 401 Unauthorized response if no owner is logged in
            }

            var owner = _database.GetOwnerById(ownerId.Value);
            if (owner == null)
            {
                return NotFound("Owner not found."); 
            }

            newCampingSpot.OwnerId = ownerId.Value;

            _database.AddCampingSpot(newCampingSpot);

            return CreatedAtAction(nameof(GetAll), new { id = newCampingSpot.Id }, newCampingSpot);
        }
    }
}
