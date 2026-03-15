using System.ComponentModel.DataAnnotations;
using StudentManagementSystem.BLL.DTOs;
using StudentManagementSystem.BLL.Interfaces;
using StudentManagementSystem.Presentation.Models;
using StudentManagementSystem.Shared.Constants;
using StudentManagementSystem.Shared.Entities;
using StudentManagementSystem.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StudentManagementSystem.Presentation.Pages.Admin.Schedules;

[Authorize(Roles = AppRoles.Admin)]
public sealed class IndexModel(IAcademicService academicService) : PageModel
{
    public const string LookupTargetCourseSection = "courseSection";
    public const string LookupTargetDay = "day";
    public const string LookupTargetSessionSlot = "sessionSlot";
    public const string LookupTargetRoom = "room";

    private static readonly DayOfWeek[] SupportedDays =
    [
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday,
        DayOfWeek.Saturday
    ];

    [BindProperty(SupportsGet = true)]
    public int? SemesterFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? CourseSectionFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? EditId { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool ShowLookup { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? LookupTarget { get; set; }

    [BindProperty(SupportsGet = true)]
    [DataType(DataType.Date)]
    public DateTime? LookupDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? InputCourseSectionId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? InputDayOfWeekValue { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? InputSessionSlotValue { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? InputRoomValue { get; set; }

    [BindProperty(SupportsGet = true)]
    [DataType(DataType.Date)]
    public DateTime? InputStartDateValue { get; set; }

    [BindProperty(SupportsGet = true)]
    [DataType(DataType.Date)]
    public DateTime? InputEndDateValue { get; set; }

    [BindProperty]
    public ScheduleInputModel Input { get; set; } = ScheduleInputModel.CreateDefault();

    public IReadOnlyList<ScheduleSlot> ScheduleSlots { get; private set; } = [];
    public IReadOnlyList<CourseSection> CourseSections { get; private set; } = [];
    public IReadOnlyList<Semester> Semesters { get; private set; } = [];
    public IReadOnlyList<global::StudentManagementSystem.Shared.Entities.Lecturer> Lecturers { get; private set; } = [];
    public IReadOnlyList<string> LookupRoomOptions { get; private set; } = [];
    public IReadOnlyList<LookupCourseSectionOptionViewModel> LookupCourseSectionOptions { get; private set; } = [];
    public IReadOnlyList<LookupDayOptionViewModel> LookupDayOptions { get; private set; } = [];
    public IReadOnlyList<LookupSlotOptionViewModel> LookupSlotOptions { get; private set; } = [];
    public IReadOnlyList<LookupRoomOptionViewModel> LookupRoomChoices { get; private set; } = [];
    public IReadOnlyList<ScheduleSlot> LookupBusySchedules { get; private set; } = [];
    public TimetableViewModel? LookupTimetable { get; private set; }
    public Semester? SelectedSemester { get; private set; }
    public CourseSection? ActiveInputCourseSection => CourseSections.FirstOrDefault(x => x.CourseSectionId == Input.CourseSectionId);
    public string ActiveLookupTarget => LookupTarget switch
    {
        LookupTargetCourseSection or LookupTargetDay or LookupTargetSessionSlot or LookupTargetRoom => LookupTarget,
        _ => LookupTargetSessionSlot
    };

    public string ActiveLookupTargetLabel => ActiveLookupTarget switch
    {
        LookupTargetCourseSection => "Course Section / Lecturer",
        LookupTargetDay => "Day of Week",
        LookupTargetRoom => "Room",
        _ => "Session Slot"
    };

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnGetLookupAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);
        return new JsonResult(BuildLookupResponse());
    }

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadAsync(cancellationToken);
            return Page();
        }

        var isUpdate = Input.ScheduleSlotId.HasValue;
        var result = isUpdate
            ? await academicService.UpdateScheduleSlotAsync(MapRequest(), cancellationToken)
            : await academicService.CreateScheduleSlotAsync(MapRequest(), cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage ?? (isUpdate ? "Failed to update schedule slot." : "Failed to create schedule slot."));
            await LoadAsync(cancellationToken);
            return Page();
        }

