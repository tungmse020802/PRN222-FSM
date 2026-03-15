using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.Shared.Entities;

public class Lecturer
{
    [Key]
    public int LecturerId { get; set; }

    public int UserAccountId { get; set; }

    [Required]
    [StringLength(30)]
    public string LecturerCode { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string Department { get; set; } = string.Empty;

    [StringLength(50)]
    public string? OfficeRoom { get; set; }

    public UserAccount? UserAccount { get; set; }

    public ICollection<CourseSection> CourseSections { get; set; } = new List<CourseSection>();
}
