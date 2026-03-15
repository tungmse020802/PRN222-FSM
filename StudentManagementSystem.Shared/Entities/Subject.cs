using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.Shared.Entities;

public class Subject
{
    [Key]
    public int SubjectId { get; set; }

    [Required]
    [StringLength(30)]
    public string SubjectCode { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string SubjectName { get; set; } = string.Empty;

    public int Credits { get; set; }

    public int TheoryHours { get; set; }

    public int PracticeHours { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<SubjectPrerequisite> PrerequisiteLinks { get; set; } = new List<SubjectPrerequisite>();

    public ICollection<SubjectPrerequisite> RequiredForLinks { get; set; } = new List<SubjectPrerequisite>();

    public ICollection<CourseSection> CourseSections { get; set; } = new List<CourseSection>();
}
