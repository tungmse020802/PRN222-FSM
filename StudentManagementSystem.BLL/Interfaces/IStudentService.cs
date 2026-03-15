using StudentManagementSystem.BLL.Common;
using StudentManagementSystem.BLL.DTOs;
using StudentManagementSystem.Shared.Entities;

namespace StudentManagementSystem.BLL.Interfaces;

public interface IStudentService
{
    Task<IReadOnlyList<Student>> GetStudentsAsync(string? keyword = null, CancellationToken cancellationToken = default);
    Task<Student?> GetStudentByIdAsync(int studentId, CancellationToken cancellationToken = default);
    Task<Student?> GetProfileByUserAccountIdAsync(int userAccountId, CancellationToken cancellationToken = default);
    Task<ServiceResult> CreateAsync(StudentUpsertRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult> UpdateAsync(StudentUpsertRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult> SetActiveAsync(int studentId, bool isActive, CancellationToken cancellationToken = default);
}
