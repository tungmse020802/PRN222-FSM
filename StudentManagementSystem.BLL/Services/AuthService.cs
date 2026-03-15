using StudentManagementSystem.BLL.Common;
using StudentManagementSystem.BLL.DTOs;
using StudentManagementSystem.BLL.Interfaces;
using StudentManagementSystem.DAL.Repositories.Interfaces;
using StudentManagementSystem.Shared.Configurations;
using StudentManagementSystem.Shared.Constants;

namespace StudentManagementSystem.BLL.Services;

public sealed class AuthService(IUserAccountRepository userAccountRepository, AdminAccountOptions adminOptions) : IAuthService
{
    public async Task<ServiceResult<AuthenticatedUser>> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();
        var password = request.Password.Trim();
        var adminEmail = adminOptions.Email.Trim();
        var adminPassword = adminOptions.Password.Trim();

        if (string.Equals(email, adminEmail, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(password, adminPassword, StringComparison.Ordinal))
        {
            return ServiceResult<AuthenticatedUser>.Success(new AuthenticatedUser(
                null,
                null,
                null,
                adminEmail,
                adminOptions.FullName,
                AppRoles.Admin));
        }

        var account = await userAccountRepository.GetUserByEmailAsync(email, cancellationToken);
        if (account is null || !account.IsActive || account.Password != password)
        {
            return ServiceResult<AuthenticatedUser>.Failure("Email or Password is invalid.");
        }

        var student = account.RoleName == AppRoles.Student
            ? await userAccountRepository.GetStudentByUserAccountIdAsync(account.UserAccountId, cancellationToken)
            : null;
        var lecturer = account.RoleName == AppRoles.Lecturer
            ? await userAccountRepository.GetLecturerByUserAccountIdAsync(account.UserAccountId, cancellationToken)
            : null;

        return ServiceResult<AuthenticatedUser>.Success(new AuthenticatedUser(
            account.UserAccountId,
            student?.StudentId,
            lecturer?.LecturerId,
            account.Email,
            account.FullName,
            account.RoleName));
    }
}
