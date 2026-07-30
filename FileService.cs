using System.IO;
using CmlLib.Core;

namespace MinecraftLauncherBR.Services;

/// <summary>
/// Serviço responsável por toda a manipulação de arquivos e pastas do launcher:
/// criar a estrutura de diretórios necessária e copiar mods/resource packs
/// importados pelo usuário para os locais corretos.
///
/// Observação: as pastas "mods" e "resourcepacks" são compartilhadas entre
/// todas as versões instaladas na mesma pasta do jogo — isso é exatamente
/// como o Minecraft (e o Forge/Fabric) funcionam de verdade. Se um dia você
/// quiser pastas separadas por versão/perfil, basta criar uma MinecraftPath
/// diferente para cada perfil (uma pasta de jogo própria por perfil).
/// </summary>
public static class FileService
{
    /// <summary>Cria toda a estrutura de pastas necessária para o launcher funcionar.</summary>
    public static void EnsureFolders(MinecraftPath path)
    {
        Directory.CreateDirectory(path.BasePath);
        Directory.CreateDirectory(path.Versions);
        Directory.CreateDirectory(path.Assets);
        Directory.CreateDirectory(path.Runtime);
        Directory.CreateDirectory(GetModsFolder(path));
        Directory.CreateDirectory(GetResourcePacksFolder(path));
    }

    public static string GetModsFolder(MinecraftPath path)
        => Path.Combine(path.BasePath, "mods");

    public static string GetResourcePacksFolder(MinecraftPath path)
        => Path.Combine(path.BasePath, "resourcepacks");

    /// <summary>
    /// Copia um arquivo .jar de mod para a pasta "mods" da instalação atual.
    /// Retorna o caminho final do arquivo copiado.
    /// </summary>
    /// <remarks>
    /// Um mod .jar sozinho não faz nada em uma instalação vanilla: ele precisa
    /// de um mod loader (Forge, Fabric, NeoForge, etc.) instalado na versão
    /// selecionada para funcionar. Este launcher apenas copia o arquivo —
    /// a instalação do mod loader em si está fora do escopo deste projeto.
    /// </remarks>
    public static string ImportMod(MinecraftPath path, string sourceFilePath)
    {
        var destinationFolder = GetModsFolder(path);
        Directory.CreateDirectory(destinationFolder);

        var destinationPath = Path.Combine(destinationFolder, Path.GetFileName(sourceFilePath));
        File.Copy(sourceFilePath, destinationPath, overwrite: true);

        return destinationPath;
    }

    /// <summary>Copia um arquivo .zip de resource pack para a pasta "resourcepacks".</summary>
    public static string ImportResourcePack(MinecraftPath path, string sourceFilePath)
    {
        var destinationFolder = GetResourcePacksFolder(path);
        Directory.CreateDirectory(destinationFolder);

        var destinationPath = Path.Combine(destinationFolder, Path.GetFileName(sourceFilePath));
        File.Copy(sourceFilePath, destinationPath, overwrite: true);

        return destinationPath;
    }
}
