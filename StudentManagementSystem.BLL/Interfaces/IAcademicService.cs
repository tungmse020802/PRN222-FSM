using StudentManagementSystem.BLL.Common;
using StudentManagementSystem.BLL.DTOs;
using StudentManagementSystem.Shared.Entities;

namespace StudentManagementSystem.BLL.Interfaces;

public interface IAcademicService
{
    Task<IReadOnlyList<Subject>> GetSubjectsAsync(string? keyword = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Semester>> GetSemestersAsync(string? keyword = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CourseSection>> GetCourseSectionsAsync(int? semesterId = null, int? lecturerId = null, string? keyword = null, bool? openOnly = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ScheduleSlot>> GetScheduleSlotsAsync(int? semesterId = null, int? courseSectionId = null, int? lecturerId = null, int? studentId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Lecturer>> GetLecturersAsync(CancellationToken cancellationToken = default);
    Task<CourseSection?> GetCourseSectionByIdAsync(int courseSectionId, CancellationToken cancellationToken = default);
    Task<ServiceResult> CreateSubjectAsync(SubjectUpsertRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult> UpdateSubjectAsync(SubjectUpsertRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult> SetSubjectActiveAsync(int subjectId, bool isActive, CancellationToken cancellationToken = default);
    Task<ServiceResult> CreateSemesterAsync(SemesterUpsertRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult> UpdateSemesterAsync(SemesterUpsertRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult> CreateCourseSectionAsync(CourseSectionUpsertRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult> UpdateCourseSectionAsync(CourseSectionUpsertRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult> CreateScheduleSlotAsync(ScheduleSlotUpsertRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult> UpdateScheduleSlotAsync(ScheduleSlotUpsertRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteScheduleSlotAsync(int scheduleSlotId, CancellationToken cancellationToken = default);
}
