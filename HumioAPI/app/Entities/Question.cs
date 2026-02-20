namespace HumioAPI.Entities;

public class Question
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public long LessonId { get; set; }

    public Lesson Lesson { get; set; } = default!;
    public ICollection<Answer> Answers { get; } = new List<Answer>();
    public ICollection<QuestionLocalization> Localizations { get; } = new List<QuestionLocalization>();
}
