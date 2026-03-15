using StudentManagementSystem.Shared.Entities;

namespace StudentManagementSystem.Presentation.Models;

public sealed class ScheduleAvailabilityOverviewViewModel
{
    private static readonly int[] DefaultSlots = [1, 2, 3, 4, 5, 6];

    public required DateTime FocusDate { get; init; }

    public required DayOfWeek FocusDay { get; init; }

    public required IReadOnlyList<ScheduleAvailabilitySlotViewModel> SlotRows { get; init; }

    public required IReadOnlyList<string> KnownRooms { get; init; }

    public required int LecturerCount { get; init; }

    public string FocusDayLabel => TimetableViewModel.GetDayLabel(FocusDay);

    public static ScheduleAvailabilityOverviewViewModel Create(
        DateTime focusDate,
        IEnumerable<ScheduleSlot> semesterSlots,
        IEnumerable<Lecturer> lecturers)
    {
        var date = focusDate.Date;
        var day = date.DayOfWeek;
        var allSlots = semesterSlots.ToList();
        var activeDaySlots = allSlots
            .Where(x => x.DayOfWeek == day && x.StartDate.Date <= date && date <= x.EndDate.Date)
            .ToList();

        var roomNames = allSlots
            .Select(x => x.Room)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        var lecturerList = lecturers
            .OrderBy(x => x.UserAccount?.FullName)
            .ToList();

        var slotRows = DefaultSlots
            .Select(slot => BuildSlotRow(slot, activeDaySlots, roomNames, lecturerList))
            .ToList();

        return new ScheduleAvailabilityOverviewViewModel
        {
            FocusDate = date,
            FocusDay = day,
            SlotRows = slotRows,
            KnownRooms = roomNames,
            LecturerCount = lecturerList.Count
        };
    }

    private static ScheduleAvailabilitySlotViewModel BuildSlotRow(
        int sessionSlot,
        IReadOnlyList<ScheduleSlot> activeDaySlots,
        IReadOnlyList<string> roomNames,
        IReadOnlyList<Lecturer> lecturers)
    {
        var busySlots = activeDaySlots
            .Where(x => x.SessionSlot == sessionSlot)
            .ToList();

        var roomStatuses = roomNames
            .Select(room =>
            {
                var roomSlot = busySlots.FirstOrDefault(x => x.Room.Equals(room, StringComparison.OrdinalIgnoreCase));
                return new AvailabilityResourceStatusViewModel
                {
                    Name = room,
                    SecondaryText = "Room",
                    IsBusy = roomSlot is not null,
                    DetailText = roomSlot is null
                        ? "Available"
                        : $"{roomSlot.CourseSection?.SectionCode} · {roomSlot.CourseSection?.SectionName}"
                };
            })
            .OrderBy(x => x.IsBusy)
            .ThenBy(x => x.Name)
            .ToList();

        var lecturerStatuses = lecturers
            .Select(lecturer =>
            {
                var lecturerSlot = busySlots.FirstOrDefault(x => x.CourseSection?.LecturerId == lecturer.LecturerId);
                return new AvailabilityResourceStatusViewModel
                {
                    Name = lecturer.UserAccount?.FullName ?? lecturer.LecturerCode,
                    SecondaryText = lecturer.Department,
                    IsBusy = lecturerSlot is not null,
                    DetailText = lecturerSlot is null
                        ? "Available"
                        : $"{lecturerSlot.CourseSection?.SectionCode} · {lecturerSlot.Room}"
                };
            })
            .OrderBy(x => x.IsBusy)
            .ThenBy(x => x.Name)
            .ToList();

        return new ScheduleAvailabilitySlotViewModel
        {
            SessionSlot = sessionSlot,
            BusySections = busySlots
                .OrderBy(x => x.Room)
                .ThenBy(x => x.CourseSection!.SectionCode)
                .ToList(),
            RoomStatuses = roomStatuses,
            LecturerStatuses = lecturerStatuses
        };
    }
}

public sealed class ScheduleAvailabilitySlotViewModel
{
    public required int SessionSlot { get; init; }

    public required IReadOnlyList<ScheduleSlot> BusySections { get; init; }

    public required IReadOnlyList<AvailabilityResourceStatusViewModel> RoomStatuses { get; init; }

    public required IReadOnlyList<AvailabilityResourceStatusViewModel> LecturerStatuses { get; init; }

    public int FreeRoomCount => RoomStatuses.Count(x => !x.IsBusy);

    public int BusyRoomCount => RoomStatuses.Count(x => x.IsBusy);

    public int FreeLecturerCount => LecturerStatuses.Count(x => !x.IsBusy);

    public int BusyLecturerCount => LecturerStatuses.Count(x => x.IsBusy);
}

public sealed class AvailabilityResourceStatusViewModel
{
    public required string Name { get; init; }

    public required string SecondaryText { get; init; }

    public required bool IsBusy { get; init; }

    public required string DetailText { get; init; }
}
