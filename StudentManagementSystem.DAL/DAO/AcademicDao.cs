using StudentManagementSystem.DAL.DAO.Interfaces;
using StudentManagementSystem.DAL.Data;
using StudentManagementSystem.Shared.Entities;
using StudentManagementSystem.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace StudentManagementSystem.DAL.DAO;

public sealed class AcademicDao(IDbContextFactory<StudentManagementDbContext> contextFactory) : IAcademicDao
{
    public async Task<IReadOnlyList<Subject>> GetSubjectsAsync(string? keyword = null, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var query = context.Subjects.AsNoTracking()
            .Include(x => x.PrerequisiteLinks)
            .ThenInclude(x => x.PrerequisiteSubject)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var search = keyword.Trim();
            query = query.Where(x => x.SubjectCode.Contains(search) || x.SubjectName.Contains(search));
        }

        return await query.OrderBy(x => x.SubjectCode).ToListAsync(cancellationToken);
    }

    public async Task<Subject?> GetSubjectByIdAsync(int subjectId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Subjects.AsNoTracking()
            .Include(x => x.PrerequisiteLinks)
            .ThenInclude(x => x.PrerequisiteSubject)
            .FirstOrDefaultAsync(x => x.SubjectId == subjectId, cancellationToken);
    }

    public async Task<bool> SubjectCodeExistsAsync(string subjectCode, int? excludeSubjectId = null, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var query = context.Subjects.AsNoTracking().Where(x => x.SubjectCode.ToLower() == subjectCode.ToLower());
        if (excludeSubjectId.HasValue)
        {
            query = query.Where(x => x.SubjectId != excludeSubjectId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> SubjectHasCourseSectionsAsync(int subjectId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.CourseSections.AsNoTracking().AnyAsync(x => x.SubjectId == subjectId, cancellationToken);
    }

    public async Task AddSubjectAsync(Subject subject, IEnumerable<int> prerequisiteIds, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        subject.PrerequisiteLinks = prerequisiteIds.Distinct()
            .Where(x => x != subject.SubjectId)
            .Select(x => new SubjectPrerequisite { PrerequisiteSubjectId = x })
            .ToList();
        context.Subjects.Add(subject);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateSubjectAsync(Subject subject, IEnumerable<int> prerequisiteIds, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var existing = await context.Subjects.Include(x => x.PrerequisiteLinks)
            .FirstOrDefaultAsync(x => x.SubjectId == subject.SubjectId, cancellationToken);

        if (existing is null)
        {
            return;
        }

        existing.SubjectCode = subject.SubjectCode;
        existing.SubjectName = subject.SubjectName;
        existing.Credits = subject.Credits;
        existing.TheoryHours = subject.TheoryHours;
        existing.PracticeHours = subject.PracticeHours;
        existing.IsActive = subject.IsActive;
        context.SubjectPrerequisites.RemoveRange(existing.PrerequisiteLinks);
        existing.PrerequisiteLinks = prerequisiteIds.Distinct()
            .Where(x => x != existing.SubjectId)
            .Select(x => new SubjectPrerequisite
            {
                SubjectId = existing.SubjectId,
                PrerequisiteSubjectId = x
            }).ToList();

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task SetSubjectActiveAsync(int subjectId, bool isActive, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var existing = await context.Subjects.FirstOrDefaultAsync(x => x.SubjectId == subjectId, cancellationToken);
        if (existing is null)
        {
            return;
        }

        existing.IsActive = isActive;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Semester>> GetSemestersAsync(string? keyword = null, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var query = context.Semesters.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var search = keyword.Trim();
            query = query.Where(x => x.SemesterCode.Contains(search) || x.SemesterName.Contains(search) || x.SchoolYear.Contains(search));
        }

        return await query.OrderByDescending(x => x.StartDate).ToListAsync(cancellationToken);
    }

    public async Task<Semester?> GetSemesterByIdAsync(int semesterId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Semesters.AsNoTracking().FirstOrDefaultAsync(x => x.SemesterId == semesterId, cancellationToken);
    }

    public async Task<bool> SemesterCodeExistsAsync(string semesterCode, int? excludeSemesterId = null, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var query = context.Semesters.AsNoTracking().Where(x => x.SemesterCode.ToLower() == semesterCode.ToLower());
        if (excludeSemesterId.HasValue)
        {
            query = query.Where(x => x.SemesterId != excludeSemesterId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddSemesterAsync(Semester semester, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.Semesters.Add(semester);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateSemesterAsync(Semester semester, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.Semesters.Update(semester);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CourseSection>> GetCourseSectionsAsync(
        int? semesterId = null,
        int? lecturerId = null,
        string? keyword = null,
        bool? openOnly = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var query = BuildCourseSectionQuery(context);

        if (semesterId.HasValue)
        {
            query = query.Where(x => x.SemesterId == semesterId.Value);
        }

        if (lecturerId.HasValue)
        {
            query = query.Where(x => x.LecturerId == lecturerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var search = keyword.Trim();
            query = query.Where(x =>
                x.SectionCode.Contains(search) ||
                x.SectionName.Contains(search) ||
                x.Subject!.SubjectCode.Contains(search) ||
                x.Subject.SubjectName.Contains(search));
        }

        if (openOnly.HasValue)
        {
            query = query.Where(x => x.IsOpen == openOnly.Value);
        }

        return await query.OrderByDescending(x => x.Semester!.StartDate).ThenBy(x => x.SectionCode).ToListAsync(cancellationToken);
    }

    public async Task<CourseSection?> GetCourseSectionByIdAsync(int courseSectionId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await BuildCourseSectionQuery(context).FirstOrDefaultAsync(x => x.CourseSectionId == courseSectionId, cancellationToken);
    }

    public async Task<bool> SectionCodeExistsAsync(string sectionCode, int? excludeSectionId = null, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var query = context.CourseSections.AsNoTracking().Where(x => x.SectionCode.ToLower() == sectionCode.ToLower());
        if (excludeSectionId.HasValue)
        {
            query = query.Where(x => x.CourseSectionId != excludeSectionId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddCourseSectionAsync(CourseSection courseSection, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.CourseSections.Add(courseSection);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateCourseSectionAsync(CourseSection courseSection, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.CourseSections.Update(courseSection);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ScheduleSlot>> GetScheduleSlotsAsync(
        int? semesterId = null,
        int? courseSectionId = null,
        int? lecturerId = null,
        int? studentId = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var query = BuildScheduleQuery(context);

        if (semesterId.HasValue)
        {
            query = query.Where(x => x.CourseSection!.SemesterId == semesterId.Value);
        }

        if (courseSectionId.HasValue)
        {
            query = query.Where(x => x.CourseSectionId == courseSectionId.Value);
        }

        if (lecturerId.HasValue)
        {
            query = query.Where(x => x.CourseSection!.LecturerId == lecturerId.Value);
        }

        if (studentId.HasValue)
        {
            query = query.Where(x =>
                x.CourseSection!.Enrollments.Any(e =>
                    e.StudentId == studentId.Value &&
                    e.Status == EnrollmentStatus.Registered));
        }

        return await query
            .OrderBy(x => x.DayOfWeek)
            .ThenBy(x => x.SessionSlot)
            .ThenBy(x => x.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<ScheduleSlot?> GetScheduleSlotByIdAsync(int scheduleSlotId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await BuildScheduleQuery(context).FirstOrDefaultAsync(x => x.ScheduleSlotId == scheduleSlotId, cancellationToken);
    }

    public async Task AddScheduleSlotAsync(ScheduleSlot scheduleSlot, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.ScheduleSlots.Add(scheduleSlot);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateScheduleSlotAsync(ScheduleSlot scheduleSlot, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.ScheduleSlots.Update(scheduleSlot);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteScheduleSlotAsync(int scheduleSlotId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var existing = await context.ScheduleSlots.FirstOrDefaultAsync(x => x.ScheduleSlotId == scheduleSlotId, cancellationToken);
        if (existing is null)
        {
            return;
        }

        context.ScheduleSlots.Remove(existing);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasScheduleConflictAsync(int studentId, ScheduleSlot candidateSlot, int? excludeCourseSectionId = null, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var query = context.ScheduleSlots.AsNoTracking()
            .Where(x =>
                x.CourseSection!.Enrollments.Any(e =>
                    e.StudentId == studentId &&
                    e.Status == EnrollmentStatus.Registered));

        if (excludeCourseSectionId.HasValue)
        {
            query = query.Where(x => x.CourseSectionId != excludeCourseSectionId.Value);
        }

        var existingSlots = await query.ToListAsync(cancellationToken);
        return existingSlots.Any(existing =>
            existing.DayOfWeek == candidateSlot.DayOfWeek &&
            existing.SessionSlot == candidateSlot.SessionSlot &&
            existing.StartDate <= candidateSlot.EndDate &&
            candidateSlot.StartDate <= existing.EndDate);
    }

    private static IQueryable<CourseSection> BuildCourseSectionQuery(StudentManagementDbContext context) =>
        context.CourseSections.AsNoTracking()
            .Include(x => x.Subject)
            .Include(x => x.Semester)
            .Include(x => x.Lecturer)
            .ThenInclude(x => x!.UserAccount)
            .Include(x => x.ScheduleSlots);

    private static IQueryable<ScheduleSlot> BuildScheduleQuery(StudentManagementDbContext context) =>
        context.ScheduleSlots.AsNoTracking()
            .Include(x => x.CourseSection)
            .ThenInclude(x => x!.Subject)
            .Include(x => x.CourseSection)
            .ThenInclude(x => x!.Semester)
            .Include(x => x.CourseSection)
            .ThenInclude(x => x!.Lecturer)
            .ThenInclude(x => x!.UserAccount)
            .Include(x => x.CourseSection)
            .ThenInclude(x => x!.Enrollments);
}
