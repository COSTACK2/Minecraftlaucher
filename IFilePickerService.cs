using System.Threading.Tasks;

namespace MinecraftLauncherBR.Services;

/// <summary>
/// Abstração para abrir seletores de arquivo nativos.
/// Existe para que a ViewModel não precise conhecer tipos do Avalonia
/// diretamente (mantendo a separação MVVM). A implementação real
/// (que usa a janela/StorageProvider do Avalonia) fica em Views/AvaloniaFilePickerService.cs.
/// </summary>
public interface IFilePickerService
{
    /// <summary>Abre o seletor de arquivos filtrando por .jar. Retorna null se o usuário cancelar.</summary>
    Task<string?> PickJarFileAsync();

    /// <summary>Abre o seletor de arquivos filtrando por .zip. Retorna null se o usuário cancelar.</summary>
    Task<string?> PickResourcePackFileAsync();
}
