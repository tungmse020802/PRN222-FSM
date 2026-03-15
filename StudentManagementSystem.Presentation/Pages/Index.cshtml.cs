using System.Security.Claims;
using StudentManagementSystem.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StudentManagementSystem.Presentation.Pages;

[AllowAnonymous]
public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Page();
        }

        return User.FindFirstValue(ClaimTypes.Role) switch
        {
            AppRoles.Admin => RedirectToPage("/Admin/Students/Index"),
            AppRoles.Lecturer => RedirectToPage("/Lecturer/Sections/Index"),
            AppRoles.Student => RedirectToPage("/Student/Registration/Index"),
            _ => RedirectToPage("/Auth/Login")
        };
    }
}
