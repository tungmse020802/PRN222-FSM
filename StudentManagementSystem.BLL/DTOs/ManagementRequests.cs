using StudentManagementSystem.Shared.Enums;

namespace StudentManagementSystem.BLL.DTOs;

public sealed class StudentUpsertRequest
{
    public int? StudentId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string StudentCode { get; init; } = string.Empty;
    public DateTime DateOfBirth { get; init; }
    public Gender Gender { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string? Address { get; init; }
    public string Major { get; init; } = string.Empty;
    public string Cohort { get; init; } = string.Empty;
    public AcademicStatus AcademicStatus { get; init; } = AcademicStatus.Active;
    public bool IsActive { get; init; } = true;
    public string? Password { get; init; }
}

public sealed class SubjectUpsertRequest
{
    public int? SubjectId { get; init; }
    public string SubjectCode { get; init; } = string.Empty;
    public string SubjectName { get; init; } = string.Empty;
    public int Credits { get; init; }
    public int TheoryHours { get; init; }
    public int PracticeHours { get; init; }
    public bool IsActive { get; init; } = true;
    public IReadOnlyCollection<int> PrerequisiteIds { get; init; } = Array.Empty<int>();
}

public sealed class SemesterUpsertRequest
{
    public int? SemesterId { get; init; }
    public string SemesterCode { get; init; } = string.Empty;
    public string SemesterName { get; init; } = string.Empty;
    public string SchoolYear { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public DateTime RegistrationStartDate { get; init; }
    public DateTime RegistrationEndDate { get; init; }
    public int MaxCreditsPerStudent { get; init; }
    public SemesterStatus Status { get; init; }
    public bool IsActive { get; init; } = true;
}

public sealed class CourseSectionUpsertRequest
{
    public int? CourseSectionId { get; init; }
    public string SectionCode { get; init; } = string.Empty;
    public string SectionName { get; init; } = string.Empty;
    public int SubjectId { get; init; }
    public int SemesterId { get; init; }
    public int LecturerId { get; init; }
    public int MaxCapacity { get; init; }
    public bool IsOpen { get; init; } = true;
}

public sealed class ScheduleSlotUpsertRequest
{
    public int? ScheduleSlotId { get; init; }
    public int CourseSectionId { get; init; }
    public string Room { get; init; } = string.Empty;
    public DayOfWeek DayOfWeek { get; init; }
    public int SessionSlot { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}

public sealed class GradeUpsertRequest
{
    public int CourseSectionId { get; init; }
    public int StudentId { get; init; }
    public decimal? AssignmentScore { get; init; }
    public decimal? QuizScore { get; init; }
    public decimal? MidtermScore { get; init; }
    public decimal? FinalScore { get; init; }
}

public sealed class NotificationComposeRequest
{
    public int? CourseSectionId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public IReadOnlyCollection<int> RecipientUserAccountIds { get; init; } = Array.Empty<int>();
}
