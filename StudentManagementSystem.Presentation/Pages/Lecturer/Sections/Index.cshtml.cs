using StudentManagementSystem.BLL.Interfaces;
using StudentManagementSystem.Presentation.Extensions;
using StudentManagementSystem.Shared.Constants;
using StudentManagementSystem.Shared.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StudentManagementSystem.Presentation.Pages.Lecturer.Sections;

[Authorize(Roles = AppRoles.Lecturer)]
public sealed class IndexModel(IAcademicService academicService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int? SemesterId { get; set; }

    public IReadOnlyList<Semester> Semesters { get; private set; } = [];
    public IReadOnlyList<CourseSection> Sections { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var lecturerId = User.GetLecturerId();
        if (!lecturerId.HasValue)
        {
            return Forbid();
        }

        Semesters = await academicService.GetSemestersAsync(null, cancellationToken);
        Sections = await academicService.GetCourseSectionsAsync(SemesterId, lecturerId, null, null, cancellationToken);
        return Page();
    }
}
