using System.ComponentModel.DataAnnotations;
using StudentManagementSystem.BLL.DTOs;
using StudentManagementSystem.BLL.Interfaces;
using StudentManagementSystem.Shared.Constants;
using StudentManagementSystem.Shared.Entities;
using StudentManagementSystem.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StudentManagementSystem.Presentation.Pages.Admin.Students;

[Authorize(Roles = AppRoles.Admin)]
public sealed class IndexModel(IStudentService studentService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? EditId { get; set; }

    [BindProperty]
    public StudentInputModel Input { get; set; } = StudentInputModel.CreateDefault();

    public IReadOnlyList<StudentManagementSystem.Shared.Entities.Student> Students { get; private set; } = [];

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

        var result = await studentService.CreateAsync(MapRequest(), cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create student.");
            await LoadAsync(cancellationToken);
            return Page();
        }

        TempData["StatusMessage"] = "Student created successfully.";
        return RedirectToPage(new { SearchTerm });
    }

    public async Task<IActionResult> OnPostUpdateAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            EditId = Input.StudentId;
            await LoadAsync(cancellationToken);
            return Page();
        }

        var result = await studentService.UpdateAsync(MapRequest(), cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to update student.");
            EditId = Input.StudentId;
            await LoadAsync(cancellationToken);
            return Page();
        }

        TempData["StatusMessage"] = "Student updated successfully.";
        return RedirectToPage(new { SearchTerm });
    }

    public async Task<IActionResult> OnPostToggleAsync(int studentId, bool isActive, CancellationToken cancellationToken)
    {
        await studentService.SetActiveAsync(studentId, isActive, cancellationToken);
        TempData["StatusMessage"] = isActive ? "Student activated." : "Student deactivated.";
        return RedirectToPage(new { SearchTerm, EditId });
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        Students = await studentService.GetStudentsAsync(SearchTerm, cancellationToken);

        if (!EditId.HasValue)
        {
            return;
        }

        var student = await studentService.GetStudentByIdAsync(EditId.Value, cancellationToken);
        if (student?.UserAccount is null)
        {
            TempData["ErrorMessage"] = "Student not found.";
            EditId = null;
            return;
        }

        Input = new StudentInputModel
        {
            StudentId = student.StudentId,
            FullName = student.UserAccount.FullName,
            StudentCode = student.StudentCode,
            DateOfBirth = student.DateOfBirth,
            Gender = student.Gender,
            Email = student.UserAccount.Email,
            PhoneNumber = student.PhoneNumber,
            Address = student.Address,
            Major = student.Major,
            Cohort = student.Cohort,
            AcademicStatus = student.AcademicStatus,
            IsActive = student.IsActive
        };
    }

    private StudentUpsertRequest MapRequest() => new()
    {
        StudentId = Input.StudentId,
        FullName = Input.FullName,
        StudentCode = Input.StudentCode,
        DateOfBirth = Input.DateOfBirth,
        Gender = Input.Gender,
        Email = Input.Email,
        PhoneNumber = Input.PhoneNumber,
        Address = Input.Address,
        Major = Input.Major,
        Cohort = Input.Cohort,
        AcademicStatus = Input.AcademicStatus,
        IsActive = Input.IsActive,
        Password = string.IsNullOrWhiteSpace(Input.Password) ? null : Input.Password
    };

    public sealed class StudentInputModel
    {
        public static StudentInputModel CreateDefault() => new()
        {
            DateOfBirth = DateTime.Today.AddYears(-18),
            AcademicStatus = AcademicStatus.Active,
            Gender = Gender.Male,
            IsActive = true
        };

        public int? StudentId { get; set; }

        [Required]
        [StringLength(120)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string StudentCode { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        public Gender Gender { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [Required]
        [StringLength(120)]
        public string Major { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string Cohort { get; set; } = string.Empty;

        public AcademicStatus AcademicStatus { get; set; }

        public bool IsActive { get; set; }

        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
