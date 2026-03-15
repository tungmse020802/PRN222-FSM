using System.ComponentModel.DataAnnotations;
using StudentManagementSystem.Shared.Enums;

namespace StudentManagementSystem.Shared.Entities;

public class Enrollment
{
    [Key]
    public int EnrollmentId { get; set; }

    public int StudentId { get; set; }

    public int CourseSectionId { get; set; }

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Registered;

    public Student? Student { get; set; }

    public CourseSection? CourseSection { get; set; }

    public GradeRecord? GradeRecord { get; set; }
}
