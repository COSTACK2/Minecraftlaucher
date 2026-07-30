# MinecraftLauncherBR

Launcher de Minecraft moderno, feito em C# com **Avalonia UI** (interface) e
**CmlLib.Core** (download/instalação/execução do jogo oficial). Inspirado no
Brasil Launcher, em padrão **MVVM**.

> Este projeto só usa APIs oficiais da Mojang/Microsoft (via CmlLib.Core) e
> não contém nada relacionado a versões piratas/crackeadas. Para jogar online
> você precisa de uma conta Microsoft com o Minecraft comprado legitimamente.

---

## 1. O que já está pronto

- Login real com conta Microsoft/Xbox (fluxo "Device Code", funciona em
  Windows/Linux/macOS sem navegador embutido)
- Modo Offline (nome de usuário livre, para jogo local/LAN/servidores próprios)
- Lista de versões oficiais carregada automaticamente (release e snapshot)
- Download e instalação de bibliotecas, assets, natives e do próprio Java
  (feito pelo CmlLib.Core — o usuário final não precisa ter Java instalado)
- Botão "Jogar" que instala (se necessário) e inicia o jogo
- Importar Mod (.jar) → copia para a pasta `mods`
- Importar Resource Pack (.zip) → copia para a pasta `resourcepacks`
- Criação automática de todas as pastas necessárias
- Área de status + log com barra de progresso

## 2. Estrutura do projeto

```
MinecraftLauncherBR/
├── MinecraftLauncherBR.csproj    # dependências (Avalonia, CmlLib.Core, etc.)
├── app.manifest                  # manifesto do Windows (DPI)
├── Program.cs                    # ponto de entrada
├── App.axaml / App.axaml.cs      # estilos globais + inicialização
├── Models/
│   ├── LauncherConfig.cs         # Client ID, nome do launcher, RAM padrão
│   └── VersionListItem.cs        # item de versão exibido no ComboBox
├── ViewModels/
│   ├── ViewModelBase.cs
│   └── MainWindowViewModel.cs    # toda a lógica/estado da tela (MVVM)
├── Views/
│   ├── MainWindow.axaml          # layout da tela (Grid/StackPanel)
│   ├── MainWindow.axaml.cs       # composição das dependências
│   └── AvaloniaFilePickerService.cs  # seletor de arquivos nativo
└── Services/
    ├── MinecraftService.cs       # encapsula o CmlLib.Core
    ├── AuthService.cs            # login Microsoft real + sessão offline
    ├── FileService.cs            # pastas, importar mod/resource pack
    └── IFilePickerService.cs     # abstração usada pela ViewModel
```

**Separação MVVM:** a `MainWindowViewModel` não conhece nenhum tipo do
Avalonia (só a interface `IFilePickerService`); toda a comunicação com o
CmlLib.Core fica isolada em `MinecraftService` e `AuthService`.

---

## 3. Como compilar e gerar o `.exe` (Windows)

### Pré-requisito único
Instale o **.NET 8 SDK**: https://dotnet.microsoft.com/download/dotnet/8.0
(o Java necessário para o Minecraft é baixado automaticamente pelo próprio launcher).

### Testar rapidamente (sem gerar .exe)
```bash
cd MinecraftLauncherBR
dotnet restore
dotnet run
```

