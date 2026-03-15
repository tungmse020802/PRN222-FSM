using StudentManagementSystem.Shared.Entities;

namespace StudentManagementSystem.DAL.DAO.Interfaces;

public interface IAcademicDao
{
    Task<IReadOnlyList<Subject>> GetSubjectsAsync(string? keyword = null, CancellationToken cancellationToken = default);
    Task<Subject?> GetSubjectByIdAsync(int subjectId, CancellationToken cancellationToken = default);
    Task<bool> SubjectCodeExistsAsync(string subjectCode, int? excludeSubjectId = null, CancellationToken cancellationToken = default);
    Task<bool> SubjectHasCourseSectionsAsync(int subjectId, CancellationToken cancellationToken = default);
    Task AddSubjectAsync(Subject subject, IEnumerable<int> prerequisiteIds, CancellationToken cancellationToken = default);
    Task UpdateSubjectAsync(Subject subject, IEnumerable<int> prerequisiteIds, CancellationToken cancellationToken = default);
    Task SetSubjectActiveAsync(int subjectId, bool isActive, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Semester>> GetSemestersAsync(string? keyword = null, CancellationToken cancellationToken = default);
    Task<Semester?> GetSemesterByIdAsync(int semesterId, CancellationToken cancellationToken = default);
    Task<bool> SemesterCodeExistsAsync(string semesterCode, int? excludeSemesterId = null, CancellationToken cancellationToken = default);
    Task AddSemesterAsync(Semester semester, CancellationToken cancellationToken = default);
    Task UpdateSemesterAsync(Semester semester, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CourseSection>> GetCourseSectionsAsync(
        int? semesterId = null,
        int? lecturerId = null,
        string? keyword = null,
        bool? openOnly = null,
        CancellationToken cancellationToken = default);
    Task<CourseSection?> GetCourseSectionByIdAsync(int courseSectionId, CancellationToken cancellationToken = default);
    Task<bool> SectionCodeExistsAsync(string sectionCode, int? excludeSectionId = null, CancellationToken cancellationToken = default);
    Task AddCourseSectionAsync(CourseSection courseSection, CancellationToken cancellationToken = default);
    Task UpdateCourseSectionAsync(CourseSection courseSection, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ScheduleSlot>> GetScheduleSlotsAsync(
        int? semesterId = null,
        int? courseSectionId = null,
        int? lecturerId = null,
        int? studentId = null,
        CancellationToken cancellationToken = default);
    Task<ScheduleSlot?> GetScheduleSlotByIdAsync(int scheduleSlotId, CancellationToken cancellationToken = default);
    Task AddScheduleSlotAsync(ScheduleSlot scheduleSlot, CancellationToken cancellationToken = default);
    Task UpdateScheduleSlotAsync(ScheduleSlot scheduleSlot, CancellationToken cancellationToken = default);
    Task DeleteScheduleSlotAsync(int scheduleSlotId, CancellationToken cancellationToken = default);
    Task<bool> HasScheduleConflictAsync(int studentId, ScheduleSlot candidateSlot, int? excludeCourseSectionId = null, CancellationToken cancellationToken = default);
}
