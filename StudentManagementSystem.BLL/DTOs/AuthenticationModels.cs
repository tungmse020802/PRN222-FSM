namespace StudentManagementSystem.BLL.DTOs;

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthenticatedUser(
    int? UserAccountId,
    int? StudentId,
    int? LecturerId,
    string Email,
    string FullName,
    string Role);
