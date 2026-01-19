using NotesApp;

public class Program
{
    public static void Main()
    {
        IUserInterface ui = new ConsoleUserInterface();
        var app = new TodoApp(ui);
        app.Run();
    }
}