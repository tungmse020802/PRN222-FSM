using StudentManagementSystem.BLL.Interfaces;
using StudentManagementSystem.Presentation.Models;
using StudentManagementSystem.Presentation.Extensions;
using StudentManagementSystem.Shared.Constants;
using StudentManagementSystem.Shared.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StudentManagementSystem.Presentation.Pages.Lecturer.Schedule;

[Authorize(Roles = AppRoles.Lecturer)]
public sealed class IndexModel(IAcademicService academicService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int? SemesterId { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? WeekDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? DayFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? SessionSlotFilter { get; set; }

    public IReadOnlyList<Semester> Semesters { get; private set; } = [];
    public IReadOnlyList<ScheduleSlot> ScheduleSlots { get; private set; } = [];
    public Semester? SelectedSemester { get; private set; }
    public TimetableViewModel? Timetable { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var lecturerId = User.GetLecturerId();
        if (!lecturerId.HasValue)
        {
            return Forbid();
        }

        Semesters = await academicService.GetSemestersAsync(null, cancellationToken);
        SelectedSemester = SemesterId.HasValue ? Semesters.FirstOrDefault(x => x.SemesterId == SemesterId.Value) : null;
        ScheduleSlots = await academicService.GetScheduleSlotsAsync(SemesterId, null, lecturerId, null, cancellationToken);
        WeekDate ??= ResolveDefaultWeekDate();
        Timetable = new TimetableViewModel(WeekDate.Value, ScheduleSlots, DayFilter, SessionSlotFilter);
        return Page();
    }

    private DateTime ResolveDefaultWeekDate()
    {
        if (SelectedSemester is not null)
        {
            var today = DateTime.Today;
            if (today >= SelectedSemester.StartDate.Date && today <= SelectedSemester.EndDate.Date)
            {
                return today;
            }

            return SelectedSemester.StartDate.Date;
        }

        return ScheduleSlots.Select(x => x.StartDate.Date).DefaultIfEmpty(DateTime.Today).Min();
    }
}
