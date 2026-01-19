namespace NotesApp;
public class TodoApp
{
    private readonly IUserInterface _ui;

    private List<TaskItem> _tasks = new List<TaskItem>();
    private int _nextId = 1;
    private readonly string _filePath = "tasks.txt";

    public TodoApp(IUserInterface ui)
    {
        _ui = ui;
    }

    public void Run()
    {
        LoadTasksFromFile();

        while (true)
        {
            _ui.Clear();
            _ui.WriteLine("===== Менеджер задач =====");
            _ui.WriteLine("1. Показать все задачи");
            _ui.WriteLine("2. Добавить задачу");
            _ui.WriteLine("3. Отметить задачу выполненной");
            _ui.WriteLine("4. Удалить задачу");
            _ui.WriteLine("5. Сохранить задачи в файл");
            _ui.WriteLine("0. Выход");
            _ui.Write("Ваш выбор: ");

            var choice = _ui.ReadLine();

            if (choice == "0")
            {
                SaveTasksToFile();
                break;
            }
            else if (choice == "1")
            {
                ShowTasks();
            }
            else if (choice == "2")
            {
                AddTask();
            }
            else if (choice == "3")
            {
                MarkTaskAsDone();
            }
            else if (choice == "4")
            {
                DeleteTask();
            }
            else if (choice == "5")
            {
                SaveTasksToFile();
                _ui.WaitForKey("Задачи сохранены. Нажмите любую клавишу...");
            }
            else
            {
                _ui.WaitForKey("Неизвестная команда. Нажмите любую клавишу...");
            }
        }
    }

    private void LoadTasksFromFile()
    {
        _tasks.Clear();

        if (!File.Exists(_filePath))
            return;

        var lines = File.ReadAllLines(_filePath);
        foreach (var line in lines)
        {
            var parts = line.Split(';');
            if (parts.Length < 4)
                continue;

            var task = new TaskItem
            {
                Id = int.Parse(parts[0]),
                IsDone = bool.Parse(parts[1]),
                Title = parts[2],
                Description = parts[3]
            };

            _tasks.Add(task);
            if (task.Id >= _nextId)
                _nextId = task.Id + 1;
        }
    }

    private void SaveTasksToFile()
    {
        var lines = new List<string>();
        foreach (var t in _tasks)
        {
            lines.Add($"{t.Id};{t.IsDone};{t.Title};{t.Description}");
        }

        File.WriteAllLines(_filePath, lines);
    }

    private void ShowTasks()
    {
        _ui.Clear();
        _ui.WriteLine("===== Список задач =====");

        if (_tasks.Count == 0)
        {
            _ui.WriteLine("Задач пока нет.");
        }
        else
        {
            foreach (var task in _tasks)
            {
                _ui.WriteLine(task.ToString());
            }
        }

        _ui.WriteLine();
        _ui.WaitForKey("Нажмите любую клавишу для возврата в меню...");
    }

    private void AddTask()
    {
        _ui.Clear();
        _ui.WriteLine("===== Добавление задачи =====");

        _ui.Write("Название: ");
        var title = _ui.ReadLine() ?? "";

        _ui.Write("Описание: ");
        var description = _ui.ReadLine() ?? "";

        var task = new TaskItem
        {
            Id = _nextId++,
            Title = title,
            Description = description,
            IsDone = false
        };

        _tasks.Add(task);

        _ui.WaitForKey("Задача добавлена. Нажмите любую клавишу...");
    }

    private void MarkTaskAsDone()
    {
        _ui.Clear();
        _ui.WriteLine("===== Отметить задачу выполненной =====");
        ShowTasksShort();

        _ui.Write("Введите Id задачи: ");
        var input = _ui.ReadLine();

        if (!int.TryParse(input, out var id))
        {
            _ui.WaitForKey("Некорректный Id. Нажмите любую клавишу...");
            return;
        }

        var task = _tasks.Find(t => t.Id == id);
        if (task == null)
        {
            _ui.WaitForKey("Задача не найдена. Нажмите любую клавишу...");
            return;
        }

        task.IsDone = true;
        _ui.WaitForKey("Задача отмечена выполненной. Нажмите любую клавишу...");
    }

    private void DeleteTask()
    {
        _ui.Clear();
        _ui.WriteLine("===== Удаление задачи =====");
        ShowTasksShort();

        _ui.Write("Введите Id задачи для удаления: ");
        var input = _ui.ReadLine();

        if (!int.TryParse(input, out var id))
        {
            _ui.WaitForKey("Некорректный Id. Нажмите любую клавишу...");
            return;
        }

        var task = _tasks.Find(t => t.Id == id);
        if (task == null)
        {
            _ui.WaitForKey("Задача не найдена. Нажмите любую клавишу...");
            return;
        }

        _tasks.Remove(task);
        _ui.WaitForKey("Задача удалена. Нажмите любую клавишу...");
    }

    private void ShowTasksShort()
    {
        if (_tasks.Count == 0)
        {
            _ui.WriteLine("Задач пока нет.");
            _ui.WriteLine();
            return;
        }

        foreach (var task in _tasks)
        {
            var status = task.IsDone ? "[X]" : "[ ]";
            _ui.WriteLine($"{status} {task.Id}: {task.Title}");
        }

        _ui.WriteLine();
    }
}
