namespace NotesApp;
public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsDone { get; set; }

    public override string ToString()
    {
        var status = IsDone ? "[X]" : "[ ]";
        return $"{status} {Id}: {Title} - {Description}";
    }
}