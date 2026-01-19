namespace NotesApp;

public class ConsoleUserInterface : IUserInterface
{
    public void Clear() => Console.Clear();

    public void Write(string text) => Console.Write(text);

    public void WriteLine(string text) => Console.WriteLine(text);

    public void WriteLine() => Console.WriteLine();

    public string? ReadLine() => Console.ReadLine();

    public void WaitForKey(string message)
    {
        Console.WriteLine(message);
        Console.ReadKey();
    }
}