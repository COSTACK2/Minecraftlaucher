using System;
using Avalonia;

namespace MinecraftLauncherBR;

/// <summary>
/// Ponto de entrada do programa. Esta classe apenas configura e inicia
/// a aplicação Avalonia — toda a lógica fica em App, nas Views, ViewModels e Services.
/// </summary>
class Program
{
    // O atributo [STAThread] é necessário no Windows para o funcionamento
    // correto de diálogos nativos (ex.: seletor de arquivos).
    [STAThread]
    public static void Main(string[] args)
        => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

    /// <summary>
    /// Configuração padrão do Avalonia: detecta a plataforma (Windows/Linux/macOS)
    /// automaticamente e habilita o modo de renderização adequado.
    /// </summary>
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
