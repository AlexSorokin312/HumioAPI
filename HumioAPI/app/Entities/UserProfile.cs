namespace HumioAPI.Entities;

public class UserProfile
{
    public long UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Gender { get; set; }

    public ApplicationUser User { get; set; } = default!;
}
