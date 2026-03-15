using StudentManagementSystem.Shared.Entities;

namespace StudentManagementSystem.DAL.Repositories.Interfaces;

public interface IEnrollmentRepository
{
    Task<Enrollment?> GetEnrollmentAsync(int studentId, int courseSectionId, CancellationToken cancellationToken = default);
    Task UpdateEnrollmentAsync(Enrollment enrollment, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Enrollment>> GetStudentEnrollmentsAsync(int studentId, int? semesterId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetPassedSubjectIdsAsync(int studentId, CancellationToken cancellationToken = default);
    Task<int> GetRegisteredCreditsAsync(int studentId, int semesterId, CancellationToken cancellationToken = default);
    Task AddEnrollmentAsync(Enrollment enrollment, CancellationToken cancellationToken = default);
    Task UpdateCourseSectionCapacityAsync(int courseSectionId, int delta, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Enrollment>> GetGradeBookAsync(int courseSectionId, CancellationToken cancellationToken = default);
    Task<GradeRecord?> GetGradeRecordAsync(int enrollmentId, CancellationToken cancellationToken = default);
    Task SaveGradeRecordAsync(GradeRecord gradeRecord, CancellationToken cancellationToken = default);
    Task<int?> GetStudentUserAccountIdAsync(int studentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetSectionRecipientUserAccountIdsAsync(int courseSectionId, CancellationToken cancellationToken = default);
}
