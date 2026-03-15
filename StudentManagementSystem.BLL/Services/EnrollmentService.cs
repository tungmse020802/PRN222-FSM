using StudentManagementSystem.BLL.Common;
using StudentManagementSystem.BLL.DTOs;
using StudentManagementSystem.BLL.Interfaces;
using StudentManagementSystem.DAL.Repositories.Interfaces;
using StudentManagementSystem.Shared.Entities;
using StudentManagementSystem.Shared.Enums;

namespace StudentManagementSystem.BLL.Services;

public sealed class EnrollmentService(
    IEnrollmentRepository enrollmentRepository,
    IAcademicRepository academicRepository,
    IUserAccountRepository userAccountRepository,
    INotificationRepository notificationRepository) : IEnrollmentService
{
    public Task<IReadOnlyList<Enrollment>> GetStudentEnrollmentsAsync(int studentId, int? semesterId = null, CancellationToken cancellationToken = default) =>
        enrollmentRepository.GetStudentEnrollmentsAsync(studentId, semesterId, cancellationToken);

    public Task<IReadOnlyList<Enrollment>> GetGradeBookAsync(int courseSectionId, CancellationToken cancellationToken = default) =>
        enrollmentRepository.GetGradeBookAsync(courseSectionId, cancellationToken);

    public async Task<ServiceResult> RegisterAsync(int studentId, int userAccountId, int courseSectionId, CancellationToken cancellationToken = default)
    {
        var student = await userAccountRepository.GetStudentByIdAsync(studentId, cancellationToken);
        var section = await academicRepository.GetCourseSectionByIdAsync(courseSectionId, cancellationToken);

        if (student is null || section is null || section.Semester is null || section.Subject is null)
        {
            return ServiceResult.Failure("Student or course section was not found.");
        }

        if (!student.IsActive || student.AcademicStatus == AcademicStatus.Inactive || student.AcademicStatus == AcademicStatus.Suspended)
        {
            return ServiceResult.Failure("Inactive student cannot register.");
        }

        if (section.Semester.Status != SemesterStatus.OpenForRegistration ||
            DateTime.Today < section.Semester.RegistrationStartDate.Date ||
            DateTime.Today > section.Semester.RegistrationEndDate.Date)
        {
            return ServiceResult.Failure("Course registration is not open for this semester.");
        }

        if (!section.IsOpen || section.CurrentCapacity >= section.MaxCapacity)
        {
            return ServiceResult.Failure("This course section is full or closed.");
        }

        if (section.ScheduleSlots.Count == 0)
        {
            return ServiceResult.Failure("This course section has no teaching schedule yet.");
        }

        var classStartDate = section.ScheduleSlots.Min(x => x.StartDate.Date);
        if (DateTime.Today >= classStartDate)
        {
            return ServiceResult.Failure("This course section has already started. Registration is closed.");
        }

        var existing = await enrollmentRepository.GetEnrollmentAsync(studentId, courseSectionId, cancellationToken);
        if (existing is not null && existing.Status == EnrollmentStatus.Registered)
        {
            return ServiceResult.Failure("Student already registered this course section.");
        }

        var currentSemesterEnrollments = await enrollmentRepository.GetStudentEnrollmentsAsync(studentId, section.SemesterId, cancellationToken);
        var duplicateSubjectEnrollment = currentSemesterEnrollments.Any(x =>
            x.Status == EnrollmentStatus.Registered &&
            x.CourseSectionId != section.CourseSectionId &&
            x.CourseSection?.SubjectId == section.SubjectId);

        if (duplicateSubjectEnrollment)
        {
            return ServiceResult.Failure("Student already registered another class for this subject.");
        }

        var passedSubjectIds = await enrollmentRepository.GetPassedSubjectIdsAsync(studentId, cancellationToken);
        var subject = await academicRepository.GetSubjectByIdAsync(section.SubjectId, cancellationToken);
        var missingPrerequisites = subject?.PrerequisiteLinks
            .Where(x => !passedSubjectIds.Contains(x.PrerequisiteSubjectId))
            .Select(x => x.PrerequisiteSubject?.SubjectCode ?? string.Empty)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList() ?? [];

        if (missingPrerequisites.Count > 0)
        {
            return ServiceResult.Failure($"Missing prerequisite subjects: {string.Join(", ", missingPrerequisites)}.");
        }

        foreach (var candidateSlot in section.ScheduleSlots)
        {
            var hasConflict = await academicRepository.HasScheduleConflictAsync(studentId, candidateSlot, section.CourseSectionId, cancellationToken);
            if (hasConflict)
            {
                return ServiceResult.Failure("This course section conflicts with your current schedule.");
            }
        }

        var currentCredits = await enrollmentRepository.GetRegisteredCreditsAsync(studentId, section.SemesterId, cancellationToken);
        if (currentCredits + section.Subject.Credits > section.Semester.MaxCreditsPerStudent)
        {
            return ServiceResult.Failure("Registration exceeds the semester credit limit.");
        }

        if (existing is not null && existing.Status == EnrollmentStatus.Cancelled)
        {
            existing.Status = EnrollmentStatus.Registered;
            existing.RegisteredAt = DateTime.UtcNow;
            await enrollmentRepository.UpdateEnrollmentAsync(existing, cancellationToken);
        }
        else
        {
            await enrollmentRepository.AddEnrollmentAsync(new Enrollment
            {
                StudentId = studentId,
                CourseSectionId = courseSectionId,
                RegisteredAt = DateTime.UtcNow,
                Status = EnrollmentStatus.Registered
            }, cancellationToken);
        }

        await enrollmentRepository.UpdateCourseSectionCapacityAsync(courseSectionId, 1, cancellationToken);
        await notificationRepository.AddAsync(new Notification
        {
            UserAccountId = userAccountId,
            Title = "Registration successful",
            Message = $"You have registered {section.SectionCode} successfully.",
            Type = NotificationType.Registration,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> CancelAsync(int studentId, int userAccountId, int courseSectionId, CancellationToken cancellationToken = default)
    {
        var enrollment = await enrollmentRepository.GetEnrollmentAsync(studentId, courseSectionId, cancellationToken);
        if (enrollment is null || enrollment.Status != EnrollmentStatus.Registered || enrollment.CourseSection?.Semester is null)
        {
            return ServiceResult.Failure("Enrollment not found.");
        }

        var sectionStartDate = enrollment.CourseSection.ScheduleSlots.Count == 0
            ? enrollment.CourseSection.Semester.StartDate.Date
            : enrollment.CourseSection.ScheduleSlots.Min(x => x.StartDate.Date);

        if (DateTime.Today >= sectionStartDate)
        {
            return ServiceResult.Failure("Cancellation is closed because the class has already started.");
        }

        if (DateTime.Today > enrollment.CourseSection.Semester.RegistrationEndDate.Date)
        {
            return ServiceResult.Failure("Cancellation period has ended.");
        }

        enrollment.Status = EnrollmentStatus.Cancelled;
        await enrollmentRepository.UpdateEnrollmentAsync(enrollment, cancellationToken);
        await enrollmentRepository.UpdateCourseSectionCapacityAsync(courseSectionId, -1, cancellationToken);

        await notificationRepository.AddAsync(new Notification
        {
            UserAccountId = userAccountId,
            Title = "Registration cancelled",
            Message = $"You cancelled {enrollment.CourseSection.SectionCode} successfully.",
            Type = NotificationType.Registration,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<int>> SaveGradeAsync(int lecturerUserAccountId, GradeUpsertRequest request, CancellationToken cancellationToken = default)
    {
        if (!IsValidScore(request.AssignmentScore) ||
            !IsValidScore(request.QuizScore) ||
            !IsValidScore(request.MidtermScore) ||
            !IsValidScore(request.FinalScore))
        {
            return ServiceResult<int>.Failure("All scores must be between 0 and 10.");
        }

        var lecturer = await userAccountRepository.GetLecturerByUserAccountIdAsync(lecturerUserAccountId, cancellationToken);
        if (lecturer is null)
        {
            return ServiceResult<int>.Failure("Lecturer account not found.");
        }

        var section = await academicRepository.GetCourseSectionByIdAsync(request.CourseSectionId, cancellationToken);
        if (section is null || section.LecturerId != lecturer.LecturerId)
        {
            return ServiceResult<int>.Failure("Lecturer cannot update grades for this section.");
        }

        var enrollment = await enrollmentRepository.GetEnrollmentAsync(request.StudentId, request.CourseSectionId, cancellationToken);
        if (enrollment is null || enrollment.Status == EnrollmentStatus.Cancelled)
        {
            return ServiceResult<int>.Failure("Student is not enrolled in this course section.");
        }

        var total = CalculateTotalScore(request);
        var (letter, isPassed) = CalculateLetterGrade(total);

        await enrollmentRepository.SaveGradeRecordAsync(new GradeRecord
        {
            EnrollmentId = enrollment.EnrollmentId,
            AssignmentScore = request.AssignmentScore,
            QuizScore = request.QuizScore,
            MidtermScore = request.MidtermScore,
            FinalScore = request.FinalScore,
            TotalScore = total,
            LetterGrade = letter,
            IsPassed = isPassed,
            UpdatedAt = DateTime.UtcNow
        }, cancellationToken);

        var recipientUserAccountId = await enrollmentRepository.GetStudentUserAccountIdAsync(request.StudentId, cancellationToken);
        if (recipientUserAccountId.HasValue)
        {
            await notificationRepository.AddAsync(new Notification
            {
                UserAccountId = recipientUserAccountId.Value,
                Title = "New grade available",
                Message = $"New grade has been updated for {section.SectionCode}.",
                Type = NotificationType.Grade,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);
        }

        return recipientUserAccountId.HasValue
            ? ServiceResult<int>.Success(recipientUserAccountId.Value)
            : ServiceResult<int>.Failure("Grade saved, but student notification target was not found.");
    }

    public async Task<decimal> CalculateSemesterGpaAsync(int studentId, int? semesterId = null, CancellationToken cancellationToken = default)
    {
        var enrollments = await enrollmentRepository.GetStudentEnrollmentsAsync(studentId, semesterId, cancellationToken);
        var graded = enrollments
            .Where(x => x.Status != EnrollmentStatus.Cancelled && x.GradeRecord is not null && x.CourseSection?.Subject is not null)
            .Select(x => new
            {
                Credits = x.CourseSection!.Subject!.Credits,
                Point = ConvertToGpaPoint(x.GradeRecord!.TotalScore)
            })
            .ToList();

        if (graded.Count == 0)
        {
            return 0m;
        }

        var totalCredits = graded.Sum(x => x.Credits);
        var weighted = graded.Sum(x => x.Point * x.Credits);
        return totalCredits == 0 ? 0m : Math.Round(weighted / totalCredits, 2);
    }

    private static decimal CalculateTotalScore(GradeUpsertRequest request)
    {
        var assignment = request.AssignmentScore ?? 0m;
        var quiz = request.QuizScore ?? 0m;
        var midterm = request.MidtermScore ?? 0m;
        var final = request.FinalScore ?? 0m;
        return Math.Round((assignment * 0.2m) + (quiz * 0.1m) + (midterm * 0.2m) + (final * 0.5m), 2);
    }

    private static (string Letter, bool Passed) CalculateLetterGrade(decimal score) => score switch
    {
        >= 8.5m => ("A", true),
        >= 7.0m => ("B", true),
        >= 5.5m => ("C", true),
        >= 4.0m => ("D", true),
        _ => ("F", false)
    };

    private static decimal ConvertToGpaPoint(decimal score) => score switch
    {
        >= 8.5m => 4.0m,
        >= 7.0m => 3.0m,
        >= 5.5m => 2.0m,
        >= 4.0m => 1.0m,
        _ => 0m
    };

    private static bool IsValidScore(decimal? score) => !score.HasValue || (score.Value >= 0m && score.Value <= 10m);
}
