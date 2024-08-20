﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Camping_retake.Models
{
    public class Owner
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }
        public List<int> CampingSpotIds { get; set; } = new List<int>();
    }
}
