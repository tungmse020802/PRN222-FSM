using System.ComponentModel.DataAnnotations;
using StudentManagementSystem.Shared.Enums;

namespace StudentManagementSystem.Shared.Entities;

public class AIRecommendation
{
    [Key]
    public int AIRecommendationId { get; set; }

    public int StudentId { get; set; }

    public int SemesterId { get; set; }

    public RecommendationLevel RiskLevel { get; set; } = RecommendationLevel.Balanced;

    public int RecommendedCredits { get; set; }

    [Required]
    [StringLength(2000)]
    public string Summary { get; set; } = string.Empty;

    [StringLength(2000)]
    public string RecommendedSubjects { get; set; } = string.Empty;

    [StringLength(1000)]
    public string AvoidSubjects { get; set; } = string.Empty;

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public Student? Student { get; set; }

    public Semester? Semester { get; set; }
}
