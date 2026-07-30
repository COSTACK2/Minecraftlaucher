using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using MinecraftLauncherBR.Services;

namespace MinecraftLauncherBR.Views;

/// <summary>
/// Implementação real de IFilePickerService, usando o StorageProvider do
/// Avalonia (API multiplataforma de diálogos de arquivo — funciona em
/// Windows, Linux e macOS).
/// </summary>
public class AvaloniaFilePickerService : IFilePickerService
{
    private readonly Window _owner;

    public AvaloniaFilePickerService(Window owner)
    {
        _owner = owner;
    }

    public Task<string?> PickJarFileAsync()
        => PickSingleFileAsync(
            "Selecione o arquivo do mod (.jar)",
            new FilePickerFileType("Arquivo de mod (*.jar)") { Patterns = new[] { "*.jar" } });

    public Task<string?> PickResourcePackFileAsync()
        => PickSingleFileAsync(
            "Selecione o resource pack (.zip)",
            new FilePickerFileType("Resource pack (*.zip)") { Patterns = new[] { "*.zip" } });

    private async Task<string?> PickSingleFileAsync(string title, FilePickerFileType fileType)
    {
        var topLevel = TopLevel.GetTopLevel(_owner);
        if (topLevel?.StorageProvider is null)
            return null;

        var resultado = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType> { fileType, FilePickerFileTypes.All }
        });

        var arquivo = resultado.Count > 0 ? resultado[0] : null;
        return arquivo?.TryGetLocalPath();
    }
}
