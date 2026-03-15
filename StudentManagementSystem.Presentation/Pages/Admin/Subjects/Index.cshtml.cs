using System.ComponentModel.DataAnnotations;
using StudentManagementSystem.BLL.DTOs;
using StudentManagementSystem.BLL.Interfaces;
using StudentManagementSystem.Shared.Constants;
using StudentManagementSystem.Shared.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StudentManagementSystem.Presentation.Pages.Admin.Subjects;

[Authorize(Roles = AppRoles.Admin)]
public sealed class IndexModel(IAcademicService academicService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? EditId { get; set; }

    [BindProperty]
    public SubjectInputModel Input { get; set; } = new();

    public IReadOnlyList<Subject> Subjects { get; private set; } = [];

    public IReadOnlyList<Subject> PrerequisiteOptions { get; private set; } = [];

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

        var result = await academicService.CreateSubjectAsync(MapRequest(), cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create subject.");
            await LoadAsync(cancellationToken);
            return Page();
        }

        TempData["StatusMessage"] = "Subject created successfully.";
        return RedirectToPage(new { SearchTerm });
    }

    public async Task<IActionResult> OnPostUpdateAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            EditId = Input.SubjectId;
            await LoadAsync(cancellationToken);
            return Page();
        }

        var result = await academicService.UpdateSubjectAsync(MapRequest(), cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to update subject.");
            EditId = Input.SubjectId;
            await LoadAsync(cancellationToken);
            return Page();
        }

        TempData["StatusMessage"] = "Subject updated successfully.";
        return RedirectToPage(new { SearchTerm });
    }

    public async Task<IActionResult> OnPostToggleAsync(int subjectId, bool isActive, CancellationToken cancellationToken)
    {
        var result = await academicService.SetSubjectActiveAsync(subjectId, isActive, cancellationToken);
        TempData[result.Succeeded ? "StatusMessage" : "ErrorMessage"] =
            result.Succeeded
                ? (isActive ? "Subject activated." : "Subject deactivated.")
                : result.ErrorMessage ?? "Unable to update subject status.";
        return RedirectToPage(new { SearchTerm, EditId });
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        Subjects = await academicService.GetSubjectsAsync(SearchTerm, cancellationToken);
        PrerequisiteOptions = await academicService.GetSubjectsAsync(null, cancellationToken);

        if (!EditId.HasValue)
        {
            return;
        }

        var subject = Subjects.FirstOrDefault(x => x.SubjectId == EditId.Value)
            ?? await academicService.GetSubjectsAsync(null, cancellationToken)
                .ContinueWith(task => task.Result.FirstOrDefault(x => x.SubjectId == EditId.Value), cancellationToken);

        if (subject is null)
        {
            TempData["ErrorMessage"] = "Subject not found.";
            EditId = null;
            return;
        }

        Input = new SubjectInputModel
        {
            SubjectId = subject.SubjectId,
            SubjectCode = subject.SubjectCode,
            SubjectName = subject.SubjectName,
            Credits = subject.Credits,
            TheoryHours = subject.TheoryHours,
            PracticeHours = subject.PracticeHours,
            IsActive = subject.IsActive,
            PrerequisiteIds = subject.PrerequisiteLinks.Select(x => x.PrerequisiteSubjectId).ToList()
        };
    }

    private SubjectUpsertRequest MapRequest() => new()
    {
        SubjectId = Input.SubjectId,
        SubjectCode = Input.SubjectCode,
        SubjectName = Input.SubjectName,
        Credits = Input.Credits,
        TheoryHours = Input.TheoryHours,
        PracticeHours = Input.PracticeHours,
        IsActive = Input.IsActive,
        PrerequisiteIds = Input.PrerequisiteIds
    };

    public sealed class SubjectInputModel
    {
        public int? SubjectId { get; set; }

        [Required]
        [StringLength(30)]
        public string SubjectCode { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string SubjectName { get; set; } = string.Empty;

        [Range(1, 30)]
        public int Credits { get; set; } = 3;

        [Range(0, 1000)]
        public int TheoryHours { get; set; } = 30;

        [Range(0, 1000)]
        public int PracticeHours { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public List<int> PrerequisiteIds { get; set; } = [];
    }
}
