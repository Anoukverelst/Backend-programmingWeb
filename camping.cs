using System;

namespace Camping_retake.Models
{
    public class CampingSpot
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public decimal PricePerNight { get; set; }
        public int MaxCapacity { get; set; }

        public int OwnerId { get; set; } 
    }
}
