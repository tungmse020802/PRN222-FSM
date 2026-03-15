using StudentManagementSystem.BLL.Common;
using StudentManagementSystem.BLL.Interfaces;
using StudentManagementSystem.DAL.Repositories.Interfaces;
using StudentManagementSystem.Shared.Entities;
using StudentManagementSystem.Shared.Enums;

namespace StudentManagementSystem.BLL.Services;

public sealed class RecommendationService(
    IRecommendationRepository recommendationRepository,
    IAcademicRepository academicRepository,
    IEnrollmentRepository enrollmentRepository,
    INotificationRepository notificationRepository) : IRecommendationService
{
    public Task<AIRecommendation?> GetLatestAsync(int studentId, int semesterId, CancellationToken cancellationToken = default) =>
        recommendationRepository.GetLatestAsync(studentId, semesterId, cancellationToken);

    public async Task<ServiceResult<AIRecommendation>> GenerateAsync(int studentId, int userAccountId, int semesterId, CancellationToken cancellationToken = default)
    {
        var semester = await academicRepository.GetSemesterByIdAsync(semesterId, cancellationToken);
        if (semester is null)
        {
            return ServiceResult<AIRecommendation>.Failure("Semester not found.");
        }

        var transcript = await enrollmentRepository.GetStudentEnrollmentsAsync(studentId, null, cancellationToken);
        var graded = transcript.Where(x => x.Status != EnrollmentStatus.Cancelled && x.GradeRecord is not null && x.CourseSection?.Subject is not null).ToList();

        decimal gpa = 0m;
        if (graded.Count > 0)
        {
            var totalCredits = graded.Sum(x => x.CourseSection!.Subject!.Credits);
            var totalPoints = graded.Sum(x => ConvertToGpaPoint(x.GradeRecord!.TotalScore) * x.CourseSection!.Subject!.Credits);
            gpa = totalCredits == 0 ? 0m : Math.Round(totalPoints / totalCredits, 2);
        }

        var passedSubjectIds = await enrollmentRepository.GetPassedSubjectIdsAsync(studentId, cancellationToken);
        var availableSections = await academicRepository.GetCourseSectionsAsync(semesterId, null, null, true, cancellationToken);
        var availableSubjects = availableSections
            .Where(x => x.Subject is not null)
            .GroupBy(x => x.SubjectId)
            .Select(x => x.First().Subject!)
            .ToList();

        var eligibleSubjects = availableSubjects
            .Where(subject =>
                !passedSubjectIds.Contains(subject.SubjectId) &&
                subject.PrerequisiteLinks.All(link => passedSubjectIds.Contains(link.PrerequisiteSubjectId)))
            .ToList();

        var failedSubjects = graded
            .Where(x => x.GradeRecord is not null && !x.GradeRecord.IsPassed)
            .Select(x => x.CourseSection!.Subject!)
            .DistinctBy(x => x.SubjectId)
            .ToList();

        var recommendedCredits = gpa switch
        {
            < 2.0m => 12,
            >= 3.2m => 18,
            _ => 15
        };

        var riskLevel = gpa switch
        {
            < 2.0m => RecommendationLevel.Warning,
            >= 3.2m => RecommendationLevel.Strong,
            _ => RecommendationLevel.Balanced
        };

        var recommendedSubjectNames = failedSubjects
            .Concat(eligibleSubjects)
            .DistinctBy(x => x.SubjectId)
            .Take(5)
            .Select(x => $"{x.SubjectCode} - {x.SubjectName}")
            .ToList();

        var summary = gpa switch
        {
            < 2.0m => "Academic risk detected. Prioritize retaking failed or prerequisite subjects and keep a lighter credit load.",
            >= 3.2m => "Performance is strong. You can take a fuller load if prerequisites are satisfied.",
            _ => "Learning progress is stable. Keep a balanced set of subjects and avoid overloading difficult courses."
        };

        var recommendation = new AIRecommendation
        {
            StudentId = studentId,
            SemesterId = semesterId,
            RiskLevel = riskLevel,
            RecommendedCredits = recommendedCredits,
            Summary = $"GPA: {gpa:0.00}. {summary}",
            RecommendedSubjects = string.Join(", ", recommendedSubjectNames),
            AvoidSubjects = gpa < 2.0m
                ? "Avoid taking multiple advanced or 4-credit subjects in the same semester."
                : "Avoid subjects with overlapping schedules and missing prerequisites.",
            GeneratedAt = DateTime.UtcNow
        };

        await recommendationRepository.SaveAsync(recommendation, cancellationToken);
        await notificationRepository.AddAsync(new Notification
        {
            UserAccountId = userAccountId,
            Title = "New study recommendation",
            Message = $"Your study plan suggestion for {semester.SemesterCode} has been updated.",
            Type = NotificationType.Recommendation,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        return ServiceResult<AIRecommendation>.Success(recommendation);
    }

    private static decimal ConvertToGpaPoint(decimal score) => score switch
    {
        >= 8.5m => 4.0m,
        >= 7.0m => 3.0m,
        >= 5.5m => 2.0m,
        >= 4.0m => 1.0m,
        _ => 0m
    };
}
