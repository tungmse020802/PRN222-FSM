using System.Security.Claims;
using StudentManagementSystem.Shared.Constants;

namespace StudentManagementSystem.Presentation.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int? GetUserAccountId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userAccountId) ? userAccountId : null;
    }

    public static int? GetStudentId(this ClaimsPrincipal user) =>
        TryReadIntClaim(user, AppClaimTypes.StudentId);

    public static int? GetLecturerId(this ClaimsPrincipal user) =>
        TryReadIntClaim(user, AppClaimTypes.LecturerId);

    public static string? GetDisplayName(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.Name);

    public static string? GetRoleName(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.Role);

    private static int? TryReadIntClaim(ClaimsPrincipal user, string claimType)
    {
        var value = user.FindFirstValue(claimType);
        return int.TryParse(value, out var parsedValue) ? parsedValue : null;
    }
}
