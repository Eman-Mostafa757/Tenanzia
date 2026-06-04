using System.ComponentModel.DataAnnotations;

namespace Tenanzia.API.DTOs
{
    public class InviteDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Employee"; // Employee or Manager
    }
}
