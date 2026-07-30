namespace MinecraftLauncherBR.Models;

/// <summary>
/// Configurações fixas do launcher.
///
/// IMPORTANTE: o "MicrosoftClientId" é obrigatório apenas para o botão
/// "Entrar com a Microsoft" funcionar. O Modo Offline funciona normalmente
/// mesmo sem configurar isso.
///
/// Veja o passo a passo completo em README.md, seção "Configurando o login Microsoft".
/// </summary>
public static class LauncherConfig
{
    /// <summary>
    /// Application (Client) ID do aplicativo registrado no Azure Portal.
    /// Troque o valor abaixo pelo seu Client ID real.
    /// </summary>
    public const string MicrosoftClientId = "COLOQUE_SEU_CLIENT_ID_AQUI";

    /// <summary>Nome exibido para a Mojang/Xbox como "launcher de origem" do lançamento.</summary>
    public const string LauncherName = "MinecraftLauncherBR";

    public const string LauncherVersion = "1.0.0";

    /// <summary>Memória RAM máxima padrão (MB) usada para iniciar o jogo.</summary>
    public const int DefaultMaximumRamMb = 4096;

    /// <summary>Nome do arquivo onde as contas Microsoft logadas ficam salvas (login silencioso).</summary>
    public const string AccountsFileName = "cml_accounts.json";
}
