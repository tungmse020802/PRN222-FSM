using StudentManagementSystem.DAL.DAO.Interfaces;
using StudentManagementSystem.DAL.Repositories.Interfaces;
using StudentManagementSystem.Shared.Entities;

namespace StudentManagementSystem.DAL.Repositories;

public sealed class EnrollmentRepository(IEnrollmentDao dao) : IEnrollmentRepository
{
    public Task<Enrollment?> GetEnrollmentAsync(int studentId, int courseSectionId, CancellationToken cancellationToken = default) => dao.GetEnrollmentAsync(studentId, courseSectionId, cancellationToken);
    public Task UpdateEnrollmentAsync(Enrollment enrollment, CancellationToken cancellationToken = default) => dao.UpdateEnrollmentAsync(enrollment, cancellationToken);
    public Task<IReadOnlyList<Enrollment>> GetStudentEnrollmentsAsync(int studentId, int? semesterId = null, CancellationToken cancellationToken = default) => dao.GetStudentEnrollmentsAsync(studentId, semesterId, cancellationToken);
    public Task<IReadOnlyList<int>> GetPassedSubjectIdsAsync(int studentId, CancellationToken cancellationToken = default) => dao.GetPassedSubjectIdsAsync(studentId, cancellationToken);
    public Task<int> GetRegisteredCreditsAsync(int studentId, int semesterId, CancellationToken cancellationToken = default) => dao.GetRegisteredCreditsAsync(studentId, semesterId, cancellationToken);
    public Task AddEnrollmentAsync(Enrollment enrollment, CancellationToken cancellationToken = default) => dao.AddEnrollmentAsync(enrollment, cancellationToken);
    public Task UpdateCourseSectionCapacityAsync(int courseSectionId, int delta, CancellationToken cancellationToken = default) => dao.UpdateCourseSectionCapacityAsync(courseSectionId, delta, cancellationToken);
    public Task<IReadOnlyList<Enrollment>> GetGradeBookAsync(int courseSectionId, CancellationToken cancellationToken = default) => dao.GetGradeBookAsync(courseSectionId, cancellationToken);
    public Task<GradeRecord?> GetGradeRecordAsync(int enrollmentId, CancellationToken cancellationToken = default) => dao.GetGradeRecordAsync(enrollmentId, cancellationToken);
    public Task SaveGradeRecordAsync(GradeRecord gradeRecord, CancellationToken cancellationToken = default) => dao.SaveGradeRecordAsync(gradeRecord, cancellationToken);
    public Task<int?> GetStudentUserAccountIdAsync(int studentId, CancellationToken cancellationToken = default) => dao.GetStudentUserAccountIdAsync(studentId, cancellationToken);
    public Task<IReadOnlyList<int>> GetSectionRecipientUserAccountIdsAsync(int courseSectionId, CancellationToken cancellationToken = default) => dao.GetSectionRecipientUserAccountIdsAsync(courseSectionId, cancellationToken);
}
