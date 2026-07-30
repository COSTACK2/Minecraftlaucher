namespace MinecraftLauncherBR.Models;

/// <summary>
/// Representa uma versão do Minecraft para exibição no ComboBox.
/// É apenas um "empacotador" (wrapper) simples em cima do que o
/// CmlLib.Core retorna, usado somente para exibição na interface.
/// </summary>
public class VersionListItem
{
    /// <summary>Identificador real da versão usado pelo CmlLib.Core (ex.: "1.21.4").</summary>
    public string Name { get; }

    /// <summary>Tipo da versão: Release, Snapshot, OldBeta, OldAlpha, etc.</summary>
    public string Type { get; }

    /// <summary>Texto amigável mostrado no ComboBox.</summary>
    public string DisplayName => $"{Name}   ·   {Type}";

    public VersionListItem(string name, string type)
    {
        Name = name;
        Type = type;
    }

    public override string ToString() => DisplayName;
}
