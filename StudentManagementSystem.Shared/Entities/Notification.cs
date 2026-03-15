using System.ComponentModel.DataAnnotations;
using StudentManagementSystem.Shared.Enums;

namespace StudentManagementSystem.Shared.Entities;

public class Notification
{
    [Key]
    public int NotificationId { get; set; }

    public int UserAccountId { get; set; }

    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Message { get; set; } = string.Empty;

    public NotificationType Type { get; set; } = NotificationType.System;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public UserAccount? UserAccount { get; set; }
}
