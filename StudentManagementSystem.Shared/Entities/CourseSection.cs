using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.Shared.Entities;

public class CourseSection
{
    [Key]
    public int CourseSectionId { get; set; }

    [Required]
    [StringLength(30)]
    public string SectionCode { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string SectionName { get; set; } = string.Empty;

    public int SubjectId { get; set; }

    public int SemesterId { get; set; }

    public int LecturerId { get; set; }

    public int MaxCapacity { get; set; }

    public int CurrentCapacity { get; set; }

    public bool IsOpen { get; set; } = true;

    public Subject? Subject { get; set; }

    public Semester? Semester { get; set; }

    public Lecturer? Lecturer { get; set; }

    public ICollection<ScheduleSlot> ScheduleSlots { get; set; } = new List<ScheduleSlot>();

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
