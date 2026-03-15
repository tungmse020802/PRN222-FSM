using StudentManagementSystem.DAL.DAO.Interfaces;
using StudentManagementSystem.DAL.Data;
using StudentManagementSystem.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace StudentManagementSystem.DAL.DAO;

public sealed class UserAccountDao(IDbContextFactory<StudentManagementDbContext> contextFactory) : IUserAccountDao
{
    public async Task<UserAccount?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.UserAccounts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower(), cancellationToken);
    }

    public async Task<UserAccount?> GetUserByIdAsync(int userAccountId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.UserAccounts.AsNoTracking().FirstOrDefaultAsync(x => x.UserAccountId == userAccountId, cancellationToken);
    }

    public async Task<IReadOnlyList<Student>> GetStudentsAsync(string? keyword = null, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var query = BuildStudentQuery(context);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var search = keyword.Trim();
            query = query.Where(x =>
                x.StudentCode.Contains(search) ||
                x.UserAccount!.FullName.Contains(search) ||
                x.UserAccount.Email.Contains(search) ||
                x.Major.Contains(search));
        }

        return await query.OrderBy(x => x.StudentCode).ToListAsync(cancellationToken);
    }

    public async Task<Student?> GetStudentByIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await BuildStudentQuery(context).FirstOrDefaultAsync(x => x.StudentId == studentId, cancellationToken);
    }

    public async Task<Student?> GetStudentByUserAccountIdAsync(int userAccountId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await BuildStudentQuery(context).FirstOrDefaultAsync(x => x.UserAccountId == userAccountId, cancellationToken);
    }

    public async Task<bool> StudentCodeExistsAsync(string studentCode, int? excludeStudentId = null, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var query = context.Students.AsNoTracking().Where(x => x.StudentCode.ToLower() == studentCode.ToLower());
        if (excludeStudentId.HasValue)
        {
            query = query.Where(x => x.StudentId != excludeStudentId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> UserEmailExistsAsync(string email, int? excludeUserAccountId = null, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var query = context.UserAccounts.AsNoTracking().Where(x => x.Email.ToLower() == email.ToLower());
        if (excludeUserAccountId.HasValue)
        {
            query = query.Where(x => x.UserAccountId != excludeUserAccountId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddStudentAsync(UserAccount userAccount, Student student, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.UserAccounts.Add(userAccount);
        await context.SaveChangesAsync(cancellationToken);
        student.UserAccountId = userAccount.UserAccountId;
        context.Students.Add(student);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateStudentAsync(Student student, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var existing = await context.Students.Include(x => x.UserAccount)
            .FirstOrDefaultAsync(x => x.StudentId == student.StudentId, cancellationToken);

        if (existing is null || existing.UserAccount is null)
        {
            return;
        }

        existing.StudentCode = student.StudentCode;
        existing.DateOfBirth = student.DateOfBirth;
        existing.Gender = student.Gender;
        existing.PhoneNumber = student.PhoneNumber;
        existing.Address = student.Address;
        existing.Major = student.Major;
        existing.Cohort = student.Cohort;
        existing.AcademicStatus = student.AcademicStatus;
        existing.IsActive = student.IsActive;
        existing.UserAccount.FullName = student.UserAccount!.FullName;
        existing.UserAccount.Email = student.UserAccount.Email;
        existing.UserAccount.IsActive = student.IsActive;
        existing.UserAccount.ModifiedDate = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(student.UserAccount.Password))
        {
            existing.UserAccount.Password = student.UserAccount.Password;
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task SetStudentActiveAsync(int studentId, bool isActive, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var existing = await context.Students.Include(x => x.UserAccount)
            .FirstOrDefaultAsync(x => x.StudentId == studentId, cancellationToken);

        if (existing is null || existing.UserAccount is null)
        {
            return;
        }

        existing.IsActive = isActive;
        existing.UserAccount.IsActive = isActive;
        existing.UserAccount.ModifiedDate = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Lecturer>> GetLecturersAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Lecturers.AsNoTracking()
            .Include(x => x.UserAccount)
            .OrderBy(x => x.LecturerCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<Lecturer?> GetLecturerByUserAccountIdAsync(int userAccountId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Lecturers.AsNoTracking()
            .Include(x => x.UserAccount)
            .FirstOrDefaultAsync(x => x.UserAccountId == userAccountId, cancellationToken);
    }

    private static IQueryable<Student> BuildStudentQuery(StudentManagementDbContext context) =>
        context.Students.AsNoTracking().Include(x => x.UserAccount);
}
