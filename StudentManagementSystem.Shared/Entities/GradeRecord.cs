using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.Shared.Entities;

public class GradeRecord
{
    [Key]
    public int GradeRecordId { get; set; }

    public int EnrollmentId { get; set; }

    public decimal? AssignmentScore { get; set; }

    public decimal? QuizScore { get; set; }

    public decimal? MidtermScore { get; set; }

    public decimal? FinalScore { get; set; }

    public decimal TotalScore { get; set; }

    [StringLength(5)]
    public string LetterGrade { get; set; } = "N/A";

    public bool IsPassed { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Enrollment? Enrollment { get; set; }
}
