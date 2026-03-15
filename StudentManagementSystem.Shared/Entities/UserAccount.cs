using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StudentManagementSystem.Shared.Constants;
using StudentManagementSystem.Shared.Enums;

namespace StudentManagementSystem.Shared.Entities;

public class UserAccount
{
    [Key]
    public int UserAccountId { get; set; }

    [Required]
    [StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Password { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedDate { get; set; }

    public Student? Student { get; set; }

    public Lecturer? Lecturer { get; set; }

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [NotMapped]
    public string RoleName => Role switch
    {
        UserRole.Lecturer => AppRoles.Lecturer,
        UserRole.Student => AppRoles.Student,
        _ => string.Empty
    };
}
