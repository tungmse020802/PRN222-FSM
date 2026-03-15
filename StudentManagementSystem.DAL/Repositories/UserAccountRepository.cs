using StudentManagementSystem.DAL.DAO.Interfaces;
using StudentManagementSystem.DAL.Repositories.Interfaces;
using StudentManagementSystem.Shared.Entities;

namespace StudentManagementSystem.DAL.Repositories;

public sealed class UserAccountRepository(IUserAccountDao dao) : IUserAccountRepository
{
    public Task<UserAccount?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default) => dao.GetUserByEmailAsync(email, cancellationToken);
    public Task<UserAccount?> GetUserByIdAsync(int userAccountId, CancellationToken cancellationToken = default) => dao.GetUserByIdAsync(userAccountId, cancellationToken);
    public Task<IReadOnlyList<Student>> GetStudentsAsync(string? keyword = null, CancellationToken cancellationToken = default) => dao.GetStudentsAsync(keyword, cancellationToken);
    public Task<Student?> GetStudentByIdAsync(int studentId, CancellationToken cancellationToken = default) => dao.GetStudentByIdAsync(studentId, cancellationToken);
    public Task<Student?> GetStudentByUserAccountIdAsync(int userAccountId, CancellationToken cancellationToken = default) => dao.GetStudentByUserAccountIdAsync(userAccountId, cancellationToken);
    public Task<bool> StudentCodeExistsAsync(string studentCode, int? excludeStudentId = null, CancellationToken cancellationToken = default) => dao.StudentCodeExistsAsync(studentCode, excludeStudentId, cancellationToken);
    public Task<bool> UserEmailExistsAsync(string email, int? excludeUserAccountId = null, CancellationToken cancellationToken = default) => dao.UserEmailExistsAsync(email, excludeUserAccountId, cancellationToken);
    public Task AddStudentAsync(UserAccount userAccount, Student student, CancellationToken cancellationToken = default) => dao.AddStudentAsync(userAccount, student, cancellationToken);
    public Task UpdateStudentAsync(Student student, CancellationToken cancellationToken = default) => dao.UpdateStudentAsync(student, cancellationToken);
    public Task SetStudentActiveAsync(int studentId, bool isActive, CancellationToken cancellationToken = default) => dao.SetStudentActiveAsync(studentId, isActive, cancellationToken);
    public Task<IReadOnlyList<Lecturer>> GetLecturersAsync(CancellationToken cancellationToken = default) => dao.GetLecturersAsync(cancellationToken);
    public Task<Lecturer?> GetLecturerByUserAccountIdAsync(int userAccountId, CancellationToken cancellationToken = default) => dao.GetLecturerByUserAccountIdAsync(userAccountId, cancellationToken);
}