        TempData["StatusMessage"] = isUpdate
            ? "Schedule slot updated successfully."
            : "Schedule slot created successfully.";
        return RedirectToPage(new { SemesterFilter, CourseSectionFilter });
    }

    public async Task<IActionResult> OnPostUpdateAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            EditId = Input.ScheduleSlotId;
            await LoadAsync(cancellationToken);
            return Page();
        }

        var result = await academicService.UpdateScheduleSlotAsync(MapRequest(), cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to update schedule slot.");
            EditId = Input.ScheduleSlotId;
            await LoadAsync(cancellationToken);
            return Page();
        }

        TempData["StatusMessage"] = "Schedule slot updated successfully.";
        return RedirectToPage(new { SemesterFilter, CourseSectionFilter });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int scheduleSlotId, CancellationToken cancellationToken)
    {
        await academicService.DeleteScheduleSlotAsync(scheduleSlotId, cancellationToken);
        TempData["StatusMessage"] = "Schedule slot deleted.";
        return RedirectToPage(new { SemesterFilter, CourseSectionFilter });
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        var isGet = HttpContext.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase);

        Semesters = await academicService.GetSemestersAsync(null, cancellationToken);
        if (!SemesterFilter.HasValue)
        {
            SemesterFilter = Semesters.FirstOrDefault(x => x.Status == SemesterStatus.OpenForRegistration)?.SemesterId
                ?? Semesters.FirstOrDefault()?.SemesterId;
        }

        SelectedSemester = SemesterFilter.HasValue
            ? Semesters.FirstOrDefault(x => x.SemesterId == SemesterFilter.Value)
            : null;

        CourseSections = await academicService.GetCourseSectionsAsync(SemesterFilter, null, null, null, cancellationToken);
        ScheduleSlots = await academicService.GetScheduleSlotsAsync(SemesterFilter, CourseSectionFilter, null, null, cancellationToken);
        var semesterSlots = await academicService.GetScheduleSlotsAsync(SemesterFilter, null, null, null, cancellationToken);
        Lecturers = await academicService.GetLecturersAsync(cancellationToken);

        if (EditId.HasValue)
        {
            var slot = ScheduleSlots.FirstOrDefault(x => x.ScheduleSlotId == EditId.Value);
            if (slot is null)
            {
                TempData["ErrorMessage"] = "Schedule slot not found.";
                EditId = null;
            }
            else
            {
                Input = new ScheduleInputModel
                {
                    ScheduleSlotId = slot.ScheduleSlotId,
                    CourseSectionId = slot.CourseSectionId,
                    Room = slot.Room,
                    DayOfWeek = slot.DayOfWeek,
                    SessionSlot = slot.SessionSlot,
                    StartDate = slot.StartDate.Date,
                    EndDate = slot.EndDate.Date
                };
            }
        }

        if (isGet)
        {
            if (!EditId.HasValue && SelectedSemester is not null)
            {
                Input.StartDate = SelectedSemester.StartDate.Date;
                Input.EndDate = SelectedSemester.EndDate.Date;
            }

            ApplyInputOverridesFromQuery();
        }

        NormalizeInput();

        LookupDate ??= ResolveLookupDate();
        BuildLookupData(semesterSlots);
    }

    private void ApplyInputOverridesFromQuery()
    {
        if (InputCourseSectionId.HasValue)
        {
            Input.CourseSectionId = InputCourseSectionId.Value;
        }

        if (InputDayOfWeekValue.HasValue &&
            Enum.IsDefined(typeof(DayOfWeek), InputDayOfWeekValue.Value) &&
            InputDayOfWeekValue.Value is >= 1 and <= 6)
        {
            Input.DayOfWeek = (DayOfWeek)InputDayOfWeekValue.Value;
        }

        if (InputSessionSlotValue.HasValue && InputSessionSlotValue.Value > 0)
        {
            Input.SessionSlot = InputSessionSlotValue.Value;
        }

        if (InputRoomValue is not null)
        {
            Input.Room = InputRoomValue.Trim();
        }

        if (InputStartDateValue.HasValue)
        {
            Input.StartDate = InputStartDateValue.Value.Date;
        }

        if (InputEndDateValue.HasValue)
        {
            Input.EndDate = InputEndDateValue.Value.Date;
        }
    }

    private void NormalizeInput()
    {
        if (Input.CourseSectionId == 0 && CourseSections.Count > 0)
        {
            Input.CourseSectionId = CourseSections[0].CourseSectionId;
        }

        if (Input.SessionSlot <= 0)
        {
            Input.SessionSlot = 1;
        }

        if (Input.DayOfWeek is DayOfWeek.Sunday || !SupportedDays.Contains(Input.DayOfWeek))
        {
            Input.DayOfWeek = DayOfWeek.Monday;
        }

        if (SelectedSemester is not null)
        {
            if (Input.StartDate == default || Input.StartDate.Date < SelectedSemester.StartDate.Date)
            {
                Input.StartDate = AlignDateToDay(SelectedSemester.StartDate.Date, Input.DayOfWeek);
            }

            if (Input.EndDate == default || Input.EndDate.Date < Input.StartDate.Date)
            {
                Input.EndDate = SelectedSemester.EndDate.Date;
            }

            if (Input.EndDate.Date > SelectedSemester.EndDate.Date)
            {
                Input.EndDate = SelectedSemester.EndDate.Date;
            }
        }
        else if (Input.StartDate == default)
        {
            Input.StartDate = DateTime.Today;
        }

        if (Input.EndDate == default)
        {
            Input.EndDate = Input.StartDate.Date.AddMonths(4);
        }
    }

    private DateTime ResolveLookupDate()
    {
        var candidate = Input.StartDate.Date == default
            ? GetSuggestedFocusDate(SelectedSemester)
            : Input.StartDate.Date;

        candidate = AlignDateToDay(candidate, Input.DayOfWeek);

        if (SelectedSemester is null)
        {
            return candidate;
        }

        if (candidate.Date < SelectedSemester.StartDate.Date)
        {
            candidate = AlignDateToDay(SelectedSemester.StartDate.Date, Input.DayOfWeek);
        }

        return candidate.Date > SelectedSemester.EndDate.Date
            ? GetSuggestedFocusDate(SelectedSemester)
            : candidate.Date;
    }

    private ScheduleSlotUpsertRequest MapRequest() => new()
    {
        ScheduleSlotId = Input.ScheduleSlotId,
        CourseSectionId = Input.CourseSectionId,
        Room = Input.Room,
        DayOfWeek = Input.DayOfWeek,
        SessionSlot = Input.SessionSlot,
        StartDate = Input.StartDate,
        EndDate = Input.EndDate
    };

    private LookupResponse BuildLookupResponse()
    {
        var selectedSection = ActiveInputCourseSection;

        return new LookupResponse
        {
            LookupTarget = ActiveLookupTarget,
            LookupTargetLabel = ActiveLookupTargetLabel,
            LookupDateValue = (LookupDate?.Date ?? ResolveLookupDate()).ToString("yyyy-MM-dd"),
            LookupDateText = (LookupDate?.Date ?? ResolveLookupDate()).ToString("dd/MM/yyyy"),
            Context = new LookupContextResponse
            {
                LecturerName = selectedSection?.Lecturer?.UserAccount?.FullName ?? "No lecturer selected",
                SectionCode = selectedSection?.SectionCode ?? "No class selected",
                SectionName = selectedSection?.SectionName ?? string.Empty,
                SubjectText = selectedSection?.Subject is null
                    ? "No subject selected"
                    : $"{selectedSection.Subject.SubjectCode} - {selectedSection.Subject.SubjectName}",
                DayLabel = TimetableViewModel.GetDayLabel(Input.DayOfWeek),
                SessionSlotLabel = $"Slot {Input.SessionSlot}",
                Room = string.IsNullOrWhiteSpace(Input.Room) ? "No room selected" : Input.Room.Trim()
            },
            CourseSectionOptions = LookupCourseSectionOptions
                .Select(option => new LookupCourseSectionOptionResponse
                {
                    CourseSectionId = option.CourseSectionId,
                    SectionCode = option.SectionCode,
                    SectionName = option.SectionName,
                    SubjectText = option.SubjectText,
                    LecturerName = option.LecturerName,
                    LecturerDepartment = option.LecturerDepartment,
                    IsAvailable = option.IsAvailable,
                    IsSelected = option.IsSelected,
                    StatusText = option.StatusText
                })
                .ToList(),
            DayOptions = LookupDayOptions
                .Select(option => new LookupDayOptionResponse
                {
                    DayValue = option.DayValue,
                    DayLabel = option.DayLabel,
                    SuggestedDateValue = option.SuggestedDateValue,
                    SuggestedDateText = option.SuggestedDateText,
                    IsAvailable = option.IsAvailable,
                    IsSelected = option.IsSelected,
                    StatusText = option.StatusText
                })
                .ToList(),
            SlotOptions = LookupSlotOptions
                .Select(option => new LookupSlotOptionResponse
                {
                    SessionSlot = option.SessionSlot,
                    BusyScheduleCount = option.BusyScheduleCount,
                    IsAvailable = option.IsAvailable,
                    IsSelected = option.IsSelected,
                    StatusText = option.StatusText
                })
                .ToList(),
            RoomOptions = LookupRoomChoices
                .Select(option => new LookupRoomOptionResponse
                {
                    Room = option.Room,
                    IsAvailable = option.IsAvailable,
                    IsSelected = option.IsSelected,
                    StatusText = option.StatusText
                })
                .ToList(),
            BusySchedules = LookupBusySchedules
                .Select(slot => new LookupBusyScheduleResponse
                {
                    SectionCode = slot.CourseSection?.SectionCode ?? string.Empty,
                    SectionName = slot.CourseSection?.SectionName ?? string.Empty,
                    LecturerName = slot.CourseSection?.Lecturer?.UserAccount?.FullName ?? "Unassigned",
                    DayLabel = TimetableViewModel.GetDayLabel(slot.DayOfWeek),
                    SessionSlotLabel = $"Slot {slot.SessionSlot}",
                    Room = slot.Room,
                    DateRangeText = $"{slot.StartDate:dd/MM/yyyy} - {slot.EndDate:dd/MM/yyyy}"
                })
                .ToList(),
            Timetable = LookupTimetable is null ? null : BuildLookupTimetableResponse(LookupTimetable)
        };
    }

    private static LookupTimetableResponse BuildLookupTimetableResponse(TimetableViewModel timetable)
    {
        return new LookupTimetableResponse
        {
            WeekLabel = timetable.WeekLabel,
            DayFilterLabel = timetable.DayFilter.HasValue
                ? TimetableViewModel.GetDayLabel((DayOfWeek)timetable.DayFilter.Value)
                : null,
            SessionSlotFilterLabel = timetable.SessionSlotFilter.HasValue
                ? TimetableViewModel.GetSlotLabel(timetable.SessionSlotFilter.Value)
                : null,
            Rows = timetable.SessionSlots
                .Select(slot => new LookupTimetableRowResponse
                {
                    SessionSlot = slot,
                    SessionSlotLabel = TimetableViewModel.GetSlotLabel(slot),
                    Cells = timetable.Days
                        .Select(day => new LookupTimetableCellResponse
                        {
                            DayLabel = TimetableViewModel.GetDayLabel(day),
                            Sessions = timetable.GetCellSlots(day, slot)
                                .Select(item => new LookupTimetableSessionResponse
                                {
                                    SubjectCode = item.CourseSection?.Subject?.SubjectCode ?? string.Empty,
                                    SectionText = $"{item.CourseSection?.SectionCode} - {item.CourseSection?.SectionName}",
                                    Room = item.Room,
                                    DateRangeShortText = $"{item.StartDate:dd/MM} - {item.EndDate:dd/MM}"
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .ToList(),
            VisibleSessions = timetable.VisibleSlots
                .Select(item => new LookupVisibleSessionResponse
                {
                    SubjectText = item.CourseSection?.Subject is null
                        ? "Unknown subject"
                        : $"{item.CourseSection.Subject.SubjectCode} - {item.CourseSection.Subject.SubjectName}",
                    SectionText = $"{item.CourseSection?.SectionCode} - {item.CourseSection?.SectionName}",
                    DayLabel = TimetableViewModel.GetDayLabel(item.DayOfWeek),
                    SessionSlotLabel = TimetableViewModel.GetSlotLabel(item.SessionSlot),
                    Room = item.Room,
                    DateRangeText = $"{item.StartDate:dd/MM/yyyy} - {item.EndDate:dd/MM/yyyy}"
                })
                .ToList()
        };
    }

    private static DateTime GetSuggestedFocusDate(Semester? semester)
    {
        var candidate = DateTime.Today;
        if (semester is null)
        {
            return candidate.DayOfWeek == DayOfWeek.Sunday ? candidate.AddDays(1) : candidate;
        }

        if (candidate.Date < semester.StartDate.Date || candidate.Date > semester.EndDate.Date)
        {
            candidate = semester.StartDate.Date;
        }

        if (candidate.DayOfWeek == DayOfWeek.Sunday)
        {
            candidate = candidate.AddDays(1);
        }

        return candidate.Date > semester.EndDate.Date
            ? semester.StartDate.Date
            : candidate.Date;
    }

    private static DateTime AlignDateToDay(DateTime date, DayOfWeek targetDay)
    {
        var candidate = date.Date;
        if (targetDay == DayOfWeek.Sunday)
        {
            return candidate;
        }

        var guard = 0;
        while (candidate.DayOfWeek != targetDay && guard < 7)
        {
            candidate = candidate.AddDays(1);
            guard++;
        }

        return candidate;
    }

    private void BuildLookupData(IReadOnlyList<ScheduleSlot> semesterSlots)
    {
        var lookupDate = LookupDate?.Date ?? ResolveLookupDate();
        var inputStartDate = Input.StartDate.Date;
        var inputEndDate = Input.EndDate.Date >= inputStartDate
            ? Input.EndDate.Date
            : inputStartDate;
        var selectedSection = ActiveInputCourseSection;
        var selectedLecturerId = selectedSection?.LecturerId;
        var normalizedRoom = Input.Room.Trim();

        LookupRoomOptions = semesterSlots
            .Select(x => x.Room)
            .Append(normalizedRoom)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        LookupCourseSectionOptions = CourseSections
            .OrderBy(x => x.SectionCode)
            .Select(section =>
            {
                var lecturerBusy = HasLecturerConflict(
                    semesterSlots,
                    section.LecturerId,
                    Input.DayOfWeek,
                    Input.SessionSlot,
                    inputStartDate,
                    inputEndDate);

                return new LookupCourseSectionOptionViewModel
                {
                    CourseSectionId = section.CourseSectionId,
                    SectionCode = section.SectionCode,
                    SectionName = section.SectionName,
                    SubjectText = $"{section.Subject?.SubjectCode} - {section.Subject?.SubjectName}",
                    LecturerName = section.Lecturer?.UserAccount?.FullName ?? "Unassigned",
                    LecturerDepartment = section.Lecturer?.Department ?? string.Empty,
                    IsAvailable = !lecturerBusy,
                    IsSelected = Input.CourseSectionId == section.CourseSectionId,
                    StatusText = lecturerBusy
                        ? $"Lecturer busy on {Input.DayOfWeek} / Slot {Input.SessionSlot}"
                        : $"Lecturer available on {Input.DayOfWeek} / Slot {Input.SessionSlot}"
                };
            })
            .ToList();

        LookupDayOptions = SupportedDays
            .Select(day =>
            {
                var lecturerBusy = selectedLecturerId.HasValue &&
                    HasLecturerConflict(semesterSlots, selectedLecturerId.Value, day, Input.SessionSlot, inputStartDate, inputEndDate);
                var roomBusy = !string.IsNullOrWhiteSpace(normalizedRoom) &&
                    HasRoomConflict(semesterSlots, normalizedRoom, day, Input.SessionSlot, inputStartDate, inputEndDate);
                var suggestedDate = AlignDateToDay(lookupDate, day);
                var isAvailable = !lecturerBusy && !roomBusy;

                return new LookupDayOptionViewModel
                {
                    DayValue = (int)day,
                    DayLabel = TimetableViewModel.GetDayLabel(day),
                    SuggestedDateValue = suggestedDate.ToString("yyyy-MM-dd"),
                    SuggestedDateText = suggestedDate.ToString("dd/MM/yyyy"),
                    IsAvailable = isAvailable,
                    IsSelected = Input.DayOfWeek == day,
                    StatusText = BuildAvailabilityStatus(
                        isAvailable,
                        lecturerBusy,
                        roomBusy,
                        roomLabel: normalizedRoom,
                        subjectLabel: selectedSection?.SectionCode)
                };
            })
            .ToList();

        LookupSlotOptions = Enumerable.Range(1, 6)
            .Select(slot =>
            {
                var lecturerBusy = selectedLecturerId.HasValue &&
                    HasLecturerConflict(semesterSlots, selectedLecturerId.Value, Input.DayOfWeek, slot, inputStartDate, inputEndDate);
                var roomBusy = !string.IsNullOrWhiteSpace(normalizedRoom) &&
                    HasRoomConflict(semesterSlots, normalizedRoom, Input.DayOfWeek, slot, inputStartDate, inputEndDate);
                var busyScheduleCount = semesterSlots.Count(x =>
                    x.ScheduleSlotId != Input.ScheduleSlotId &&
                    x.DayOfWeek == Input.DayOfWeek &&
                    x.SessionSlot == slot &&
                    RangesOverlap(x.StartDate.Date, x.EndDate.Date, inputStartDate, inputEndDate));
                var isAvailable = !lecturerBusy && !roomBusy;

                return new LookupSlotOptionViewModel
                {
                    SessionSlot = slot,
                    BusyScheduleCount = busyScheduleCount,
                    IsAvailable = isAvailable,
                    IsSelected = Input.SessionSlot == slot,
                    StatusText = BuildAvailabilityStatus(
                        isAvailable,
                        lecturerBusy,
                        roomBusy,
                        roomLabel: normalizedRoom,
                        subjectLabel: selectedSection?.SectionCode)
                };
            })
            .ToList();

        LookupRoomChoices = LookupRoomOptions
            .Select(room =>
            {
                var roomBusy = HasRoomConflict(semesterSlots, room, Input.DayOfWeek, Input.SessionSlot, inputStartDate, inputEndDate);
                return new LookupRoomOptionViewModel
                {
                    Room = room,
                    IsAvailable = !roomBusy,
                    IsSelected = room.Equals(normalizedRoom, StringComparison.OrdinalIgnoreCase),
                    StatusText = roomBusy
                        ? $"Room busy on {Input.DayOfWeek} / Slot {Input.SessionSlot}"
                        : $"Room available on {Input.DayOfWeek} / Slot {Input.SessionSlot}"
                };
            })
            .ToList();

        LookupBusySchedules = BuildBusySchedules(
            semesterSlots,
            selectedLecturerId,
            normalizedRoom,
            inputStartDate,
            inputEndDate);

        LookupTimetable = BuildTimetable(semesterSlots, lookupDate, selectedLecturerId, normalizedRoom);
    }

    private IReadOnlyList<ScheduleSlot> BuildBusySchedules(
        IReadOnlyList<ScheduleSlot> semesterSlots,
        int? selectedLecturerId,
        string normalizedRoom,
        DateTime inputStartDate,
        DateTime inputEndDate)
    {
        IEnumerable<ScheduleSlot> query = semesterSlots
            .Where(x => x.ScheduleSlotId != Input.ScheduleSlotId)
            .Where(x => RangesOverlap(x.StartDate.Date, x.EndDate.Date, inputStartDate, inputEndDate));

        query = ActiveLookupTarget switch
        {
            LookupTargetCourseSection => query
                .Where(x => x.DayOfWeek == Input.DayOfWeek && x.SessionSlot == Input.SessionSlot),
            LookupTargetDay => query
                .Where(x => x.SessionSlot == Input.SessionSlot)
                .Where(x => !selectedLecturerId.HasValue || x.CourseSection?.LecturerId == selectedLecturerId.Value)
                .Where(x => string.IsNullOrWhiteSpace(normalizedRoom) || x.Room.Equals(normalizedRoom, StringComparison.OrdinalIgnoreCase)),
            LookupTargetRoom => query
                .Where(x => x.DayOfWeek == Input.DayOfWeek && x.SessionSlot == Input.SessionSlot),
            _ => query
                .Where(x => x.DayOfWeek == Input.DayOfWeek)
                .Where(x => !selectedLecturerId.HasValue || x.CourseSection?.LecturerId == selectedLecturerId.Value)
                .Where(x => string.IsNullOrWhiteSpace(normalizedRoom) || x.Room.Equals(normalizedRoom, StringComparison.OrdinalIgnoreCase))
        };

        return query
            .OrderBy(x => x.DayOfWeek)
            .ThenBy(x => x.SessionSlot)
            .ThenBy(x => x.Room)
            .ThenBy(x => x.CourseSection!.SectionCode)
            .ToList();
    }

    private TimetableViewModel BuildTimetable(
        IReadOnlyList<ScheduleSlot> semesterSlots,
        DateTime lookupDate,
        int? selectedLecturerId,
        string normalizedRoom)
    {
        IEnumerable<ScheduleSlot> timetableSource = semesterSlots;
        int? dayFilter = Input.DayOfWeek is DayOfWeek.Sunday ? null : (int)Input.DayOfWeek;
        int? slotFilter = Input.SessionSlot > 0 ? Input.SessionSlot : null;

        switch (ActiveLookupTarget)
        {
            case LookupTargetCourseSection:
                if (selectedLecturerId.HasValue)
                {
                    timetableSource = timetableSource.Where(x => x.CourseSection?.LecturerId == selectedLecturerId.Value);
                }
                break;

            case LookupTargetDay:
                if (selectedLecturerId.HasValue)
                {
                    timetableSource = timetableSource.Where(x => x.CourseSection?.LecturerId == selectedLecturerId.Value);
                }
                else if (!string.IsNullOrWhiteSpace(normalizedRoom))
                {
                    timetableSource = timetableSource.Where(x => x.Room.Equals(normalizedRoom, StringComparison.OrdinalIgnoreCase));
                }

                dayFilter = null;
                break;

            case LookupTargetRoom:
                if (!string.IsNullOrWhiteSpace(normalizedRoom))
                {
                    timetableSource = timetableSource.Where(x => x.Room.Equals(normalizedRoom, StringComparison.OrdinalIgnoreCase));
                }
                else if (selectedLecturerId.HasValue)
                {
                    timetableSource = timetableSource.Where(x => x.CourseSection?.LecturerId == selectedLecturerId.Value);
                }

                dayFilter = null;
                slotFilter = null;
                break;

            default:
                if (selectedLecturerId.HasValue)
                {
                    timetableSource = timetableSource.Where(x => x.CourseSection?.LecturerId == selectedLecturerId.Value);
                }
                else if (!string.IsNullOrWhiteSpace(normalizedRoom))
                {
                    timetableSource = timetableSource.Where(x => x.Room.Equals(normalizedRoom, StringComparison.OrdinalIgnoreCase));
                }

                slotFilter = null;
                break;
        }

        return new TimetableViewModel(lookupDate, timetableSource, dayFilter, slotFilter);
    }

    private bool HasLecturerConflict(
        IEnumerable<ScheduleSlot> semesterSlots,
        int lecturerId,
        DayOfWeek dayOfWeek,
        int sessionSlot,
        DateTime startDate,
        DateTime endDate)
    {
        return semesterSlots.Any(x =>
            x.ScheduleSlotId != Input.ScheduleSlotId &&
            x.CourseSection?.LecturerId == lecturerId &&
            x.DayOfWeek == dayOfWeek &&
            x.SessionSlot == sessionSlot &&
            RangesOverlap(x.StartDate.Date, x.EndDate.Date, startDate, endDate));
    }

    private bool HasRoomConflict(
        IEnumerable<ScheduleSlot> semesterSlots,
        string room,
        DayOfWeek dayOfWeek,
        int sessionSlot,
        DateTime startDate,
        DateTime endDate)
    {
        return semesterSlots.Any(x =>
            x.ScheduleSlotId != Input.ScheduleSlotId &&
            x.DayOfWeek == dayOfWeek &&
            x.SessionSlot == sessionSlot &&
            x.Room.Equals(room, StringComparison.OrdinalIgnoreCase) &&
            RangesOverlap(x.StartDate.Date, x.EndDate.Date, startDate, endDate));
    }

    private static bool RangesOverlap(DateTime startA, DateTime endA, DateTime startB, DateTime endB) =>
        startA <= endB && startB <= endA;

    private static string BuildAvailabilityStatus(
        bool isAvailable,
        bool lecturerBusy,
        bool roomBusy,
        string? roomLabel,
        string? subjectLabel)
    {
        if (isAvailable)
        {
            return "Available for the current date range.";
        }

        if (lecturerBusy && roomBusy)
        {
            return $"Lecturer and room {(string.IsNullOrWhiteSpace(roomLabel) ? "selection" : roomLabel)} both conflict.";
        }

        if (lecturerBusy)
        {
            return $"Lecturer conflict detected for {(string.IsNullOrWhiteSpace(subjectLabel) ? "the selected class" : subjectLabel)}.";
        }

        return $"Room {(string.IsNullOrWhiteSpace(roomLabel) ? "selection" : roomLabel)} is already occupied.";
    }

    public sealed class LookupCourseSectionOptionViewModel
    {
        public int CourseSectionId { get; init; }

        public string SectionCode { get; init; } = string.Empty;

        public string SectionName { get; init; } = string.Empty;

        public string SubjectText { get; init; } = string.Empty;

        public string LecturerName { get; init; } = string.Empty;

        public string LecturerDepartment { get; init; } = string.Empty;

        public bool IsAvailable { get; init; }

        public bool IsSelected { get; init; }

        public string StatusText { get; init; } = string.Empty;
    }

    public sealed class LookupDayOptionViewModel
    {
        public int DayValue { get; init; }

        public string DayLabel { get; init; } = string.Empty;

        public string SuggestedDateValue { get; init; } = string.Empty;

        public string SuggestedDateText { get; init; } = string.Empty;

        public bool IsAvailable { get; init; }

        public bool IsSelected { get; init; }

        public string StatusText { get; init; } = string.Empty;
    }

    public sealed class LookupSlotOptionViewModel
    {
        public int SessionSlot { get; init; }

        public int BusyScheduleCount { get; init; }

        public bool IsAvailable { get; init; }

        public bool IsSelected { get; init; }

        public string StatusText { get; init; } = string.Empty;
    }

    public sealed class LookupRoomOptionViewModel
    {
        public string Room { get; init; } = string.Empty;

        public bool IsAvailable { get; init; }

        public bool IsSelected { get; init; }

        public string StatusText { get; init; } = string.Empty;
    }

    public sealed class LookupResponse
    {
        public string LookupTarget { get; init; } = string.Empty;

        public string LookupTargetLabel { get; init; } = string.Empty;

        public string LookupDateValue { get; init; } = string.Empty;

        public string LookupDateText { get; init; } = string.Empty;

        public LookupContextResponse Context { get; init; } = new();

        public IReadOnlyList<LookupCourseSectionOptionResponse> CourseSectionOptions { get; init; } = [];

        public IReadOnlyList<LookupDayOptionResponse> DayOptions { get; init; } = [];

        public IReadOnlyList<LookupSlotOptionResponse> SlotOptions { get; init; } = [];

        public IReadOnlyList<LookupRoomOptionResponse> RoomOptions { get; init; } = [];

        public IReadOnlyList<LookupBusyScheduleResponse> BusySchedules { get; init; } = [];

        public LookupTimetableResponse? Timetable { get; init; }
    }

    public sealed class LookupContextResponse
    {
        public string LecturerName { get; init; } = string.Empty;

        public string SectionCode { get; init; } = string.Empty;

        public string SectionName { get; init; } = string.Empty;

        public string SubjectText { get; init; } = string.Empty;

        public string DayLabel { get; init; } = string.Empty;

        public string SessionSlotLabel { get; init; } = string.Empty;

        public string Room { get; init; } = string.Empty;
    }

    public sealed class LookupCourseSectionOptionResponse
    {
        public int CourseSectionId { get; init; }

        public string SectionCode { get; init; } = string.Empty;

        public string SectionName { get; init; } = string.Empty;

        public string SubjectText { get; init; } = string.Empty;

        public string LecturerName { get; init; } = string.Empty;

        public string LecturerDepartment { get; init; } = string.Empty;

        public bool IsAvailable { get; init; }

        public bool IsSelected { get; init; }

        public string StatusText { get; init; } = string.Empty;
    }

    public sealed class LookupDayOptionResponse
    {
        public int DayValue { get; init; }

        public string DayLabel { get; init; } = string.Empty;

        public string SuggestedDateValue { get; init; } = string.Empty;

        public string SuggestedDateText { get; init; } = string.Empty;

        public bool IsAvailable { get; init; }

        public bool IsSelected { get; init; }

        public string StatusText { get; init; } = string.Empty;
    }

    public sealed class LookupSlotOptionResponse
    {
        public int SessionSlot { get; init; }

        public int BusyScheduleCount { get; init; }

        public bool IsAvailable { get; init; }

        public bool IsSelected { get; init; }

        public string StatusText { get; init; } = string.Empty;
    }

    public sealed class LookupRoomOptionResponse
    {
        public string Room { get; init; } = string.Empty;

        public bool IsAvailable { get; init; }

        public bool IsSelected { get; init; }

        public string StatusText { get; init; } = string.Empty;
    }

    public sealed class LookupBusyScheduleResponse
    {
        public string SectionCode { get; init; } = string.Empty;

        public string SectionName { get; init; } = string.Empty;

        public string LecturerName { get; init; } = string.Empty;

        public string DayLabel { get; init; } = string.Empty;

        public string SessionSlotLabel { get; init; } = string.Empty;

        public string Room { get; init; } = string.Empty;

        public string DateRangeText { get; init; } = string.Empty;
    }

    public sealed class LookupTimetableResponse
    {
        public string WeekLabel { get; init; } = string.Empty;

        public string? DayFilterLabel { get; init; }

        public string? SessionSlotFilterLabel { get; init; }

        public IReadOnlyList<LookupTimetableRowResponse> Rows { get; init; } = [];

        public IReadOnlyList<LookupVisibleSessionResponse> VisibleSessions { get; init; } = [];
    }

    public sealed class LookupTimetableRowResponse
    {
        public int SessionSlot { get; init; }

        public string SessionSlotLabel { get; init; } = string.Empty;

        public IReadOnlyList<LookupTimetableCellResponse> Cells { get; init; } = [];
    }

    public sealed class LookupTimetableCellResponse
    {
        public string DayLabel { get; init; } = string.Empty;

        public IReadOnlyList<LookupTimetableSessionResponse> Sessions { get; init; } = [];
    }

    public sealed class LookupTimetableSessionResponse
    {
        public string SubjectCode { get; init; } = string.Empty;

        public string SectionText { get; init; } = string.Empty;

        public string Room { get; init; } = string.Empty;

        public string DateRangeShortText { get; init; } = string.Empty;
    }

    public sealed class LookupVisibleSessionResponse
    {
        public string SubjectText { get; init; } = string.Empty;

        public string SectionText { get; init; } = string.Empty;

        public string DayLabel { get; init; } = string.Empty;

        public string SessionSlotLabel { get; init; } = string.Empty;

        public string Room { get; init; } = string.Empty;

        public string DateRangeText { get; init; } = string.Empty;
    }

    public sealed class ScheduleInputModel
    {
        public static ScheduleInputModel CreateDefault()
        {
            var today = DateTime.Today;
            return new ScheduleInputModel
            {
                StartDate = today,
                EndDate = today.AddMonths(4),
                DayOfWeek = DayOfWeek.Monday,
                SessionSlot = 1
            };
        }

        public int? ScheduleSlotId { get; set; }

        [Range(1, int.MaxValue)]
        public int CourseSectionId { get; set; }

        [Required]
        [StringLength(50)]
        public string Room { get; set; } = string.Empty;

        public DayOfWeek DayOfWeek { get; set; }

        [Range(1, 20)]
        public int SessionSlot { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
    }
}
