using System.ComponentModel;
using System.IO;
using Avalonia.Controls;
using MinecraftLauncherBR.Models;
using MinecraftLauncherBR.Services;
using MinecraftLauncherBR.ViewModels;

namespace MinecraftLauncherBR.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // ===== Composição manual das dependências (sem contêiner de DI, para manter simples) =====
        var minecraftService = new MinecraftService();

        var authService = new AuthService(
            LauncherConfig.MicrosoftClientId,
            Path.Combine(minecraftService.GamePath.BasePath, LauncherConfig.AccountsFileName));

        var filePickerService = new AvaloniaFilePickerService(this);

        var viewModel = new MainWindowViewModel(minecraftService, authService, filePickerService);
        DataContext = viewModel;

        // Rola o log automaticamente para a última linha sempre que uma nova mensagem é adicionada.
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.LogText))
        {
            LogTextBox.CaretIndex = LogTextBox.Text?.Length ?? 0;
        }
    }
}
