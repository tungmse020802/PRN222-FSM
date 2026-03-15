using StudentManagementSystem.BLL.Common;
using StudentManagementSystem.BLL.DTOs;
using StudentManagementSystem.BLL.Interfaces;
using StudentManagementSystem.DAL.Repositories.Interfaces;
using StudentManagementSystem.Shared.Entities;
using StudentManagementSystem.Shared.Enums;

namespace StudentManagementSystem.BLL.Services;

public sealed class StudentService(IUserAccountRepository userAccountRepository) : IStudentService
{
    public Task<IReadOnlyList<Student>> GetStudentsAsync(string? keyword = null, CancellationToken cancellationToken = default) =>
        userAccountRepository.GetStudentsAsync(keyword, cancellationToken);

    public Task<Student?> GetStudentByIdAsync(int studentId, CancellationToken cancellationToken = default) =>
        userAccountRepository.GetStudentByIdAsync(studentId, cancellationToken);

    public Task<Student?> GetProfileByUserAccountIdAsync(int userAccountId, CancellationToken cancellationToken = default) =>
        userAccountRepository.GetStudentByUserAccountIdAsync(userAccountId, cancellationToken);

    public async Task<ServiceResult> CreateAsync(StudentUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var validation = ValidateRequest(request, true);
        if (!validation.Succeeded)
        {
            return validation;
        }

        if (await userAccountRepository.StudentCodeExistsAsync(request.StudentCode, null, cancellationToken))
        {
            return ServiceResult.Failure("Student code already exists.");
        }

        if (await userAccountRepository.UserEmailExistsAsync(request.Email, null, cancellationToken))
        {
            return ServiceResult.Failure("Email already exists.");
        }

        var userAccount = new UserAccount
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            Password = request.Password!.Trim(),
            Role = UserRole.Student,
            IsActive = request.IsActive
        };

        var student = new Student
        {
            StudentCode = request.StudentCode.Trim(),
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            PhoneNumber = request.PhoneNumber?.Trim(),
            Address = request.Address?.Trim(),
            Major = request.Major.Trim(),
            Cohort = request.Cohort.Trim(),
            AcademicStatus = request.AcademicStatus,
            IsActive = request.IsActive
        };

        await userAccountRepository.AddStudentAsync(userAccount, student, cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateAsync(StudentUpsertRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.StudentId.HasValue)
        {
            return ServiceResult.Failure("StudentId is required.");
        }

        var validation = ValidateRequest(request, false);
        if (!validation.Succeeded)
        {
            return validation;
        }

        var existing = await userAccountRepository.GetStudentByIdAsync(request.StudentId.Value, cancellationToken);
        if (existing is null || existing.UserAccount is null)
        {
            return ServiceResult.Failure("Student not found.");
        }

        if (await userAccountRepository.StudentCodeExistsAsync(request.StudentCode, request.StudentId, cancellationToken))
        {
            return ServiceResult.Failure("Student code already exists.");
        }

        if (await userAccountRepository.UserEmailExistsAsync(request.Email, existing.UserAccountId, cancellationToken))
        {
            return ServiceResult.Failure("Email already exists.");
        }

        existing.StudentCode = request.StudentCode.Trim();
        existing.DateOfBirth = request.DateOfBirth;
        existing.Gender = request.Gender;
        existing.PhoneNumber = request.PhoneNumber?.Trim();
        existing.Address = request.Address?.Trim();
        existing.Major = request.Major.Trim();
        existing.Cohort = request.Cohort.Trim();
        existing.AcademicStatus = request.AcademicStatus;
        existing.IsActive = request.IsActive;
        existing.UserAccount.FullName = request.FullName.Trim();
        existing.UserAccount.Email = request.Email.Trim();
        existing.UserAccount.Password = string.IsNullOrWhiteSpace(request.Password)
            ? existing.UserAccount.Password
            : request.Password.Trim();
        existing.UserAccount.ModifiedDate = DateTime.UtcNow;

        await userAccountRepository.UpdateStudentAsync(existing, cancellationToken);
        return ServiceResult.Success();
    }

    public Task<ServiceResult> SetActiveAsync(int studentId, bool isActive, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync();

        async Task<ServiceResult> ExecuteAsync()
        {
            await userAccountRepository.SetStudentActiveAsync(studentId, isActive, cancellationToken);
            return ServiceResult.Success();
        }
    }

    private static ServiceResult ValidateRequest(StudentUpsertRequest request, bool isCreate)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.StudentCode) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Major) ||
            string.IsNullOrWhiteSpace(request.Cohort))
        {
            return ServiceResult.Failure("Required student information is missing.");
        }

        if (isCreate && string.IsNullOrWhiteSpace(request.Password))
        {
            return ServiceResult.Failure("Password is required.");
        }

        return ServiceResult.Success();
    }
}
