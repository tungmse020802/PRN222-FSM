using StudentManagementSystem.Shared.Entities;

namespace StudentManagementSystem.DAL.DAO.Interfaces;

public interface IUserAccountDao
{
    Task<UserAccount?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<UserAccount?> GetUserByIdAsync(int userAccountId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Student>> GetStudentsAsync(string? keyword = null, CancellationToken cancellationToken = default);
    Task<Student?> GetStudentByIdAsync(int studentId, CancellationToken cancellationToken = default);
    Task<Student?> GetStudentByUserAccountIdAsync(int userAccountId, CancellationToken cancellationToken = default);
    Task<bool> StudentCodeExistsAsync(string studentCode, int? excludeStudentId = null, CancellationToken cancellationToken = default);
    Task<bool> UserEmailExistsAsync(string email, int? excludeUserAccountId = null, CancellationToken cancellationToken = default);
    Task AddStudentAsync(UserAccount userAccount, Student student, CancellationToken cancellationToken = default);
    Task UpdateStudentAsync(Student student, CancellationToken cancellationToken = default);
    Task SetStudentActiveAsync(int studentId, bool isActive, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Lecturer>> GetLecturersAsync(CancellationToken cancellationToken = default);
    Task<Lecturer?> GetLecturerByUserAccountIdAsync(int userAccountId, CancellationToken cancellationToken = default);
}
