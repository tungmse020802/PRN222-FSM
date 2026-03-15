using StudentManagementSystem.BLL.Interfaces;
using StudentManagementSystem.Presentation.Extensions;
using StudentManagementSystem.Shared.Constants;
using StudentManagementSystem.Shared.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StudentManagementSystem.Presentation.Pages.Student.Profile;

[Authorize(Roles = AppRoles.Student)]
public sealed class IndexModel(IStudentService studentService) : PageModel
{
    public StudentManagementSystem.Shared.Entities.Student? Profile { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var userAccountId = User.GetUserAccountId();
        if (!userAccountId.HasValue)
        {
            return Forbid();
        }

        Profile = await studentService.GetProfileByUserAccountIdAsync(userAccountId.Value, cancellationToken);
        return Page();
    }
}
