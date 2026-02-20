namespace HumioAPI.Entities;

public class Answer
{
    public long Id { get; set; }
    public long QuestionId { get; set; }

    public Question Question { get; set; } = default!;
    public ICollection<AnswerLocalization> Localizations { get; } = new List<AnswerLocalization>();
}
