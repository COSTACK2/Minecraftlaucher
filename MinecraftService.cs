using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.ProcessBuilder;
using MinecraftLauncherBR.Models;

namespace MinecraftLauncherBR.Services;

/// <summary>
/// Encapsula toda a comunicação com o CmlLib.Core: listagem de versões oficiais,
/// instalação de arquivos (jar da versão, bibliotecas, assets, natives e o
/// próprio Java, que é baixado automaticamente) e criação do processo do jogo
/// (equivalente a rodar o "javaw" com todos os argumentos corretos).
/// </summary>
public class MinecraftService
{
    private readonly MinecraftLauncher _launcher;

    /// <summary>Caminho da pasta ".minecraft" usada por este launcher.</summary>
    public MinecraftPath GamePath { get; }

    /// <summary>
    /// Disparado com uma linha curta de status (nome do arquivo/etapa atual).
    /// Pode ser chamado centenas de vezes por segundo durante a instalação —
    /// use para atualizar um texto de status, não para um log linha a linha.
    /// </summary>
    public event EventHandler<string>? StatusChanged;

    /// <summary>Disparado poucas vezes por segundo com o progresso em bytes (0 a 100).</summary>
    public event EventHandler<double>? ProgressChanged;

    public MinecraftService(string? gameDirectory = null)
    {
        GamePath = string.IsNullOrWhiteSpace(gameDirectory)
            ? new MinecraftPath()
            : new MinecraftPath(gameDirectory);

        // Garante que todas as pastas necessárias existem antes de qualquer operação.
        FileService.EnsureFolders(GamePath);

        _launcher = new MinecraftLauncher(GamePath);

        // Evento chamado a cada arquivo processado (biblioteca, asset, native, etc.).
        _launcher.FileProgressChanged += (_, e) =>
            StatusChanged?.Invoke(this, $"{e.EventType}: {e.Name} ({e.ProgressedTasks}/{e.TotalTasks})");

        // Evento chamado com o progresso em bytes (ideal para a barra de progresso).
        _launcher.ByteProgressChanged += (_, e) =>
        {
            var percent = e.TotalBytes <= 0 ? 0 : (double)e.ProgressedBytes / e.TotalBytes * 100.0;
            ProgressChanged?.Invoke(this, percent);
        };
    }

    /// <summary>
    /// Busca a lista completa de versões oficiais (releases e snapshots) diretamente
    /// dos servidores da Mojang, ordenadas da mais recente para a mais antiga.
    /// </summary>
    public async Task<List<VersionListItem>> GetVersionsAsync()
    {
        var versions = await _launcher.GetAllVersionsAsync();

        return versions
            .OrderByDescending(v => v.ReleaseTime)
            .Select(v => new VersionListItem(v.Name, v.GetVersionType().ToString()))
            .ToList();
    }

    /// <summary>
    /// Garante que todos os arquivos da versão escolhida estão presentes
    /// (baixa o que estiver faltando: jar, bibliotecas, assets, natives e o Java necessário).
    /// </summary>
    public Task InstallAsync(string versionName, CancellationToken cancellationToken = default)
        => _launcher.InstallAsync(versionName, cancellationToken);

    /// <summary>
    /// Monta o processo do jogo (o equivalente a montar a linha de comando do "javaw"
    /// com todos os argumentos) pronto para ser iniciado com "process.Start()".
    /// </summary>
    public Task<Process> BuildProcessAsync(string versionName, MSession session, int maximumRamMb = 4096)
    {
        var options = new MLaunchOption
        {
            Session = session,
            MaximumRamMb = maximumRamMb,
            GameLauncherName = LauncherConfig.LauncherName,
            GameLauncherVersion = LauncherConfig.LauncherVersion,
        };

        return _launcher.BuildProcessAsync(versionName, options);
    }
}
