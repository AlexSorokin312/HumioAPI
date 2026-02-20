namespace HumioAPI.Entities;

public class LessonLink
{
    public long Id { get; set; }
    public long LocalizationId { get; set; }
    public int Pos { get; set; }

    public LessonLocalization Localization { get; set; } = default!;
}
