using StudentManagementSystem.BLL.Common;
using StudentManagementSystem.BLL.DTOs;
using StudentManagementSystem.BLL.Interfaces;
using StudentManagementSystem.DAL.Repositories.Interfaces;
using StudentManagementSystem.Shared.Entities;

namespace StudentManagementSystem.BLL.Services;

public sealed class AcademicService(IAcademicRepository academicRepository, IUserAccountRepository userAccountRepository) : IAcademicService
{
    public Task<IReadOnlyList<Subject>> GetSubjectsAsync(string? keyword = null, CancellationToken cancellationToken = default) =>
        academicRepository.GetSubjectsAsync(keyword, cancellationToken);

    public Task<IReadOnlyList<Semester>> GetSemestersAsync(string? keyword = null, CancellationToken cancellationToken = default) =>
        academicRepository.GetSemestersAsync(keyword, cancellationToken);

    public Task<IReadOnlyList<CourseSection>> GetCourseSectionsAsync(int? semesterId = null, int? lecturerId = null, string? keyword = null, bool? openOnly = null, CancellationToken cancellationToken = default) =>
        academicRepository.GetCourseSectionsAsync(semesterId, lecturerId, keyword, openOnly, cancellationToken);

    public Task<IReadOnlyList<ScheduleSlot>> GetScheduleSlotsAsync(int? semesterId = null, int? courseSectionId = null, int? lecturerId = null, int? studentId = null, CancellationToken cancellationToken = default) =>
        academicRepository.GetScheduleSlotsAsync(semesterId, courseSectionId, lecturerId, studentId, cancellationToken);

    public Task<IReadOnlyList<Lecturer>> GetLecturersAsync(CancellationToken cancellationToken = default) =>
        userAccountRepository.GetLecturersAsync(cancellationToken);

    public Task<CourseSection?> GetCourseSectionByIdAsync(int courseSectionId, CancellationToken cancellationToken = default) =>
        academicRepository.GetCourseSectionByIdAsync(courseSectionId, cancellationToken);

