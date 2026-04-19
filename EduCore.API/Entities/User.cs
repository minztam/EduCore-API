using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduCore.API.Entities
{
    [Table("tb_User")]
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Password { get; set; }
        public string? AvatarUrl { get; set; }

        public string Provider { get; set; } = "Local";
        public string? ProviderId { get; set; }

        public string Role { get; set; } = "User";
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
