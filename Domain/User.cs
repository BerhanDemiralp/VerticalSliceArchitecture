namespace VerticalSliceArchitecture.Domain;

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;

    public string Role { get; set; } = "default";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
