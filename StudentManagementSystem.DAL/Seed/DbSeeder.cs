using StudentManagementSystem.DAL.Data;
using StudentManagementSystem.Shared.Configurations;
using StudentManagementSystem.Shared.Entities;
using StudentManagementSystem.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace StudentManagementSystem.DAL.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(
        StudentManagementDbContext context,
        DefaultDatabaseAccountsOptions defaultDatabaseAccountsOptions,
        CancellationToken cancellationToken = default)
    {
        await SeedDefaultAccountsAsync(context, defaultDatabaseAccountsOptions, cancellationToken);

        if (!await context.Subjects.AnyAsync(cancellationToken))
        {
            var subjects = new[]
            {
                new Subject { SubjectCode = "PRN211", SubjectName = "Programming with .NET", Credits = 3, TheoryHours = 30, PracticeHours = 30, IsActive = true },
                new Subject { SubjectCode = "PRN222", SubjectName = "Web Application Development", Credits = 3, TheoryHours = 30, PracticeHours = 30, IsActive = true },
                new Subject { SubjectCode = "DBI202", SubjectName = "Database Systems", Credits = 3, TheoryHours = 30, PracticeHours = 15, IsActive = true },
                new Subject { SubjectCode = "MLN101", SubjectName = "Machine Learning Foundations", Credits = 4, TheoryHours = 45, PracticeHours = 15, IsActive = true }
            };

            context.Subjects.AddRange(subjects);
            await context.SaveChangesAsync(cancellationToken);

            var prn211 = subjects.First(x => x.SubjectCode == "PRN211");
            var prn222 = subjects.First(x => x.SubjectCode == "PRN222");
            var dbi202 = subjects.First(x => x.SubjectCode == "DBI202");
            var mln101 = subjects.First(x => x.SubjectCode == "MLN101");

            context.SubjectPrerequisites.AddRange(
                new SubjectPrerequisite { SubjectId = prn222.SubjectId, PrerequisiteSubjectId = prn211.SubjectId },
                new SubjectPrerequisite { SubjectId = mln101.SubjectId, PrerequisiteSubjectId = dbi202.SubjectId });

            await context.SaveChangesAsync(cancellationToken);
        }

        if (!await context.Semesters.AnyAsync(cancellationToken))
        {
            var now = DateTime.Today;
            context.Semesters.AddRange(
                new Semester
                {
                    SemesterCode = "SU2026",
                    SemesterName = "Summer 2026",
                    SchoolYear = "2025-2026",
                    StartDate = now.AddDays(-30),
                    EndDate = now.AddDays(60),
                    RegistrationStartDate = now.AddDays(-20),
                    RegistrationEndDate = now.AddDays(10),
                    MaxCreditsPerStudent = 15,
                    Status = SemesterStatus.OpenForRegistration,
                    IsActive = true
                },
                new Semester
                {
                    SemesterCode = "FA2026",
                    SemesterName = "Fall 2026",
                    SchoolYear = "2026-2027",
                    StartDate = now.AddDays(90),
                    EndDate = now.AddDays(180),
                    RegistrationStartDate = now.AddDays(60),
                    RegistrationEndDate = now.AddDays(80),
                    MaxCreditsPerStudent = 18,
                    Status = SemesterStatus.Planned,
                    IsActive = true
                });

            await context.SaveChangesAsync(cancellationToken);
        }

        if (!await context.CourseSections.AnyAsync(cancellationToken))
        {
            var prn211 = await context.Subjects.FirstAsync(x => x.SubjectCode == "PRN211", cancellationToken);
            var prn222 = await context.Subjects.FirstAsync(x => x.SubjectCode == "PRN222", cancellationToken);
            var dbi202 = await context.Subjects.FirstAsync(x => x.SubjectCode == "DBI202", cancellationToken);
            var summer = await context.Semesters.FirstAsync(x => x.SemesterCode == "SU2026", cancellationToken);
            var lecturer1 = await context.Lecturers.FirstAsync(x => x.LecturerCode == "LEC001", cancellationToken);
            var lecturer2 = await context.Lecturers.FirstAsync(x => x.LecturerCode == "LEC002", cancellationToken);

            var sections = new[]
            {
                new CourseSection { SectionCode = "PRN211-SU26-01", SectionName = "PRN211 - Group 01", SubjectId = prn211.SubjectId, SemesterId = summer.SemesterId, LecturerId = lecturer1.LecturerId, MaxCapacity = 35, CurrentCapacity = 0, IsOpen = true },
                new CourseSection { SectionCode = "PRN222-SU26-01", SectionName = "PRN222 - Group 01", SubjectId = prn222.SubjectId, SemesterId = summer.SemesterId, LecturerId = lecturer1.LecturerId, MaxCapacity = 30, CurrentCapacity = 0, IsOpen = true },
                new CourseSection { SectionCode = "DBI202-SU26-01", SectionName = "DBI202 - Group 01", SubjectId = dbi202.SubjectId, SemesterId = summer.SemesterId, LecturerId = lecturer2.LecturerId, MaxCapacity = 40, CurrentCapacity = 0, IsOpen = true }
            };

            context.CourseSections.AddRange(sections);
            await context.SaveChangesAsync(cancellationToken);

            context.ScheduleSlots.AddRange(
                new ScheduleSlot { CourseSectionId = sections[0].CourseSectionId, Room = "LAB-A1", DayOfWeek = DayOfWeek.Monday, SessionSlot = 1, StartDate = summer.StartDate, EndDate = summer.EndDate },
                new ScheduleSlot { CourseSectionId = sections[1].CourseSectionId, Room = "LAB-B2", DayOfWeek = DayOfWeek.Wednesday, SessionSlot = 2, StartDate = summer.StartDate, EndDate = summer.EndDate },
                new ScheduleSlot { CourseSectionId = sections[2].CourseSectionId, Room = "ROOM-C3", DayOfWeek = DayOfWeek.Friday, SessionSlot = 3, StartDate = summer.StartDate, EndDate = summer.EndDate });

            await context.SaveChangesAsync(cancellationToken);
        }

        var sectionNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["PRN211-SU26-01"] = "PRN211 - Group 01",
            ["PRN222-SU26-01"] = "PRN222 - Group 01",
            ["DBI202-SU26-01"] = "DBI202 - Group 01"
        };

        var sectionsToNormalize = await context.CourseSections
            .Where(x => string.IsNullOrWhiteSpace(x.SectionName) || x.SectionName.EndsWith("- Group"))
            .ToListAsync(cancellationToken);

        foreach (var section in sectionsToNormalize)
        {
            section.SectionName = sectionNameMap.TryGetValue(section.SectionCode, out var mappedName)
                ? mappedName
                : $"{section.SectionCode} - Group";
        }

        if (sectionsToNormalize.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        if (!await context.Enrollments.AnyAsync(cancellationToken))
        {
            var student1 = await context.Students.FirstAsync(x => x.StudentCode == "SE170001", cancellationToken);
            var student2 = await context.Students.FirstAsync(x => x.StudentCode == "SE170002", cancellationToken);
            var student3 = await context.Students.FirstAsync(x => x.StudentCode == "SE170003", cancellationToken);

            var prn211Section = await context.CourseSections.FirstAsync(x => x.SectionCode == "PRN211-SU26-01", cancellationToken);
            var prn222Section = await context.CourseSections.FirstAsync(x => x.SectionCode == "PRN222-SU26-01", cancellationToken);
            var dbi202Section = await context.CourseSections.FirstAsync(x => x.SectionCode == "DBI202-SU26-01", cancellationToken);

            var enrollments = new[]
            {
                new Enrollment
                {
                    StudentId = student1.StudentId,
                    CourseSectionId = prn211Section.CourseSectionId,
                    RegisteredAt = DateTime.UtcNow.AddDays(-12),
                    Status = EnrollmentStatus.Registered
                },
                new Enrollment
                {
                    StudentId = student1.StudentId,
                    CourseSectionId = dbi202Section.CourseSectionId,
                    RegisteredAt = DateTime.UtcNow.AddDays(-11),
                    Status = EnrollmentStatus.Registered
                },
                new Enrollment
                {
                    StudentId = student2.StudentId,
                    CourseSectionId = prn211Section.CourseSectionId,
                    RegisteredAt = DateTime.UtcNow.AddDays(-10),
                    Status = EnrollmentStatus.Registered
                },
                new Enrollment
                {
                    StudentId = student3.StudentId,
                    CourseSectionId = prn222Section.CourseSectionId,
                    RegisteredAt = DateTime.UtcNow.AddDays(-9),
                    Status = EnrollmentStatus.Registered
                }
            };

            context.Enrollments.AddRange(enrollments);
            await context.SaveChangesAsync(cancellationToken);

            context.GradeRecords.AddRange(
                new GradeRecord
                {
                    EnrollmentId = enrollments[0].EnrollmentId,
                    AssignmentScore = 8.0m,
                    QuizScore = 7.5m,
                    MidtermScore = 7.8m,
                    FinalScore = 8.6m,
                    TotalScore = 8.18m,
                    LetterGrade = "B",
                    IsPassed = true,
                    UpdatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new GradeRecord
                {
                    EnrollmentId = enrollments[1].EnrollmentId,
                    AssignmentScore = 6.5m,
                    QuizScore = 6.0m,
                    MidtermScore = 5.8m,
                    FinalScore = 6.2m,
                    TotalScore = 6.14m,
                    LetterGrade = "C",
                    IsPassed = true,
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new GradeRecord
                {
                    EnrollmentId = enrollments[2].EnrollmentId,
                    AssignmentScore = 4.5m,
                    QuizScore = 5.0m,
                    MidtermScore = 4.0m,
                    FinalScore = 4.3m,
                    TotalScore = 4.31m,
                    LetterGrade = "D",
                    IsPassed = true,
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                }
            );

            prn211Section.CurrentCapacity = 2;
            prn222Section.CurrentCapacity = 1;
            dbi202Section.CurrentCapacity = 1;

            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task SeedDefaultAccountsAsync(
        StudentManagementDbContext context,
        DefaultDatabaseAccountsOptions defaultDatabaseAccountsOptions,
        CancellationToken cancellationToken)
    {
        foreach (var lecturerOptions in defaultDatabaseAccountsOptions.Lecturers.Where(IsValidLecturer))
        {
            var userAccount = await context.UserAccounts
                .Include(x => x.Lecturer)
                .FirstOrDefaultAsync(x => x.Email == lecturerOptions.Email, cancellationToken);

            if (userAccount is null)
            {
                userAccount = new UserAccount();
                context.UserAccounts.Add(userAccount);
            }

            userAccount.FullName = lecturerOptions.FullName.Trim();
            userAccount.Email = lecturerOptions.Email.Trim();
            userAccount.Password = lecturerOptions.Password.Trim();
            userAccount.Role = UserRole.Lecturer;
            userAccount.IsActive = lecturerOptions.IsActive;
            userAccount.ModifiedDate = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            if (userAccount.Lecturer is null)
            {
                context.Lecturers.Add(new Lecturer
                {
                    UserAccountId = userAccount.UserAccountId,
                    LecturerCode = lecturerOptions.LecturerCode.Trim(),
                    Department = lecturerOptions.Department.Trim(),
                    OfficeRoom = lecturerOptions.OfficeRoom?.Trim()
                });
            }
            else
            {
                userAccount.Lecturer.LecturerCode = lecturerOptions.LecturerCode.Trim();
                userAccount.Lecturer.Department = lecturerOptions.Department.Trim();
                userAccount.Lecturer.OfficeRoom = lecturerOptions.OfficeRoom?.Trim();
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        foreach (var studentOptions in defaultDatabaseAccountsOptions.Students.Where(IsValidStudent))
        {
            var userAccount = await context.UserAccounts
                .Include(x => x.Student)
                .FirstOrDefaultAsync(x => x.Email == studentOptions.Email, cancellationToken);

            if (userAccount is null)
            {
                userAccount = new UserAccount();
                context.UserAccounts.Add(userAccount);
            }

            userAccount.FullName = studentOptions.FullName.Trim();
            userAccount.Email = studentOptions.Email.Trim();
            userAccount.Password = studentOptions.Password.Trim();
            userAccount.Role = UserRole.Student;
            userAccount.IsActive = studentOptions.IsActive;
            userAccount.ModifiedDate = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            if (userAccount.Student is null)
            {
                context.Students.Add(new Student
                {
                    UserAccountId = userAccount.UserAccountId,
                    StudentCode = studentOptions.StudentCode.Trim(),
                    DateOfBirth = studentOptions.DateOfBirth,
                    Gender = studentOptions.Gender,
                    PhoneNumber = studentOptions.PhoneNumber?.Trim(),
                    Address = studentOptions.Address?.Trim(),
                    Major = studentOptions.Major.Trim(),
                    Cohort = studentOptions.Cohort.Trim(),
                    AcademicStatus = studentOptions.AcademicStatus,
                    IsActive = studentOptions.IsActive
                });
            }
            else
            {
                userAccount.Student.StudentCode = studentOptions.StudentCode.Trim();
                userAccount.Student.DateOfBirth = studentOptions.DateOfBirth;
                userAccount.Student.Gender = studentOptions.Gender;
                userAccount.Student.PhoneNumber = studentOptions.PhoneNumber?.Trim();
                userAccount.Student.Address = studentOptions.Address?.Trim();
                userAccount.Student.Major = studentOptions.Major.Trim();
                userAccount.Student.Cohort = studentOptions.Cohort.Trim();
                userAccount.Student.AcademicStatus = studentOptions.AcademicStatus;
                userAccount.Student.IsActive = studentOptions.IsActive;
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static bool IsValidLecturer(DefaultLecturerAccountOptions options) =>
        !string.IsNullOrWhiteSpace(options.FullName) &&
        !string.IsNullOrWhiteSpace(options.Email) &&
        !string.IsNullOrWhiteSpace(options.Password) &&
        !string.IsNullOrWhiteSpace(options.LecturerCode) &&
        !string.IsNullOrWhiteSpace(options.Department);

    private static bool IsValidStudent(DefaultStudentAccountOptions options) =>
        !string.IsNullOrWhiteSpace(options.FullName) &&
        !string.IsNullOrWhiteSpace(options.Email) &&
        !string.IsNullOrWhiteSpace(options.Password) &&
        !string.IsNullOrWhiteSpace(options.StudentCode) &&
        !string.IsNullOrWhiteSpace(options.Major) &&
        !string.IsNullOrWhiteSpace(options.Cohort);
}
