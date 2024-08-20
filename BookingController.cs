using Camping_retake.Data;
using Camping_retake.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Camping_retake.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly LiteDbContext _database;
        private readonly ILogger<BookingController> _logger;


        public BookingController(LiteDbContext database, ILogger<BookingController> logger)
        {
            _database = database;
            _logger = logger;
        }

        // Create a new booking for a camping spot
        [HttpPost]
        public ActionResult<Booking> CreateBooking([FromBody] Booking newBooking)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized("User not logged in.");
            }

            // Verify that the camping spot exists in the database
            var campingSpot = _database.GetCampingSpotById(newBooking.CampingSpotId);
            if (campingSpot == null)
            {
                return NotFound("Camping spot not found"); 
            }

            // Assign the logged-in user's ID to the booking
            newBooking.UserId = userId.Value;

            // Auto-generate a new unique booking ID
            newBooking.Id = GenerateNewBookingId();


            _database.AddBooking(newBooking);

            return CreatedAtAction(nameof(GetBooking), new { id = newBooking.Id }, newBooking);
        }

        // Retrieve a specific booking by its ID
        [HttpGet("{id}")]
        public ActionResult<Booking> GetBooking(int id)
        {
            // Find the booking by ID in the database
            var booking = _database.GetBookingById(id);
            if (booking == null)
            {
                return NotFound(); 
            }

            return booking;
        }

        // Retrieve all bookings made by the logged-in user
        [HttpGet("mybookings")]
        public ActionResult<IEnumerable<Booking>> GetMyBookings()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized("User not logged in."); 
            }

            var bookings = _database.GetBookings().Where(b => b.UserId == userId.Value).ToList();

            return Ok(bookings.Any() ? bookings : new List<Booking>());
        }

        private int GenerateNewBookingId()
        {
            var bookings = _database.GetBookings();

            if (!bookings.Any())
            {
                return 1;
            }
            return bookings.Max(b => b.Id) + 1;
        }

        /*// DELETE: /Booking/{id}
        // Delete a specific booking by its ID
        [HttpDelete("{id}")]
        public IActionResult DeleteBooking(int id)
        {
            // Attempt to delete the booking from the database
            var success = _database.DeleteBooking(id);
            if (!success)
            {
                return NotFound(); // Return 404 Not Found if the booking doesn't exist or couldn't be deleted
            }

            // Return 204 No Content to indicate successful deletion
            return NoContent();
        }*/
    }
}
