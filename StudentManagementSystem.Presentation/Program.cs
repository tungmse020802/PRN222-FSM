using StudentManagementSystem.BLL.Interfaces;
using StudentManagementSystem.BLL.Services;
using StudentManagementSystem.DAL.DAO;
using StudentManagementSystem.DAL.DAO.Interfaces;
using StudentManagementSystem.DAL.Data;
using StudentManagementSystem.DAL.Repositories;
using StudentManagementSystem.DAL.Repositories.Interfaces;
using StudentManagementSystem.DAL.Seed;
using StudentManagementSystem.Presentation.Hubs;
using StudentManagementSystem.Shared.Configurations;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var adminAccountOptions = builder.Configuration
    .GetSection("AdminAccount")
    .Get<AdminAccountOptions>() ?? new AdminAccountOptions();
var defaultDatabaseAccountsOptions = builder.Configuration
    .GetSection("DefaultDatabaseAccounts")
    .Get<DefaultDatabaseAccountsOptions>() ?? new DefaultDatabaseAccountsOptions();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

builder.Services.AddSingleton(adminAccountOptions);
builder.Services.AddSingleton(defaultDatabaseAccountsOptions);
builder.Services.AddDbContextFactory<StudentManagementDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/";
        options.SlidingExpiration = true;
        options.Cookie.Name = "StudentManagement.Auth";
    });

builder.Services.AddAuthorization();
builder.Services.AddSignalR();
builder.Services.AddRazorPages();

builder.Services.AddSingleton<IUserAccountDao, UserAccountDao>();
builder.Services.AddSingleton<IAcademicDao, AcademicDao>();
builder.Services.AddSingleton<IEnrollmentDao, EnrollmentDao>();
builder.Services.AddSingleton<INotificationDao, NotificationDao>();
builder.Services.AddSingleton<IRecommendationDao, RecommendationDao>();

builder.Services.AddScoped<IUserAccountRepository, UserAccountRepository>();
builder.Services.AddScoped<IAcademicRepository, AcademicRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IRecommendationRepository, RecommendationRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IAcademicService, AcademicService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapHub<NotificationHub>("/notificationHub");
app.MapRazorPages().WithStaticAssets();

await InitializeDatabaseAsync(app.Services, app.Logger);

app.Run();

static async Task InitializeDatabaseAsync(IServiceProvider services, ILogger logger)
{
    try
    {
        using var scope = services.CreateScope();
        var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<StudentManagementDbContext>>();
        var defaultDatabaseAccountsOptions = scope.ServiceProvider.GetRequiredService<DefaultDatabaseAccountsOptions>();
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        await dbContext.Database.MigrateAsync();
        await DbSeeder.SeedAsync(dbContext, defaultDatabaseAccountsOptions);
    }
    catch (Exception exception)
    {
        logger.LogWarning(exception, "Database initialization was skipped. Verify SQL Server is running at localhost:1433.");
    }
}
