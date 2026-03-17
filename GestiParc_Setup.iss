; Script Inno Setup pour GestiParc UI
; Emballe le client lourd Windows qui consomme l'API distante configurée dans GestiParc.Ui/App.config.

#define MyAppName "GestiParc"
#define MyAppPublisher "Dr-Revdev"
#define MyAppExeName "GestiParc.exe"
#define MyAppSourceDir "GestiParc.Ui\\bin\\Debug\\net9.0-windows"
#define MyAppIcon "GestionParc.ico"
#define MyAppVersion GetStringFileInfo(SourcePath + MyAppSourceDir + "\\" + MyAppExeName, "ProductVersion")

[Setup]
AppId={{A7B3C9E2-5F1D-4A8B-9E2C-7D6F4A1B8E3C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL=https://gestiparc-api.irobert.fr
AppSupportURL=https://gestiparc-api.irobert.fr
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir=installer
OutputBaseFilename=GestiParc_Setup_v{#MyAppVersion}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\{#MyAppExeName}
SetupIconFile={#MyAppIcon}
ChangesAssociations=no
DisableProgramGroupPage=yes
CloseApplications=yes
CloseApplicationsFilter=*.exe
PrivilegesRequired=admin

ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#MyAppSourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Désinstaller {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[Code]
function IsDotNetDesktopRuntimeInstalled(): Boolean;
begin
  Result :=
    RegValueExists(HKLM64, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App', '9.0.0') or
    RegValueExists(HKLM64, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App', '9.0.1') or
    RegValueExists(HKLM64, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App', '9.0.2') or
    RegValueExists(HKLM64, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App', '9.0.3') or
    RegValueExists(HKLM64, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App', '9.0.4') or
    RegValueExists(HKLM64, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App', '9.0.5') or
    RegValueExists(HKLM64, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App', '9.0.6') or
    RegValueExists(HKLM64, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App', '9.0.7') or
    RegValueExists(HKLM64, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App', '9.0.8') or
    RegValueExists(HKLM64, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App', '9.0.9') or
    RegValueExists(HKLM64, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App', '9.0.10') or
    RegValueExists(HKLM64, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App', '9.0.11') or
    RegValueExists(HKLM64, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App', '9.0.12');
end;

function InitializeSetup(): Boolean;
var
  ResultCode: Integer;
begin
  Result := True;

  if not DirExists(ExpandConstant('{src}\{#MyAppSourceDir}')) then
  begin
    MsgBox(
      'Dossier source introuvable :' + #13#10 +
      ExpandConstant('{src}\{#MyAppSourceDir}') + #13#10#13#10 +
      'Compile d''abord GestiParc.Ui, puis relance ISCC.',
      mbCriticalError,
      MB_OK
    );
    Result := False;
    exit;
  end;

  if not FileExists(ExpandConstant('{src}\{#MyAppSourceDir}\{#MyAppExeName}')) then
  begin
    MsgBox(
      'Exécutable UI introuvable :' + #13#10 +
      ExpandConstant('{src}\{#MyAppSourceDir}\{#MyAppExeName}') + #13#10#13#10 +
      'Vérifie le chemin MyAppSourceDir dans le script.',
      mbCriticalError,
      MB_OK
    );
    Result := False;
    exit;
  end;

  if not IsDotNetDesktopRuntimeInstalled() then
  begin
    if MsgBox('Microsoft .NET 9 Desktop Runtime est requis pour exécuter cette application.' + #13#10 + 
              'Voulez-vous ouvrir la page de téléchargement maintenant ?', 
              mbConfirmation, MB_YESNO) = IDYES then
    begin
      ShellExec('open', 'https://dotnet.microsoft.com/fr-fr/download/dotnet/9.0/runtime', '', '', SW_SHOW, ewNoWait, ResultCode);
      Result := False;
    end
    else
    begin
      Result := False;
    end;
  end;
end;
