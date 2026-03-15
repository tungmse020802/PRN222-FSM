using StudentManagementSystem.Shared.Entities;
using StudentManagementSystem.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace StudentManagementSystem.DAL.Data;

public class StudentManagementDbContext(DbContextOptions<StudentManagementDbContext> options) : DbContext(options)
{
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Lecturer> Lecturers => Set<Lecturer>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<SubjectPrerequisite> SubjectPrerequisites => Set<SubjectPrerequisite>();
    public DbSet<Semester> Semesters => Set<Semester>();
    public DbSet<CourseSection> CourseSections => Set<CourseSection>();
    public DbSet<ScheduleSlot> ScheduleSlots => Set<ScheduleSlot>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<GradeRecord> GradeRecords => Set<GradeRecord>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AIRecommendation> AIRecommendations => Set<AIRecommendation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.ToTable("UserAccount", table =>
            {
                table.HasCheckConstraint("CK_UserAccount_Role", "[Role] IN (1, 2)");
            });
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Role).HasConversion<int>();
            entity.Property(x => x.CreatedDate).HasDefaultValueSql("GETDATE()");
            entity.Property(x => x.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("Student");
            entity.HasIndex(x => x.StudentCode).IsUnique();
            entity.Property(x => x.Gender).HasConversion<int>();
            entity.Property(x => x.AcademicStatus).HasConversion<int>();
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.HasOne(x => x.UserAccount)
                .WithOne(x => x.Student)
                .HasForeignKey<Student>(x => x.UserAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Lecturer>(entity =>
        {
            entity.ToTable("Lecturer");
            entity.HasIndex(x => x.LecturerCode).IsUnique();
            entity.HasOne(x => x.UserAccount)
                .WithOne(x => x.Lecturer)
                .HasForeignKey<Lecturer>(x => x.UserAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.ToTable("Subject");
            entity.HasIndex(x => x.SubjectCode).IsUnique();
            entity.Property(x => x.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<SubjectPrerequisite>(entity =>
        {
            entity.ToTable("SubjectPrerequisite");
            entity.HasKey(x => new { x.SubjectId, x.PrerequisiteSubjectId });
            entity.HasOne(x => x.Subject)
                .WithMany(x => x.PrerequisiteLinks)
                .HasForeignKey(x => x.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PrerequisiteSubject)
                .WithMany(x => x.RequiredForLinks)
                .HasForeignKey(x => x.PrerequisiteSubjectId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Semester>(entity =>
        {
            entity.ToTable("Semester");
            entity.HasIndex(x => x.SemesterCode).IsUnique();
            entity.Property(x => x.Status).HasConversion<int>();
            entity.Property(x => x.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<CourseSection>(entity =>
        {
            entity.ToTable("CourseSection");
            entity.HasIndex(x => x.SectionCode).IsUnique();
            entity.Property(x => x.SectionName).HasMaxLength(120);
            entity.Property(x => x.IsOpen).HasDefaultValue(true);
            entity.HasOne(x => x.Subject)
                .WithMany(x => x.CourseSections)
                .HasForeignKey(x => x.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Semester)
                .WithMany(x => x.CourseSections)
                .HasForeignKey(x => x.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Lecturer)
                .WithMany(x => x.CourseSections)
                .HasForeignKey(x => x.LecturerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ScheduleSlot>(entity =>
        {
            entity.ToTable("ScheduleSlot");
            entity.Property(x => x.DayOfWeek).HasConversion<int>();
            entity.HasOne(x => x.CourseSection)
                .WithMany(x => x.ScheduleSlots)
                .HasForeignKey(x => x.CourseSectionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.ToTable("Enrollment");
            entity.HasIndex(x => new { x.StudentId, x.CourseSectionId }).IsUnique();
            entity.Property(x => x.Status).HasConversion<int>();
            entity.HasOne(x => x.Student)
                .WithMany(x => x.Enrollments)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CourseSection)
                .WithMany(x => x.Enrollments)
                .HasForeignKey(x => x.CourseSectionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GradeRecord>(entity =>
        {
            entity.ToTable("GradeRecord");
            entity.HasIndex(x => x.EnrollmentId).IsUnique();
            entity.Property(x => x.AssignmentScore).HasPrecision(5, 2);
            entity.Property(x => x.QuizScore).HasPrecision(5, 2);
            entity.Property(x => x.MidtermScore).HasPrecision(5, 2);
            entity.Property(x => x.FinalScore).HasPrecision(5, 2);
            entity.Property(x => x.TotalScore).HasPrecision(5, 2);
            entity.HasOne(x => x.Enrollment)
                .WithOne(x => x.GradeRecord)
                .HasForeignKey<GradeRecord>(x => x.EnrollmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notification");
            entity.Property(x => x.Type).HasConversion<int>();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");
            entity.HasOne(x => x.UserAccount)
                .WithMany(x => x.Notifications)
                .HasForeignKey(x => x.UserAccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AIRecommendation>(entity =>
        {
            entity.ToTable("AIRecommendation");
            entity.Property(x => x.RiskLevel).HasConversion<int>();
            entity.HasOne(x => x.Student)
                .WithMany(x => x.Recommendations)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Semester)
                .WithMany(x => x.Recommendations)
                .HasForeignKey(x => x.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
