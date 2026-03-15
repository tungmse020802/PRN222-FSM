using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StudentManagementSystem.DAL.Data;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<StudentManagementDbContext>
{
    public StudentManagementDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<StudentManagementDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=StudentManagementSystem;User Id=sa;Password=123456789a@;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=True");

        return new StudentManagementDbContext(optionsBuilder.Options);
    }
}
