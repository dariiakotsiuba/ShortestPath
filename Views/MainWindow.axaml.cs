using System;
using Avalonia.Controls;
using ReactiveUI;
using ShortestPath.ViewModels;
using System.Reactive;

namespace ShortestPath.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var vm = new MainViewModel();

        vm.RunCommand = ReactiveCommand.Create(vm.Run);

        vm.SaveCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var dialog = new SaveFileDialog
            {
                Title = "Save Results",
                DefaultExtension = "txt",
                Filters = new()
                {
                    new FileDialogFilter { Name = "Text files", Extensions = { "txt" } },
                    new FileDialogFilter { Name = "All files", Extensions = { "*" } }
                },
                InitialFileName = $"shortest_path_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
            };

            var path = await dialog.ShowAsync(this);
            if (!string.IsNullOrEmpty(path))
                await vm.SaveResultsAsync(path);
        });

        DataContext = vm;
    }
}