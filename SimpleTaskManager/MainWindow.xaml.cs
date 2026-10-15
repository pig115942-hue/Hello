using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SimpleTaskManager;

public partial class MainWindow : Window
{
    private readonly TaskStore _store = new();
    private readonly List<TaskItem> _tasks;

    public MainWindow()
    {
        InitializeComponent();
        _tasks = _store.Load();
        RenderTasks();
    }

    private void Add_Click(object sender, RoutedEventArgs e) => AddTask();

    private void TaskInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) AddTask();
    }

    private void AddTask()
    {
        var title = TaskInput.Text.Trim();
        if (title.Length == 0) return;
        _tasks.Add(new TaskItem { Title = title });
        _store.Save(_tasks);
        TaskInput.Clear();
        RenderTasks();
        TaskInput.Focus();
    }

    private void RenderTasks()
    {
        TaskPanel.Children.Clear();
        var active = _tasks.Where(t => !t.IsCompleted).ToList();
        var completed = _tasks.Where(t => t.IsCompleted).ToList();

        AddSection("TO DO", active);
        if (completed.Count > 0)
        {
            var divider = new Border { Height = 1, Background = new SolidColorBrush(Color.FromRgb(225, 227, 230)), Margin = new Thickness(0, 18, 0, 18) };
            TaskPanel.Children.Add(divider);
            AddSection("COMPLETED", completed);
        }

        StatusText.Text = _tasks.Count == 0 ? "No tasks yet." : $"{active.Count} to do · {completed.Count} completed";
    }

    private void AddSection(string heading, List<TaskItem> tasks)
    {
        TaskPanel.Children.Add(new TextBlock { Text = heading, FontSize = 12, FontWeight = FontWeights.SemiBold, Foreground = Brushes.Gray, Margin = new Thickness(4, 0, 0, 8) });
        if (tasks.Count == 0)
        {
            TaskPanel.Children.Add(new TextBlock { Text = "Nothing here.", Foreground = Brushes.Gray, Margin = new Thickness(4, 4, 0, 4) });
            return;
        }

        foreach (var task in tasks) TaskPanel.Children.Add(CreateTaskRow(task));
    }

    private FrameworkElement CreateTaskRow(TaskItem task)
    {
        var card = new Border { Background = Brushes.White, CornerRadius = new CornerRadius(9), Padding = new Thickness(12, 10, 8, 10), Margin = new Thickness(0, 0, 0, 8) };
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var check = new CheckBox { IsChecked = task.IsCompleted, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) };
        check.Checked += (_, _) => SetCompleted(task, true);
        check.Unchecked += (_, _) => SetCompleted(task, false);
        Grid.SetColumn(check, 0);

        var text = new TextBlock { Text = task.Title, FontSize = 15, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap, Foreground = task.IsCompleted ? Brushes.Gray : Brushes.Black, TextDecorations = task.IsCompleted ? TextDecorations.Strikethrough : null };
        Grid.SetColumn(text, 1);

        var edit = new Button { Content = "Edit", Padding = new Thickness(10, 4, 10, 4), Margin = new Thickness(8, 0, 4, 0) };
        edit.Click += (_, _) => EditTask(task);
        Grid.SetColumn(edit, 2);

        var delete = new Button { Content = "Delete", Padding = new Thickness(10, 4, 10, 4) };
        delete.Click += (_, _) => { _tasks.Remove(task); _store.Save(_tasks); RenderTasks(); };
        Grid.SetColumn(delete, 3);

        grid.Children.Add(check); grid.Children.Add(text); grid.Children.Add(edit); grid.Children.Add(delete);
        card.Child = grid;
        return card;
    }

    private void SetCompleted(TaskItem task, bool completed)
    {
        if (task.IsCompleted == completed) return;
        task.IsCompleted = completed;
        _store.Save(_tasks);
        RenderTasks();
    }

    private void EditTask(TaskItem task)
    {
        var dialog = new Window { Title = "Edit task", Width = 430, Height = 170, Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner, ResizeMode = ResizeMode.NoResize };
        var panel = new StackPanel { Margin = new Thickness(18) };
        var input = new TextBox { Text = task.Title, FontSize = 15, Padding = new Thickness(8), Margin = new Thickness(0, 0, 0, 14) };
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var cancel = new Button { Content = "Cancel", Padding = new Thickness(14, 7, 14, 7), Margin = new Thickness(0, 0, 8, 0) };
        var save = new Button { Content = "Save", Padding = new Thickness(14, 7, 14, 7) };
        cancel.Click += (_, _) => dialog.Close();
        save.Click += (_, _) => { var value = input.Text.Trim(); if (value.Length > 0) { task.Title = value; _store.Save(_tasks); RenderTasks(); dialog.Close(); } };
        buttons.Children.Add(cancel); buttons.Children.Add(save);
        panel.Children.Add(input); panel.Children.Add(buttons); dialog.Content = panel;
        dialog.Loaded += (_, _) => { input.Focus(); input.SelectAll(); };
        dialog.ShowDialog();
    }
}
