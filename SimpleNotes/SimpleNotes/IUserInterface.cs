namespace NotesApp;

public interface IUserInterface
{
    void Clear();
    void Write(string text);
    void WriteLine(string text);
    void WriteLine();
    string? ReadLine();
    void WaitForKey(string message);
}