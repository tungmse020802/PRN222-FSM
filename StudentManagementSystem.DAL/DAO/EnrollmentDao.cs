using StudentManagementSystem.DAL.DAO.Interfaces;
using StudentManagementSystem.DAL.Data;
using StudentManagementSystem.Shared.Entities;
using StudentManagementSystem.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace StudentManagementSystem.DAL.DAO;

public sealed class EnrollmentDao(IDbContextFactory<StudentManagementDbContext> contextFactory) : IEnrollmentDao
{
    public async Task<Enrollment?> GetEnrollmentAsync(int studentId, int courseSectionId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await BuildEnrollmentQuery(context)
            .FirstOrDefaultAsync(x => x.StudentId == studentId && x.CourseSectionId == courseSectionId, cancellationToken);
    }

    public async Task UpdateEnrollmentAsync(Enrollment enrollment, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.Enrollments.Update(enrollment);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Enrollment>> GetStudentEnrollmentsAsync(int studentId, int? semesterId = null, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var query = BuildEnrollmentQuery(context).Where(x => x.StudentId == studentId);
        if (semesterId.HasValue)
        {
            query = query.Where(x => x.CourseSection!.SemesterId == semesterId.Value);
        }

        return await query
            .OrderByDescending(x => x.CourseSection!.Semester!.StartDate)
            .ThenBy(x => x.CourseSection!.SectionCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<int>> GetPassedSubjectIdsAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Enrollments.AsNoTracking()
            .Where(x => x.StudentId == studentId &&
                        x.Status != EnrollmentStatus.Cancelled &&
                        x.GradeRecord != null &&
                        x.GradeRecord.IsPassed)
            .Select(x => x.CourseSection!.SubjectId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetRegisteredCreditsAsync(int studentId, int semesterId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Enrollments.AsNoTracking()
            .Where(x =>
                x.StudentId == studentId &&
                x.Status == EnrollmentStatus.Registered &&
                x.CourseSection!.SemesterId == semesterId)
            .SumAsync(x => x.CourseSection!.Subject!.Credits, cancellationToken);
    }

    public async Task AddEnrollmentAsync(Enrollment enrollment, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.Enrollments.Add(enrollment);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateCourseSectionCapacityAsync(int courseSectionId, int delta, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var section = await context.CourseSections.FirstOrDefaultAsync(x => x.CourseSectionId == courseSectionId, cancellationToken);
        if (section is null)
        {
            return;
        }

        section.CurrentCapacity = Math.Max(0, section.CurrentCapacity + delta);
        if (section.CurrentCapacity >= section.MaxCapacity)
        {
            section.IsOpen = false;
        }
        else if (section.CurrentCapacity < section.MaxCapacity)
        {
            section.IsOpen = true;
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Enrollment>> GetGradeBookAsync(int courseSectionId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Enrollments.AsNoTracking()
            .Include(x => x.Student)
            .ThenInclude(x => x!.UserAccount)
            .Include(x => x.CourseSection)
            .ThenInclude(x => x!.Subject)
            .Include(x => x.GradeRecord)
            .Where(x => x.CourseSectionId == courseSectionId && x.Status != EnrollmentStatus.Cancelled)
            .OrderBy(x => x.Student!.StudentCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<GradeRecord?> GetGradeRecordAsync(int enrollmentId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.GradeRecords.AsNoTracking().FirstOrDefaultAsync(x => x.EnrollmentId == enrollmentId, cancellationToken);
    }

    public async Task SaveGradeRecordAsync(GradeRecord gradeRecord, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var existing = await context.GradeRecords.FirstOrDefaultAsync(x => x.EnrollmentId == gradeRecord.EnrollmentId, cancellationToken);

        if (existing is null)
        {
            context.GradeRecords.Add(gradeRecord);
        }
        else
        {
            existing.AssignmentScore = gradeRecord.AssignmentScore;
            existing.QuizScore = gradeRecord.QuizScore;
            existing.MidtermScore = gradeRecord.MidtermScore;
            existing.FinalScore = gradeRecord.FinalScore;
            existing.TotalScore = gradeRecord.TotalScore;
            existing.LetterGrade = gradeRecord.LetterGrade;
            existing.IsPassed = gradeRecord.IsPassed;
            existing.UpdatedAt = gradeRecord.UpdatedAt;
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int?> GetStudentUserAccountIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Students.AsNoTracking()
            .Where(x => x.StudentId == studentId)
            .Select(x => (int?)x.UserAccountId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<int>> GetSectionRecipientUserAccountIdsAsync(int courseSectionId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var studentIds = await context.Enrollments.AsNoTracking()
            .Where(x => x.CourseSectionId == courseSectionId && x.Status == EnrollmentStatus.Registered)
            .Select(x => x.Student!.UserAccountId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var lecturerId = await context.CourseSections.AsNoTracking()
            .Where(x => x.CourseSectionId == courseSectionId)
            .Select(x => x.Lecturer!.UserAccountId)
            .FirstOrDefaultAsync(cancellationToken);

        if (lecturerId != 0 && !studentIds.Contains(lecturerId))
        {
            studentIds.Add(lecturerId);
        }

        return studentIds;
    }

    private static IQueryable<Enrollment> BuildEnrollmentQuery(StudentManagementDbContext context) =>
        context.Enrollments.AsNoTracking()
            .Include(x => x.Student)
            .ThenInclude(x => x!.UserAccount)
            .Include(x => x.CourseSection)
            .ThenInclude(x => x!.Subject)
            .Include(x => x.CourseSection)
            .ThenInclude(x => x!.Semester)
            .Include(x => x.CourseSection)
            .ThenInclude(x => x!.Lecturer)
            .ThenInclude(x => x!.UserAccount)
            .Include(x => x.CourseSection)
            .ThenInclude(x => x!.ScheduleSlots)
            .Include(x => x.GradeRecord);
}
