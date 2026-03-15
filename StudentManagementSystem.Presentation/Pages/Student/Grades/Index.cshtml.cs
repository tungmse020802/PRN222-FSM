using StudentManagementSystem.BLL.Interfaces;
using StudentManagementSystem.Presentation.Extensions;
using StudentManagementSystem.Shared.Constants;
using StudentManagementSystem.Shared.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StudentManagementSystem.Presentation.Pages.Student.Grades;

[Authorize(Roles = AppRoles.Student)]
public sealed class IndexModel(
    IAcademicService academicService,
    IEnrollmentService enrollmentService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int? SemesterId { get; set; }

    public IReadOnlyList<Semester> Semesters { get; private set; } = [];
    public IReadOnlyList<Enrollment> Enrollments { get; private set; } = [];
    public decimal SemesterGpa { get; private set; }
    public decimal CumulativeGpa { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var studentId = User.GetStudentId();
        if (!studentId.HasValue)
        {
            return Forbid();
        }

        Semesters = await academicService.GetSemestersAsync(null, cancellationToken);
        Enrollments = await enrollmentService.GetStudentEnrollmentsAsync(studentId.Value, SemesterId, cancellationToken);
        SemesterGpa = await enrollmentService.CalculateSemesterGpaAsync(studentId.Value, SemesterId, cancellationToken);
        CumulativeGpa = await enrollmentService.CalculateSemesterGpaAsync(studentId.Value, null, cancellationToken);
        return Page();
    }
}
