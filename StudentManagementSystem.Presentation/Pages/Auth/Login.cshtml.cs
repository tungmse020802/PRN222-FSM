using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using StudentManagementSystem.BLL.DTOs;
using StudentManagementSystem.BLL.Interfaces;
using StudentManagementSystem.Shared.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StudentManagementSystem.Presentation.Pages.Auth;

[AllowAnonymous]
public class LoginModel(IAuthService authService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToHomeByRole(User.FindFirstValue(ClaimTypes.Role));
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await authService.AuthenticateAsync(new LoginRequest(Input.Email, Input.Password), cancellationToken);

        if (!result.Succeeded || result.Data is null)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Email or Password is invalid.");
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, result.Data.Email),
            new(ClaimTypes.Name, result.Data.FullName),
            new(ClaimTypes.Role, result.Data.Role)
        };

        if (result.Data.UserAccountId.HasValue)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, result.Data.UserAccountId.Value.ToString()));
        }

        if (result.Data.StudentId.HasValue)
        {
            claims.Add(new Claim(AppClaimTypes.StudentId, result.Data.StudentId.Value.ToString()));
        }

        if (result.Data.LecturerId.HasValue)
        {
            claims.Add(new Claim(AppClaimTypes.LecturerId, result.Data.LecturerId.Value.ToString()));
        }

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });

        return RedirectToHomeByRole(result.Data.Role);
    }

    private IActionResult RedirectToHomeByRole(string? role) => role switch
    {
        AppRoles.Admin => RedirectToPage("/Admin/Students/Index"),
        AppRoles.Lecturer => RedirectToPage("/Lecturer/Sections/Index"),
        AppRoles.Student => RedirectToPage("/Student/Registration/Index"),
        _ => RedirectToPage("/")
    };

    public sealed class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;
    }
}