    public async Task<ServiceResult> CreateSubjectAsync(SubjectUpsertRequest request, CancellationToken cancellationToken = default)
    {
        if (!ValidateSubjectRequest(request).Succeeded)
        {
            return ValidateSubjectRequest(request);
        }

        if (await academicRepository.SubjectCodeExistsAsync(request.SubjectCode, null, cancellationToken))
        {
            return ServiceResult.Failure("Subject code already exists.");
        }

        await academicRepository.AddSubjectAsync(new Subject
        {
            SubjectCode = request.SubjectCode.Trim(),
            SubjectName = request.SubjectName.Trim(),
            Credits = request.Credits,
            TheoryHours = request.TheoryHours,
            PracticeHours = request.PracticeHours,
            IsActive = request.IsActive
        }, request.PrerequisiteIds, cancellationToken);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateSubjectAsync(SubjectUpsertRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.SubjectId.HasValue)
        {
            return ServiceResult.Failure("SubjectId is required.");
        }

        var validation = ValidateSubjectRequest(request);
        if (!validation.Succeeded)
        {
            return validation;
        }

        if (await academicRepository.SubjectCodeExistsAsync(request.SubjectCode, request.SubjectId, cancellationToken))
        {
            return ServiceResult.Failure("Subject code already exists.");
        }

        await academicRepository.UpdateSubjectAsync(new Subject
        {
            SubjectId = request.SubjectId.Value,
            SubjectCode = request.SubjectCode.Trim(),
            SubjectName = request.SubjectName.Trim(),
            Credits = request.Credits,
            TheoryHours = request.TheoryHours,
            PracticeHours = request.PracticeHours,
            IsActive = request.IsActive
        }, request.PrerequisiteIds, cancellationToken);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> SetSubjectActiveAsync(int subjectId, bool isActive, CancellationToken cancellationToken = default)
    {
        if (!isActive && await academicRepository.SubjectHasCourseSectionsAsync(subjectId, cancellationToken))
        {
            return ServiceResult.Failure("Subject has opened course sections. Deactivate course sections first.");
        }

        await academicRepository.SetSubjectActiveAsync(subjectId, isActive, cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> CreateSemesterAsync(SemesterUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var validation = ValidateSemesterRequest(request);
        if (!validation.Succeeded)
        {
            return validation;
        }

        if (await academicRepository.SemesterCodeExistsAsync(request.SemesterCode, null, cancellationToken))
        {
            return ServiceResult.Failure("Semester code already exists.");
        }

        await academicRepository.AddSemesterAsync(MapSemester(request), cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateSemesterAsync(SemesterUpsertRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.SemesterId.HasValue)
        {
            return ServiceResult.Failure("SemesterId is required.");
        }

        var validation = ValidateSemesterRequest(request);
        if (!validation.Succeeded)
        {
            return validation;
        }

        if (await academicRepository.SemesterCodeExistsAsync(request.SemesterCode, request.SemesterId, cancellationToken))
        {
            return ServiceResult.Failure("Semester code already exists.");
        }

        var semester = MapSemester(request);
        semester.SemesterId = request.SemesterId.Value;
        await academicRepository.UpdateSemesterAsync(semester, cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> CreateCourseSectionAsync(CourseSectionUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateCourseSectionRequestAsync(request, null, cancellationToken);
        if (!validation.Succeeded)
        {
            return validation;
        }

        await academicRepository.AddCourseSectionAsync(new CourseSection
        {
            SectionCode = request.SectionCode.Trim(),
            SectionName = request.SectionName.Trim(),
            SubjectId = request.SubjectId,
            SemesterId = request.SemesterId,
            LecturerId = request.LecturerId,
            MaxCapacity = request.MaxCapacity,
            CurrentCapacity = 0,
            IsOpen = request.IsOpen
        }, cancellationToken);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateCourseSectionAsync(CourseSectionUpsertRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.CourseSectionId.HasValue)
        {
            return ServiceResult.Failure("CourseSectionId is required.");
        }

        var existing = await academicRepository.GetCourseSectionByIdAsync(request.CourseSectionId.Value, cancellationToken);
        if (existing is null)
        {
            return ServiceResult.Failure("Course section not found.");
        }

        var validation = await ValidateCourseSectionRequestAsync(request, request.CourseSectionId, cancellationToken);
        if (!validation.Succeeded)
        {
            return validation;
        }

        existing.SectionCode = request.SectionCode.Trim();
        existing.SectionName = request.SectionName.Trim();
        existing.SubjectId = request.SubjectId;
        existing.SemesterId = request.SemesterId;
        existing.LecturerId = request.LecturerId;
        existing.MaxCapacity = request.MaxCapacity;
        existing.IsOpen = request.IsOpen && existing.CurrentCapacity < request.MaxCapacity;

        await academicRepository.UpdateCourseSectionAsync(existing, cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> CreateScheduleSlotAsync(ScheduleSlotUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateScheduleSlotRequestAsync(request, null, cancellationToken);
        if (!validation.Succeeded)
        {
            return validation;
        }

        await academicRepository.AddScheduleSlotAsync(MapScheduleSlot(request), cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateScheduleSlotAsync(ScheduleSlotUpsertRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.ScheduleSlotId.HasValue)
        {
            return ServiceResult.Failure("ScheduleSlotId is required.");
        }

        var validation = await ValidateScheduleSlotRequestAsync(request, request.ScheduleSlotId, cancellationToken);
        if (!validation.Succeeded)
        {
            return validation;
        }

        var slot = MapScheduleSlot(request);
        slot.ScheduleSlotId = request.ScheduleSlotId.Value;
        await academicRepository.UpdateScheduleSlotAsync(slot, cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteScheduleSlotAsync(int scheduleSlotId, CancellationToken cancellationToken = default)
    {
        await academicRepository.DeleteScheduleSlotAsync(scheduleSlotId, cancellationToken);
        return ServiceResult.Success();
    }

    private static ServiceResult ValidateSubjectRequest(SubjectUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SubjectCode) ||
            string.IsNullOrWhiteSpace(request.SubjectName) ||
            request.Credits <= 0)
        {
            return ServiceResult.Failure("Invalid subject information.");
        }

        return ServiceResult.Success();
    }

    private static ServiceResult ValidateSemesterRequest(SemesterUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SemesterCode) ||
            string.IsNullOrWhiteSpace(request.SemesterName) ||
            string.IsNullOrWhiteSpace(request.SchoolYear))
        {
            return ServiceResult.Failure("Invalid semester information.");
        }

        if (request.StartDate > request.EndDate || request.RegistrationStartDate > request.RegistrationEndDate)
        {
            return ServiceResult.Failure("Semester dates are invalid.");
        }

        return ServiceResult.Success();
    }

    private async Task<ServiceResult> ValidateCourseSectionRequestAsync(CourseSectionUpsertRequest request, int? excludeId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SectionCode) ||
            string.IsNullOrWhiteSpace(request.SectionName) ||
            request.MaxCapacity <= 0)
        {
            return ServiceResult.Failure("Invalid course section information.");
        }

        if (await academicRepository.SectionCodeExistsAsync(request.SectionCode, excludeId, cancellationToken))
        {
            return ServiceResult.Failure("Section code already exists.");
        }

        var subject = await academicRepository.GetSubjectByIdAsync(request.SubjectId, cancellationToken);
        var semester = await academicRepository.GetSemesterByIdAsync(request.SemesterId, cancellationToken);
        var lecturers = await userAccountRepository.GetLecturersAsync(cancellationToken);

        if (subject is null || semester is null || lecturers.All(x => x.LecturerId != request.LecturerId))
        {
            return ServiceResult.Failure("Subject, semester, or lecturer is invalid.");
        }

        return ServiceResult.Success();
    }

    private async Task<ServiceResult> ValidateScheduleSlotRequestAsync(ScheduleSlotUpsertRequest request, int? excludeScheduleSlotId, CancellationToken cancellationToken)
    {
        if (request.SessionSlot <= 0 || string.IsNullOrWhiteSpace(request.Room))
        {
            return ServiceResult.Failure("Invalid schedule slot information.");
        }

        var section = await academicRepository.GetCourseSectionByIdAsync(request.CourseSectionId, cancellationToken);
        if (section is null)
        {
            return ServiceResult.Failure("Course section not found.");
        }

        if (request.StartDate > request.EndDate)
        {
            return ServiceResult.Failure("Schedule dates are invalid.");
        }

        if (request.StartDate.DayOfWeek != request.DayOfWeek)
        {
            return ServiceResult.Failure("Start date must match the selected day of week.");
        }

        if (section.Semester is not null &&
            (request.StartDate.Date < section.Semester.StartDate.Date || request.EndDate.Date > section.Semester.EndDate.Date))
        {
            return ServiceResult.Failure("Schedule dates must be inside the selected semester.");
        }

        var lecturerSlots = await academicRepository.GetScheduleSlotsAsync(
            section.SemesterId,
            null,
            section.LecturerId,
            null,
            cancellationToken);

        var lecturerConflicts = lecturerSlots.Any(x =>
            x.ScheduleSlotId != excludeScheduleSlotId &&
            x.DayOfWeek == request.DayOfWeek &&
            x.SessionSlot == request.SessionSlot &&
            x.StartDate <= request.EndDate &&
            request.StartDate <= x.EndDate);

        if (lecturerConflicts)
        {
            return ServiceResult.Failure("Lecturer already has another teaching slot at the same time.");
        }

        var semesterSlots = await academicRepository.GetScheduleSlotsAsync(
            section.SemesterId,
            null,
            null,
            null,
            cancellationToken);

        var roomConflicts = semesterSlots.Any(x =>
            x.ScheduleSlotId != excludeScheduleSlotId &&
            x.DayOfWeek == request.DayOfWeek &&
            x.SessionSlot == request.SessionSlot &&
            x.Room.Equals(request.Room.Trim(), StringComparison.OrdinalIgnoreCase) &&
            x.StartDate <= request.EndDate &&
            request.StartDate <= x.EndDate);

        return roomConflicts
            ? ServiceResult.Failure("Room already has another class at the same time.")
            : ServiceResult.Success();
    }

    private static Semester MapSemester(SemesterUpsertRequest request) => new()
    {
        SemesterCode = request.SemesterCode.Trim(),
        SemesterName = request.SemesterName.Trim(),
        SchoolYear = request.SchoolYear.Trim(),
        StartDate = request.StartDate,
        EndDate = request.EndDate,
        RegistrationStartDate = request.RegistrationStartDate,
        RegistrationEndDate = request.RegistrationEndDate,
        MaxCreditsPerStudent = request.MaxCreditsPerStudent,
        Status = request.Status,
        IsActive = request.IsActive
    };

    private static ScheduleSlot MapScheduleSlot(ScheduleSlotUpsertRequest request) => new()
    {
        CourseSectionId = request.CourseSectionId,
        Room = request.Room.Trim(),
        DayOfWeek = request.DayOfWeek,
        SessionSlot = request.SessionSlot,
        StartDate = request.StartDate,
        EndDate = request.EndDate
    };
}
