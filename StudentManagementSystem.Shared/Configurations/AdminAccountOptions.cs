namespace StudentManagementSystem.Shared.Configurations;

public sealed class AdminAccountOptions
{
    public string Email { get; set; } = "admin@StudentManagementSystem.org";

    public string Password { get; set; } = "@@abc123@@";

    public string FullName { get; set; } = "System Administrator";
}