### Gerar o `.exe` final (arquivo único, não precisa de .NET instalado no PC de quem for usar)
```bash
dotnet publish -c Release -r win-x64 --self-contained true ^
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true ^
  -o ./publish
```
*(no Linux/macOS/PowerShell troque o `^` por `` ` `` ou coloque tudo em uma linha só)*

O executável aparece em **`./publish/MinecraftLauncherBR.exe`** — é só copiar
essa pasta `publish` inteira e distribuir (o `.exe` sozinho tem ~150-200 MB
porque já inclui o runtime do .NET).

### Alternativa: .exe menor, mas exige .NET 8 instalado em quem for rodar
```bash
dotnet publish -c Release -r win-x64 --self-contained false -o ./publish
```

### Para Linux ou macOS
Troque `win-x64` por `linux-x64` ou `osx-x64`/`osx-arm64`. A interface e a
lógica são 100% multiplataforma; só muda o executável final.

### Se der erro de compilação
O CmlLib.Core é atualizado com frequência. Se algum pacote não resolver com a
versão fixada no `.csproj`, rode (isso busca sempre a versão mais nova):
```bash
dotnet add package CmlLib.Core
dotnet add package CmlLib.Core.Auth.Microsoft
dotnet add package XboxAuthNet.Game.Msal
dotnet restore
```

---

## 4. Configurando o login Microsoft (Client ID)

O botão **"Entrar com a Microsoft"** só funciona depois de você criar um
Client ID gratuito no Azure. O **Modo Offline funciona sem isso**.

### Passo a passo
1. Acesse https://portal.azure.com e vá em **"Azure Active Directory" → "Registros de aplicativo" (App registrations)**.
2. Clique em **"Novo registro" (New registration)**.
   - Nome: qualquer um (ex.: `MinecraftLauncherBR`)
   - Tipos de conta com suporte: **"Contas em qualquer diretório organizacional e contas pessoais da Microsoft (ex.: Skype, Xbox)"**
   - Redirecionar URI: tipo **"Público/cliente nativo (móvel e desktop)"**, valor `http://localhost`
   - Clique em **Registrar**
3. Na página do app criado, copie o **"ID do aplicativo (cliente)" / Application (client) ID** — esse é o valor que você vai usar.
4. Vá em **"Autenticação" (Authentication)**:
   - Confirme que existe uma plataforma "Mobile and desktop applications"
   - Marque **"Permitir fluxos de cliente público" (Allow public client flows) = Sim**
   - Salve
5. **Passo extra obrigatório:** registre seu Client ID na Mojang para evitar erro `403 FORBIDDEN`, seguindo o artigo oficial:
   https://help.minecraft.net/hc/en-us/articles/16254801392141
   (sem isso, mesmo com tudo certo no Azure, a API de autenticação do Minecraft recusa a requisição).
6. Abra `Models/LauncherConfig.cs` e troque:
   ```csharp
   public const string MicrosoftClientId = "COLOQUE_SEU_CLIENT_ID_AQUI";
   ```
   pelo Client ID copiado no passo 3.
7. Recompile o projeto.

---

## 5. Observações importantes

- **Mods e resource packs são compartilhados entre versões** na mesma pasta
  do jogo — é assim que o Minecraft/Forge/Fabric funcionam de verdade (não
  existe pasta "mods" separada por versão no jogo original).
- **Um `.jar` de mod sozinho não faz nada em uma instalação vanilla.** Ele
  precisa de um mod loader (Forge, Fabric, NeoForge...) instalado na versão
  selecionada. Este launcher importa o arquivo para a pasta certa; instalar
  o mod loader em si ficou fora do escopo pedido (dá pra evoluir usando os
  pacotes `CmlLib.Core.Installer.Forge` / `CmlLib.Core.Installer.Fabric`).
- **Modo Offline não é "modo pirata".** Ele cria uma sessão local válida
  apenas para jogo sozinho, LAN ou servidores que você mesmo administra em
  modo offline — não contorna a verificação de servidores no modo online da
  Mojang/Microsoft.
- A pasta do jogo por padrão fica no local padrão do sistema operacional
  (`MinecraftPath()` sem argumento). Se preferir uma pasta ao lado do `.exe`,
  troque em `Views/MainWindow.axaml.cs`:
  `new MinecraftService()` → `new MinecraftService("./minecraft-data")`.

---

## 6. Pacotes usados (já no `.csproj`)

| Pacote | Função |
|---|---|
| Avalonia / Avalonia.Desktop / Avalonia.Themes.Fluent | Interface gráfica multiplataforma |
| Avalonia.Fonts.Inter | Fonte padrão da interface |
| CommunityToolkit.Mvvm | `ObservableObject` / `RelayCommand` (MVVM) |
| CmlLib.Core | Listar/baixar versões, bibliotecas, assets, natives e iniciar o jogo |
| CmlLib.Core.Auth.Microsoft + XboxAuthNet.Game.Msal | Login real com conta Microsoft/Xbox |

Bom jogo! 🎮
