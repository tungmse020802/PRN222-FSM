using StudentManagementSystem.BLL.Common;
using StudentManagementSystem.BLL.DTOs;

namespace StudentManagementSystem.BLL.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<AuthenticatedUser>> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
