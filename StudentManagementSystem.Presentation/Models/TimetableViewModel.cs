using StudentManagementSystem.Shared.Entities;

namespace StudentManagementSystem.Presentation.Models;

public sealed class TimetableViewModel
{
    private static readonly DayOfWeek[] SupportedDays =
    [
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday,
        DayOfWeek.Saturday
    ];

    private static readonly int[] SupportedSessionSlots = [1, 2, 3, 4, 5, 6];

    public TimetableViewModel(DateTime focusDate, IEnumerable<ScheduleSlot> scheduleSlots, int? dayFilter = null, int? sessionSlotFilter = null)
    {
        FocusDate = focusDate.Date;
        DayFilter = dayFilter;
        SessionSlotFilter = sessionSlotFilter;
        ScheduleSlots = scheduleSlots
            .OrderBy(x => x.DayOfWeek)
            .ThenBy(x => x.SessionSlot)
            .ThenBy(x => x.CourseSection!.SectionCode)
            .ToList();

        var weekStart = GetWeekStart(FocusDate);
        var weekEnd = weekStart.AddDays(6);

        WeekStart = weekStart;
        WeekEnd = weekEnd;
        VisibleSlots = ScheduleSlots
            .Where(slot => slot.StartDate.Date <= weekEnd && slot.EndDate.Date >= weekStart)
            .Where(slot => !dayFilter.HasValue || (int)slot.DayOfWeek == dayFilter.Value)
            .Where(slot => !sessionSlotFilter.HasValue || slot.SessionSlot == sessionSlotFilter.Value)
            .ToList();
    }

    public DateTime FocusDate { get; }

    public DateTime WeekStart { get; }

    public DateTime WeekEnd { get; }

    public int? DayFilter { get; }

    public int? SessionSlotFilter { get; }

    public IReadOnlyList<ScheduleSlot> ScheduleSlots { get; }

    public IReadOnlyList<ScheduleSlot> VisibleSlots { get; }

    public IReadOnlyList<DayOfWeek> Days => SupportedDays;

    public IReadOnlyList<int> SessionSlots => SupportedSessionSlots;

    public bool HasVisibleSlots => VisibleSlots.Count > 0;

    public string WeekLabel => $"{WeekStart:dd/MM/yyyy} - {WeekEnd:dd/MM/yyyy}";

    public IReadOnlyList<ScheduleSlot> GetCellSlots(DayOfWeek dayOfWeek, int sessionSlot) =>
        VisibleSlots
            .Where(x => x.DayOfWeek == dayOfWeek && x.SessionSlot == sessionSlot)
            .OrderBy(x => x.CourseSection!.SectionCode)
            .ToList();

    public static string GetDayLabel(DayOfWeek dayOfWeek) => dayOfWeek switch
    {
        DayOfWeek.Monday => "Monday",
        DayOfWeek.Tuesday => "Tuesday",
        DayOfWeek.Wednesday => "Wednesday",
        DayOfWeek.Thursday => "Thursday",
        DayOfWeek.Friday => "Friday",
        DayOfWeek.Saturday => "Saturday",
        DayOfWeek.Sunday => "Sunday",
        _ => dayOfWeek.ToString()
    };

    public static string GetSlotLabel(int sessionSlot) => $"Slot {sessionSlot}";

    private static DateTime GetWeekStart(DateTime date)
    {
        var diff = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return date.AddDays(-diff);
    }
}
