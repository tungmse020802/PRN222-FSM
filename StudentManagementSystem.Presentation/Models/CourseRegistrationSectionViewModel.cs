using StudentManagementSystem.Shared.Entities;
using StudentManagementSystem.Shared.Enums;

namespace StudentManagementSystem.Presentation.Models;

public sealed class CourseRegistrationSectionViewModel
{
    public required CourseSection Section { get; init; }

    public required string LecturerName { get; init; }

    public required string ScheduleSummary { get; init; }

    public required string RoomSummary { get; init; }

    public required string RegistrationWindowLabel { get; init; }

    public DateTime? ClassStartDate { get; init; }

    public DateTime? ClassEndDate { get; init; }

    public bool IsRegistered { get; init; }

    public bool CanRegister { get; init; }

    public bool CanCancel { get; init; }

    public required string StatusText { get; init; }

    public required string StatusClass { get; init; }

    public required string AvailabilityNote { get; init; }

    public static CourseRegistrationSectionViewModel Create(
        CourseSection section,
        IEnumerable<Enrollment> currentEnrollments,
        DateTime today)
    {
        var scheduleSlots = section.ScheduleSlots
            .OrderBy(x => x.DayOfWeek)
            .ThenBy(x => x.SessionSlot)
            .ThenBy(x => x.StartDate)
            .ToList();

        var classStartDate = scheduleSlots.Count == 0 ? (DateTime?)null : scheduleSlots.Min(x => x.StartDate.Date);
        var classEndDate = scheduleSlots.Count == 0 ? (DateTime?)null : scheduleSlots.Max(x => x.EndDate.Date);
        var registrationStart = section.Semester?.RegistrationStartDate.Date;
        var registrationEnd = section.Semester?.RegistrationEndDate.Date;
        var registrationWindowLabel = registrationStart.HasValue && registrationEnd.HasValue
            ? $"{registrationStart:dd/MM} - {registrationEnd:dd/MM}"
            : "--";

        var activeEnrollment = currentEnrollments.FirstOrDefault(x => x.CourseSectionId == section.CourseSectionId && x.Status == EnrollmentStatus.Registered);
        var duplicateSubject = currentEnrollments.Any(x =>
            x.Status == EnrollmentStatus.Registered &&
            x.CourseSectionId != section.CourseSectionId &&
            x.CourseSection?.SubjectId == section.SubjectId);

        var scheduleSummary = scheduleSlots.Count == 0
            ? "No timetable"
            : string.Join(", ", scheduleSlots.Select(x => $"{TimetableViewModel.GetDayLabel(x.DayOfWeek)} {TimetableViewModel.GetSlotLabel(x.SessionSlot)}"));

        var roomSummary = scheduleSlots.Count == 0
            ? "--"
            : string.Join(", ", scheduleSlots.Select(x => x.Room).Distinct(StringComparer.OrdinalIgnoreCase));

        var isWithinRegistrationWindow = section.Semester is not null &&
            section.Semester.Status == SemesterStatus.OpenForRegistration &&
            registrationStart.HasValue &&
            registrationEnd.HasValue &&
            today >= registrationStart.Value &&
            today <= registrationEnd.Value;

        var hasStarted = classStartDate.HasValue && today >= classStartDate.Value;
        var isFull = section.CurrentCapacity >= section.MaxCapacity;
        var hasActiveEnrollment = activeEnrollment is not null;
        var canRegister = !hasActiveEnrollment &&
            !duplicateSubject &&
            section.IsOpen &&
            scheduleSlots.Count > 0 &&
            !hasStarted &&
            !isFull &&
            isWithinRegistrationWindow;

        var canCancel = hasActiveEnrollment &&
            !hasStarted &&
            registrationEnd is DateTime registrationDeadline &&
            today <= registrationDeadline;

        var (statusText, statusClass, note) = hasActiveEnrollment
            ? ("Registered", "bg-primary-subtle text-primary", canCancel ? "You can cancel before the class starts." : "Cancellation is closed for this class.")
            : duplicateSubject
                ? ("Duplicate Subject", "bg-warning-subtle text-warning-emphasis", "You already registered another class for this subject.")
                : !section.IsOpen
                    ? ("Closed", "bg-secondary-subtle text-secondary", "This class is closed for registration.")
                    : scheduleSlots.Count == 0
                        ? ("No Timetable", "bg-secondary-subtle text-secondary", "Admin has not assigned timetable yet.")
                        : hasStarted
                            ? ("Started", "bg-danger-subtle text-danger", $"Class started on {classStartDate:dd/MM/yyyy}.")
                            : isFull
                                ? ("Full", "bg-warning-subtle text-warning-emphasis", "No seats left in this class.")
                                : !isWithinRegistrationWindow
                                    ? ("Waiting Window", "bg-warning-subtle text-warning-emphasis", $"Registration window: {registrationWindowLabel}.")
                                    : ("Available", "bg-success-subtle text-success", classStartDate.HasValue
                                        ? $"Register before {classStartDate:dd/MM/yyyy}."
                                        : "Ready for registration.");

        return new CourseRegistrationSectionViewModel
        {
            Section = section,
            LecturerName = section.Lecturer?.UserAccount?.FullName ?? "--",
            ScheduleSummary = scheduleSummary,
            RoomSummary = roomSummary,
            RegistrationWindowLabel = registrationWindowLabel,
            ClassStartDate = classStartDate,
            ClassEndDate = classEndDate,
            IsRegistered = hasActiveEnrollment,
            CanRegister = canRegister,
            CanCancel = canCancel,
            StatusText = statusText,
            StatusClass = statusClass,
            AvailabilityNote = note
        };
    }
}
