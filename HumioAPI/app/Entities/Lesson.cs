namespace HumioAPI.Entities;

public class Lesson
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long ModuleId { get; set; }
    public int OrderIndex { get; set; }
    public int DurationSeconds { get; set; }
    public bool IsPublished { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsFree { get; set; }

    public Module Module { get; set; } = default!;
    public ICollection<Question> Questions { get; } = new List<Question>();
    public ICollection<LessonLocalization> Localizations { get; } = new List<LessonLocalization>();
}
