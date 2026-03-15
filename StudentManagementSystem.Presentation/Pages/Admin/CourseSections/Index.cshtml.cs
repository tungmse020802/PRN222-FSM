using System.ComponentModel.DataAnnotations;
using StudentManagementSystem.BLL.DTOs;
using StudentManagementSystem.BLL.Interfaces;
using StudentManagementSystem.Shared.Constants;
using StudentManagementSystem.Shared.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StudentManagementSystem.Presentation.Pages.Admin.CourseSections;

[Authorize(Roles = AppRoles.Admin)]
public sealed class IndexModel(IAcademicService academicService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? SemesterFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? EditId { get; set; }

    [BindProperty]
    public CourseSectionInputModel Input { get; set; } = new();

    public IReadOnlyList<CourseSection> CourseSections { get; private set; } = [];
    public IReadOnlyList<Subject> Subjects { get; private set; } = [];
    public IReadOnlyList<Semester> Semesters { get; private set; } = [];
    public IReadOnlyList<StudentManagementSystem.Shared.Entities.Lecturer> Lecturers { get; private set; } = [];

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

        var result = await academicService.CreateCourseSectionAsync(MapRequest(), cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create course section.");
            await LoadAsync(cancellationToken);
            return Page();
        }

        TempData["StatusMessage"] = "Course section created successfully.";
        return RedirectToPage(new { SearchTerm, SemesterFilter });
    }

    public async Task<IActionResult> OnPostUpdateAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            EditId = Input.CourseSectionId;
            await LoadAsync(cancellationToken);
            return Page();
        }

        var result = await academicService.UpdateCourseSectionAsync(MapRequest(), cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to update course section.");
            EditId = Input.CourseSectionId;
            await LoadAsync(cancellationToken);
            return Page();
        }

        TempData["StatusMessage"] = "Course section updated successfully.";
        return RedirectToPage(new { SearchTerm, SemesterFilter });
    }

    public async Task<IActionResult> OnPostToggleAsync(int courseSectionId, CancellationToken cancellationToken)
    {
        var existing = await academicService.GetCourseSectionByIdAsync(courseSectionId, cancellationToken);
        if (existing is null)
        {
            TempData["ErrorMessage"] = "Course section not found.";
            return RedirectToPage(new { SearchTerm, SemesterFilter });
        }

        var result = await academicService.UpdateCourseSectionAsync(new CourseSectionUpsertRequest
        {
            CourseSectionId = existing.CourseSectionId,
            SectionCode = existing.SectionCode,
            SectionName = existing.SectionName,
            SubjectId = existing.SubjectId,
            SemesterId = existing.SemesterId,
            LecturerId = existing.LecturerId,
            MaxCapacity = existing.MaxCapacity,
            IsOpen = !existing.IsOpen
        }, cancellationToken);

        TempData[result.Succeeded ? "StatusMessage" : "ErrorMessage"] =
            result.Succeeded
                ? (existing.IsOpen ? "Course section closed." : "Course section opened.")
                : result.ErrorMessage ?? "Unable to update course section.";

        return RedirectToPage(new { SearchTerm, SemesterFilter, EditId });
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        Subjects = await academicService.GetSubjectsAsync(null, cancellationToken);
        Semesters = await academicService.GetSemestersAsync(null, cancellationToken);
        Lecturers = await academicService.GetLecturersAsync(cancellationToken);
        CourseSections = await academicService.GetCourseSectionsAsync(SemesterFilter, null, SearchTerm, null, cancellationToken);

        if (!EditId.HasValue)
        {
            if (Input.SubjectId == 0 && Subjects.Count > 0)
            {
                Input.SubjectId = Subjects[0].SubjectId;
            }

            if (Input.SemesterId == 0 && Semesters.Count > 0)
            {
                Input.SemesterId = Semesters[0].SemesterId;
            }

            if (Input.LecturerId == 0 && Lecturers.Count > 0)
            {
                Input.LecturerId = Lecturers[0].LecturerId;
            }

            return;
        }

        var section = await academicService.GetCourseSectionByIdAsync(EditId.Value, cancellationToken);
        if (section is null)
        {
            TempData["ErrorMessage"] = "Course section not found.";
            EditId = null;
            return;
        }

        Input = new CourseSectionInputModel
        {
            CourseSectionId = section.CourseSectionId,
            SectionCode = section.SectionCode,
            SectionName = section.SectionName,
            SubjectId = section.SubjectId,
            SemesterId = section.SemesterId,
            LecturerId = section.LecturerId,
            MaxCapacity = section.MaxCapacity,
            IsOpen = section.IsOpen
        };
    }

    private CourseSectionUpsertRequest MapRequest() => new()
    {
        CourseSectionId = Input.CourseSectionId,
        SectionCode = Input.SectionCode,
        SectionName = Input.SectionName,
        SubjectId = Input.SubjectId,
        SemesterId = Input.SemesterId,
        LecturerId = Input.LecturerId,
        MaxCapacity = Input.MaxCapacity,
        IsOpen = Input.IsOpen
    };

    public sealed class CourseSectionInputModel
    {
        public int? CourseSectionId { get; set; }

        [Required]
        [StringLength(30)]
        public string SectionCode { get; set; } = string.Empty;

        [Required]
        [StringLength(120)]
        public string SectionName { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int SubjectId { get; set; }

        [Range(1, int.MaxValue)]
        public int SemesterId { get; set; }

        [Range(1, int.MaxValue)]
        public int LecturerId { get; set; }

        [Range(1, 500)]
        public int MaxCapacity { get; set; } = 30;

        public bool IsOpen { get; set; } = true;
    }
}
