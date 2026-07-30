using System;
using System.Threading;
using System.Threading.Tasks;
using CmlLib.Core.Auth;
using CmlLib.Core.Auth.Microsoft;
using MinecraftLauncherBR.Models;
using XboxAuthNet.Game;
using XboxAuthNet.Game.Msal;

namespace MinecraftLauncherBR.Services;

/// <summary>
/// Serviço responsável pela autenticação do jogador: login real com conta
/// Microsoft/Xbox (necessário para jogar online com uma conta legítima do
/// Minecraft) e criação de sessões offline (para uso local/LAN/servidores
/// próprios que não exigem verificação online).
/// </summary>
public class AuthService
{
    private readonly string _clientId;
    private readonly string _accountsFilePath;

    /// <param name="clientId">Client ID do app registrado no Azure Portal (ver README.md).</param>
    /// <param name="accountsFilePath">Arquivo onde a conta logada fica salva para login silencioso.</param>
    public AuthService(string clientId, string accountsFilePath)
    {
        _clientId = clientId;
        _accountsFilePath = accountsFilePath;
    }

    /// <summary>
    /// Cria uma sessão offline local, apenas com um nome de usuário.
    /// Funciona para jogar sozinho, em LAN ou em servidores configurados
    /// no modo offline (que não exigem conta Microsoft). Não funciona em
    /// servidores no modo "online" (que verificam a conta pela Mojang/Xbox).
    /// </summary>
    public MSession CreateOfflineSession(string username)
        => MSession.CreateOfflineSession(username);

    /// <summary>
    /// Faz login com uma conta Microsoft/Xbox real usando o fluxo "Device Code":
    /// o launcher mostra um código e um link; o jogador abre o link em
    /// qualquer navegador (no PC, celular, etc.) e digita o código para autorizar.
    /// Esse fluxo funciona em Windows, Linux e macOS sem precisar de navegador embutido.
    ///
    /// Primeiro tenta reaproveitar a última conta salva (login silencioso).
    /// Se não houver conta salva ou o login expirou, inicia o login interativo.
    /// </summary>
    /// <param name="onDeviceCodeMessage">
    /// Chamado com a mensagem contendo a URL + código que o usuário precisa acessar
    /// (mostre essa mensagem na tela/log do launcher).
    /// </param>
    public async Task<MSession> LoginMicrosoftAsync(Action<string> onDeviceCodeMessage, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_clientId) || _clientId == LauncherConfig.MicrosoftClientId)
        {
            throw new InvalidOperationException(
                "Configure o Client ID do Azure antes de entrar com a Microsoft " +
                "(veja README.md, seção \"Configurando o login Microsoft\").");
        }

        var app = await MsalClientHelper.BuildApplicationWithCache(_clientId);

        var loginHandler = new JELoginHandlerBuilder()
            .WithAccountManager(_accountsFilePath)
            .Build();

        // 1) Tenta login silencioso com a conta mais recente salva.
        try
        {
            var silentAuthenticator = loginHandler.CreateAuthenticatorWithDefaultAccount(cancellationToken);
            silentAuthenticator.AddMsalOAuth(app, msal => msal.Silent());
            silentAuthenticator.AddXboxAuthForJE(xbox => xbox.Full());
            silentAuthenticator.AddJEAuthenticator();

            return await silentAuthenticator.ExecuteForLauncherAsync();
        }
        catch
        {
            // 2) Sem conta salva (ou expirada): login interativo via Device Code.
            var interactiveAuthenticator = loginHandler.CreateAuthenticatorWithNewAccount(cancellationToken);
            interactiveAuthenticator.AddMsalOAuth(app, msal => msal.DeviceCode(deviceCodeResult =>
            {
                onDeviceCodeMessage(deviceCodeResult.Message);
                return Task.CompletedTask;
            }));
            interactiveAuthenticator.AddXboxAuthForJE(xbox => xbox.Full());
            interactiveAuthenticator.AddJEAuthenticator();

            return await interactiveAuthenticator.ExecuteForLauncherAsync();
        }
    }
}
