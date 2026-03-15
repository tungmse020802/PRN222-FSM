using StudentManagementSystem.BLL.Common;
using StudentManagementSystem.BLL.DTOs;
using StudentManagementSystem.Shared.Entities;

namespace StudentManagementSystem.BLL.Interfaces;

public interface IEnrollmentService
{
    Task<IReadOnlyList<Enrollment>> GetStudentEnrollmentsAsync(int studentId, int? semesterId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Enrollment>> GetGradeBookAsync(int courseSectionId, CancellationToken cancellationToken = default);
    Task<ServiceResult> RegisterAsync(int studentId, int userAccountId, int courseSectionId, CancellationToken cancellationToken = default);
    Task<ServiceResult> CancelAsync(int studentId, int userAccountId, int courseSectionId, CancellationToken cancellationToken = default);
    Task<ServiceResult<int>> SaveGradeAsync(int lecturerUserAccountId, GradeUpsertRequest request, CancellationToken cancellationToken = default);
    Task<decimal> CalculateSemesterGpaAsync(int studentId, int? semesterId = null, CancellationToken cancellationToken = default);
}
