using StudentManagementSystem.DAL.DAO.Interfaces;
using StudentManagementSystem.DAL.Repositories.Interfaces;
using StudentManagementSystem.Shared.Entities;

namespace StudentManagementSystem.DAL.Repositories;

public sealed class AcademicRepository(IAcademicDao dao) : IAcademicRepository
{
    public Task<IReadOnlyList<Subject>> GetSubjectsAsync(string? keyword = null, CancellationToken cancellationToken = default) => dao.GetSubjectsAsync(keyword, cancellationToken);
    public Task<Subject?> GetSubjectByIdAsync(int subjectId, CancellationToken cancellationToken = default) => dao.GetSubjectByIdAsync(subjectId, cancellationToken);
    public Task<bool> SubjectCodeExistsAsync(string subjectCode, int? excludeSubjectId = null, CancellationToken cancellationToken = default) => dao.SubjectCodeExistsAsync(subjectCode, excludeSubjectId, cancellationToken);
    public Task<bool> SubjectHasCourseSectionsAsync(int subjectId, CancellationToken cancellationToken = default) => dao.SubjectHasCourseSectionsAsync(subjectId, cancellationToken);
    public Task AddSubjectAsync(Subject subject, IEnumerable<int> prerequisiteIds, CancellationToken cancellationToken = default) => dao.AddSubjectAsync(subject, prerequisiteIds, cancellationToken);
    public Task UpdateSubjectAsync(Subject subject, IEnumerable<int> prerequisiteIds, CancellationToken cancellationToken = default) => dao.UpdateSubjectAsync(subject, prerequisiteIds, cancellationToken);
    public Task SetSubjectActiveAsync(int subjectId, bool isActive, CancellationToken cancellationToken = default) => dao.SetSubjectActiveAsync(subjectId, isActive, cancellationToken);
    public Task<IReadOnlyList<Semester>> GetSemestersAsync(string? keyword = null, CancellationToken cancellationToken = default) => dao.GetSemestersAsync(keyword, cancellationToken);
    public Task<Semester?> GetSemesterByIdAsync(int semesterId, CancellationToken cancellationToken = default) => dao.GetSemesterByIdAsync(semesterId, cancellationToken);
    public Task<bool> SemesterCodeExistsAsync(string semesterCode, int? excludeSemesterId = null, CancellationToken cancellationToken = default) => dao.SemesterCodeExistsAsync(semesterCode, excludeSemesterId, cancellationToken);
    public Task AddSemesterAsync(Semester semester, CancellationToken cancellationToken = default) => dao.AddSemesterAsync(semester, cancellationToken);
    public Task UpdateSemesterAsync(Semester semester, CancellationToken cancellationToken = default) => dao.UpdateSemesterAsync(semester, cancellationToken);
    public Task<IReadOnlyList<CourseSection>> GetCourseSectionsAsync(int? semesterId = null, int? lecturerId = null, string? keyword = null, bool? openOnly = null, CancellationToken cancellationToken = default) => dao.GetCourseSectionsAsync(semesterId, lecturerId, keyword, openOnly, cancellationToken);
    public Task<CourseSection?> GetCourseSectionByIdAsync(int courseSectionId, CancellationToken cancellationToken = default) => dao.GetCourseSectionByIdAsync(courseSectionId, cancellationToken);
    public Task<bool> SectionCodeExistsAsync(string sectionCode, int? excludeSectionId = null, CancellationToken cancellationToken = default) => dao.SectionCodeExistsAsync(sectionCode, excludeSectionId, cancellationToken);
    public Task AddCourseSectionAsync(CourseSection courseSection, CancellationToken cancellationToken = default) => dao.AddCourseSectionAsync(courseSection, cancellationToken);
    public Task UpdateCourseSectionAsync(CourseSection courseSection, CancellationToken cancellationToken = default) => dao.UpdateCourseSectionAsync(courseSection, cancellationToken);
    public Task<IReadOnlyList<ScheduleSlot>> GetScheduleSlotsAsync(int? semesterId = null, int? courseSectionId = null, int? lecturerId = null, int? studentId = null, CancellationToken cancellationToken = default) => dao.GetScheduleSlotsAsync(semesterId, courseSectionId, lecturerId, studentId, cancellationToken);
    public Task<ScheduleSlot?> GetScheduleSlotByIdAsync(int scheduleSlotId, CancellationToken cancellationToken = default) => dao.GetScheduleSlotByIdAsync(scheduleSlotId, cancellationToken);
    public Task AddScheduleSlotAsync(ScheduleSlot scheduleSlot, CancellationToken cancellationToken = default) => dao.AddScheduleSlotAsync(scheduleSlot, cancellationToken);
    public Task UpdateScheduleSlotAsync(ScheduleSlot scheduleSlot, CancellationToken cancellationToken = default) => dao.UpdateScheduleSlotAsync(scheduleSlot, cancellationToken);
    public Task DeleteScheduleSlotAsync(int scheduleSlotId, CancellationToken cancellationToken = default) => dao.DeleteScheduleSlotAsync(scheduleSlotId, cancellationToken);
    public Task<bool> HasScheduleConflictAsync(int studentId, ScheduleSlot candidateSlot, int? excludeCourseSectionId = null, CancellationToken cancellationToken = default) => dao.HasScheduleConflictAsync(studentId, candidateSlot, excludeCourseSectionId, cancellationToken);
}
