namespace HumioAPI.Entities;

public class QuestionLocalization
{
    public long Id { get; set; }
    public long QuestionId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;

    public Question Question { get; set; } = default!;
}
