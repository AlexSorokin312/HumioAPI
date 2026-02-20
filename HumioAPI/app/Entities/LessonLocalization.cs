namespace HumioAPI.Entities;

public class LessonLocalization
{
    public long Id { get; set; }
    public long LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? TextContent { get; set; }
    public string? AudioLink { get; set; }
    public string LanguageCode { get; set; } = string.Empty;

    public Lesson Lesson { get; set; } = default!;
    public ICollection<LessonLink> Links { get; } = new List<LessonLink>();
}
