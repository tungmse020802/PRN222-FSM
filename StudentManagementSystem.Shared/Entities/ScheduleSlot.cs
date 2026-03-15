using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.Shared.Entities;

public class ScheduleSlot
{
    [Key]
    public int ScheduleSlotId { get; set; }

    public int CourseSectionId { get; set; }

    [Required]
    [StringLength(50)]
    public string Room { get; set; } = string.Empty;

    public DayOfWeek DayOfWeek { get; set; }

    public int SessionSlot { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public CourseSection? CourseSection { get; set; }
}
