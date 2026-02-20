namespace HumioAPI.Entities;

public class AnswerLocalization
{
    public long Id { get; set; }
    public long AnswerId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string AnswerText { get; set; } = string.Empty;

    public Answer Answer { get; set; } = default!;
}
