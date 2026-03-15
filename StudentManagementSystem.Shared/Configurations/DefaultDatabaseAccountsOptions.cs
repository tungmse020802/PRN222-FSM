using StudentManagementSystem.Shared.Enums;

namespace StudentManagementSystem.Shared.Configurations;

public sealed class DefaultDatabaseAccountsOptions
{
    public List<DefaultLecturerAccountOptions> Lecturers { get; set; } =
    [
        new()
        {
            FullName = "Dr. Nguyen Minh",
            Email = "lecturer1@student.local",
            Password = "123456",
            LecturerCode = "LEC001",
            Department = "Computer Science",
            OfficeRoom = "A-203",
            IsActive = true
        },
        new()
        {
            FullName = "Ms. Tran Hoa",
            Email = "lecturer2@student.local",
            Password = "123456",
            LecturerCode = "LEC002",
            Department = "Information Systems",
            OfficeRoom = "B-101",
            IsActive = true
        }
    ];

    public List<DefaultStudentAccountOptions> Students { get; set; } =
    [
        new()
        {
            FullName = "Le Thi An",
            Email = "student1@student.local",
            Password = "123456",
            StudentCode = "SE170001",
            DateOfBirth = new DateTime(2004, 5, 12),
            Gender = Gender.Female,
            PhoneNumber = "0901000001",
            Address = "Da Nang",
            Major = "Software Engineering",
            Cohort = "K17",
            AcademicStatus = AcademicStatus.Active,
            IsActive = true
        },
        new()
        {
            FullName = "Pham Quoc Bao",
            Email = "student2@student.local",
            Password = "123456",
            StudentCode = "SE170002",
            DateOfBirth = new DateTime(2004, 8, 2),
            Gender = Gender.Male,
            PhoneNumber = "0901000002",
            Address = "Hue",
            Major = "Software Engineering",
            Cohort = "K17",
            AcademicStatus = AcademicStatus.Active,
            IsActive = true
        },
        new()
        {
            FullName = "Vo Minh Chau",
            Email = "student3@student.local",
            Password = "123456",
            StudentCode = "SE170003",
            DateOfBirth = new DateTime(2004, 3, 27),
            Gender = Gender.Other,
            PhoneNumber = "0901000003",
            Address = "Quang Nam",
            Major = "Information Systems",
            Cohort = "K17",
            AcademicStatus = AcademicStatus.Probation,
            IsActive = true
        }
    ];
}

public sealed class DefaultLecturerAccountOptions
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string LecturerCode { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public string? OfficeRoom { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class DefaultStudentAccountOptions
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string StudentCode { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public Gender Gender { get; set; } = Gender.Male;

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public string Major { get; set; } = string.Empty;

    public string Cohort { get; set; } = string.Empty;

    public AcademicStatus AcademicStatus { get; set; } = AcademicStatus.Active;

    public bool IsActive { get; set; } = true;
}
