using System.ComponentModel.DataAnnotations;
using StudentManagementSystem.Shared.Enums;

namespace StudentManagementSystem.Shared.Entities;

public class Student
{
    [Key]
    public int StudentId { get; set; }

    public int UserAccountId { get; set; }

    [Required]
    [StringLength(30)]
    public string StudentCode { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public Gender Gender { get; set; }

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    [Required]
    [StringLength(120)]
    public string Major { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string Cohort { get; set; } = string.Empty;

    public AcademicStatus AcademicStatus { get; set; } = AcademicStatus.Active;

    public bool IsActive { get; set; } = true;

    public UserAccount? UserAccount { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public ICollection<AIRecommendation> Recommendations { get; set; } = new List<AIRecommendation>();
}
