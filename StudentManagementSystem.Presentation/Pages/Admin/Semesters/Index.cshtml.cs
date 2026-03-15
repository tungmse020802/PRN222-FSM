using System.ComponentModel.DataAnnotations;
using StudentManagementSystem.BLL.DTOs;
using StudentManagementSystem.BLL.Interfaces;
using StudentManagementSystem.Shared.Constants;
using StudentManagementSystem.Shared.Entities;
using StudentManagementSystem.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StudentManagementSystem.Presentation.Pages.Admin.Semesters;

[Authorize(Roles = AppRoles.Admin)]
public sealed class IndexModel(IAcademicService academicService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? EditId { get; set; }

    [BindProperty]
    public SemesterInputModel Input { get; set; } = SemesterInputModel.CreateDefault();

    public IReadOnlyList<Semester> Semesters { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadAsync(cancellationToken);
            return Page();
        }

        var result = await academicService.CreateSemesterAsync(MapRequest(), cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create semester.");
            await LoadAsync(cancellationToken);
            return Page();
        }

        TempData["StatusMessage"] = "Semester created successfully.";
        return RedirectToPage(new { SearchTerm });
    }

    public async Task<IActionResult> OnPostUpdateAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            EditId = Input.SemesterId;
            await LoadAsync(cancellationToken);
            return Page();
        }

        var result = await academicService.UpdateSemesterAsync(MapRequest(), cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to update semester.");
            EditId = Input.SemesterId;
            await LoadAsync(cancellationToken);
            return Page();
        }

        TempData["StatusMessage"] = "Semester updated successfully.";
        return RedirectToPage(new { SearchTerm });
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        Semesters = await academicService.GetSemestersAsync(SearchTerm, cancellationToken);

        if (!EditId.HasValue)
        {
            return;
        }

        var semester = Semesters.FirstOrDefault(x => x.SemesterId == EditId.Value);
        if (semester is null)
        {
            TempData["ErrorMessage"] = "Semester not found.";
            EditId = null;
            return;
        }

        Input = new SemesterInputModel
        {
            SemesterId = semester.SemesterId,
            SemesterCode = semester.SemesterCode,
            SemesterName = semester.SemesterName,
            SchoolYear = semester.SchoolYear,
            StartDate = semester.StartDate,
            EndDate = semester.EndDate,
            RegistrationStartDate = semester.RegistrationStartDate,
            RegistrationEndDate = semester.RegistrationEndDate,
            MaxCreditsPerStudent = semester.MaxCreditsPerStudent,
            Status = semester.Status,
            IsActive = semester.IsActive
        };
    }

    private SemesterUpsertRequest MapRequest() => new()
    {
        SemesterId = Input.SemesterId,
        SemesterCode = Input.SemesterCode,
        SemesterName = Input.SemesterName,
        SchoolYear = Input.SchoolYear,
        StartDate = Input.StartDate,
        EndDate = Input.EndDate,
        RegistrationStartDate = Input.RegistrationStartDate,
        RegistrationEndDate = Input.RegistrationEndDate,
        MaxCreditsPerStudent = Input.MaxCreditsPerStudent,
        Status = Input.Status,
        IsActive = Input.IsActive
    };

    public sealed class SemesterInputModel
    {
        public static SemesterInputModel CreateDefault()
        {
            var today = DateTime.Today;
            return new SemesterInputModel
            {
                StartDate = today,
                EndDate = today.AddMonths(4),
                RegistrationStartDate = today,
                RegistrationEndDate = today.AddDays(14),
                MaxCreditsPerStudent = 18,
                Status = SemesterStatus.Planned,
                IsActive = true
            };
        }

        public int? SemesterId { get; set; }

        [Required]
        [StringLength(30)]
        public string SemesterCode { get; set; } = string.Empty;

        [Required]
        [StringLength(120)]
        public string SemesterName { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string SchoolYear { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime RegistrationStartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime RegistrationEndDate { get; set; }

        [Range(1, 30)]
        public int MaxCreditsPerStudent { get; set; }

        public SemesterStatus Status { get; set; }

        public bool IsActive { get; set; }
    }
}
