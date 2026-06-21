using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NGO_Connect_LoginAndRegister_API.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Message { get; set; }

        // Nullable int: If null, it means "All Roles"
        public int? TargetRoleId { get; set; }

        // Navigation Property (Optional, but good for EF)
        [ForeignKey("TargetRoleId")]
        public Role TargetRole { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}