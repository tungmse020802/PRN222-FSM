using System.ComponentModel.DataAnnotations;
using StudentManagementSystem.Shared.Enums;

namespace StudentManagementSystem.Shared.Entities;

public class Semester
{
    [Key]
    public int SemesterId { get; set; }

    [Required]
    [StringLength(30)]
    public string SemesterCode { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string SemesterName { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string SchoolYear { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime RegistrationStartDate { get; set; }

    public DateTime RegistrationEndDate { get; set; }

    public int MaxCreditsPerStudent { get; set; } = 18;

    public SemesterStatus Status { get; set; } = SemesterStatus.Planned;

    public bool IsActive { get; set; } = true;

    public ICollection<CourseSection> CourseSections { get; set; } = new List<CourseSection>();

    public ICollection<AIRecommendation> Recommendations { get; set; } = new List<AIRecommendation>();
}
