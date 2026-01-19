namespace HumioAPI.Services;

public sealed class GoogleAuthOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string TokenUri { get; set; } = "https://oauth2.googleapis.com/token";
}
