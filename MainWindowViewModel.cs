using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CmlLib.Core.Auth;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MinecraftLauncherBR.Models;
using MinecraftLauncherBR.Services;

namespace MinecraftLauncherBR.ViewModels;

/// <summary>
/// ViewModel da janela principal. Contém todo o estado e os comandos da tela:
/// login Microsoft, modo offline, seleção de versão, botão Jogar e importação
/// de mods / resource packs. Não conhece nenhum tipo do Avalonia diretamente
/// (exceto pela interface IFilePickerService, que é apenas uma abstração).
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly MinecraftService _minecraftService;
    private readonly AuthService _authService;
    private readonly IFilePickerService _filePickerService;

    /// <summary>Sessão Microsoft atual (null enquanto o usuário não estiver logado).</summary>
    private MSession? _microsoftSession;

    // ===================== Propriedades observáveis (ligadas à interface) =====================

    [ObservableProperty]
    private ObservableCollection<VersionListItem> versions = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PlayCommand))]
    private VersionListItem? selectedVersion;

    // Offline começa ligado por padrão: assim o launcher já funciona
    // imediatamente, sem precisar configurar o login Microsoft antes.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOfflineFieldsEnabled))]
    [NotifyCanExecuteChangedFor(nameof(PlayCommand))]
    private bool isOfflineMode = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PlayCommand))]
    private string offlineUsername = "Jogador";

    [ObservableProperty]
    private bool isLoggedInMicrosoft;

    [ObservableProperty]
    private string? microsoftAccountLabel;

    [ObservableProperty]
    private string logText = string.Empty;

    [ObservableProperty]
    private double progress;

    [ObservableProperty]
    private string statusMessage = "Pronto.";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PlayCommand))]
    [NotifyCanExecuteChangedFor(nameof(LoginMicrosoftCommand))]
    [NotifyCanExecuteChangedFor(nameof(ImportModCommand))]
    [NotifyCanExecuteChangedFor(nameof(ImportResourcePackCommand))]
    private bool isBusy;

    /// <summary>Usado no XAML para habilitar/desabilitar o campo de nome de usuário offline.</summary>
    public bool IsOfflineFieldsEnabled => IsOfflineMode;

    // ===================== Construtor =====================

    public MainWindowViewModel(MinecraftService minecraftService, AuthService authService, IFilePickerService filePickerService)
    {
        _minecraftService = minecraftService;
        _authService = authService;
        _filePickerService = filePickerService;

        _minecraftService.StatusChanged += (_, msg) => StatusMessage = msg;
        _minecraftService.ProgressChanged += (_, pct) => Progress = pct;

        Log($"Pasta do jogo: {_minecraftService.GamePath.BasePath}");

        _ = CarregarVersoesAsync();
    }

    // ===================== Utilitário de log =====================

    private void Log(string message)
    {
        var horario = DateTime.Now.ToString("HH:mm:ss");
        LogText += $"[{horario}] {message}\n";
    }

    // ===================== Carregar versões =====================

    private async System.Threading.Tasks.Task CarregarVersoesAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Carregando lista de versões...";
            Log("Buscando versões oficiais nos servidores da Mojang...");

            var lista = await _minecraftService.GetVersionsAsync();
            Versions = new ObservableCollection<VersionListItem>(lista);
            SelectedVersion = Versions.FirstOrDefault(v => v.Type == "Release") ?? Versions.FirstOrDefault();

            Log($"{Versions.Count} versões encontradas.");
        }
        catch (Exception ex)
        {
            Log($"Não foi possível carregar as versões: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            StatusMessage = "Pronto.";
        }
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task RefreshVersionsAsync() => await CarregarVersoesAsync();

    // ===================== Login Microsoft =====================

    [RelayCommand(CanExecute = nameof(CanLoginMicrosoft))]
    private async System.Threading.Tasks.Task LoginMicrosoftAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Conectando com a Microsoft...";
            Log("Iniciando login com a Microsoft...");

            _microsoftSession = await _authService.LoginMicrosoftAsync(mensagem =>
            {
                // Mensagem do tipo: "Para entrar, acesse https://microsoft.com/link e digite o código ABC-DEF"
                Log(mensagem);
                StatusMessage = "Aguardando autorização no navegador...";
            });

            IsLoggedInMicrosoft = true;
            MicrosoftAccountLabel = _microsoftSession.Username;
            Log($"Login realizado com sucesso como {_microsoftSession.Username}.");
        }
        catch (Exception ex)
        {
            IsLoggedInMicrosoft = false;
            Log($"Falha no login com a Microsoft: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            StatusMessage = "Pronto.";
        }
    }

    private bool CanLoginMicrosoft() => !IsBusy;

    // ===================== Jogar =====================

    [RelayCommand(CanExecute = nameof(CanPlay))]
    private async System.Threading.Tasks.Task PlayAsync()
    {
        if (SelectedVersion is null)
        {
            Log("Selecione uma versão do Minecraft antes de jogar.");
            return;
        }

        MSession session;
        if (IsOfflineMode)
        {
            var nome = string.IsNullOrWhiteSpace(OfflineUsername) ? "Jogador" : OfflineUsername.Trim();
            session = _authService.CreateOfflineSession(nome);
        }
        else if (_microsoftSession is not null)
        {
            session = _microsoftSession;
        }
        else
        {
            Log("Faça login com a Microsoft ou ative o Modo Offline para jogar.");
            return;
        }

        try
        {
            IsBusy = true;

            Log($"Preparando a versão {SelectedVersion.Name}...");
            StatusMessage = "Instalando/verificando arquivos...";
            await _minecraftService.InstallAsync(SelectedVersion.Name);

            Log("Arquivos prontos. Iniciando o Minecraft...");
            StatusMessage = "Iniciando o jogo...";
            var process = await _minecraftService.BuildProcessAsync(
                SelectedVersion.Name, session, LauncherConfig.DefaultMaximumRamMb);

            process.Start();
            Log("Minecraft iniciado. Bom jogo!");
        }
        catch (Exception ex)
        {
            Log($"Erro ao iniciar o Minecraft: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            StatusMessage = "Pronto.";
        }
    }

    private bool CanPlay()
        => !IsBusy
           && SelectedVersion is not null
           && (!IsOfflineMode || !string.IsNullOrWhiteSpace(OfflineUsername));

    // ===================== Importar mod / resource pack =====================

    [RelayCommand(CanExecute = nameof(CanImport))]
    private async System.Threading.Tasks.Task ImportModAsync()
    {
        var arquivo = await _filePickerService.PickJarFileAsync();
        if (arquivo is null) return;

        try
        {
            var destino = FileService.ImportMod(_minecraftService.GamePath, arquivo);
            Log($"Mod importado: {Path.GetFileName(destino)}");
        }
        catch (Exception ex)
        {
            Log($"Erro ao importar mod: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanImport))]
    private async System.Threading.Tasks.Task ImportResourcePackAsync()
    {
        var arquivo = await _filePickerService.PickResourcePackFileAsync();
        if (arquivo is null) return;

        try
        {
            var destino = FileService.ImportResourcePack(_minecraftService.GamePath, arquivo);
            Log($"Resource pack importado: {Path.GetFileName(destino)}");
        }
        catch (Exception ex)
        {
            Log($"Erro ao importar resource pack: {ex.Message}");
        }
    }

    private bool CanImport() => !IsBusy;
}
