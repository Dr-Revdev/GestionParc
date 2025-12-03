; Script Inno Setup pour GestiParc
; Créé avec Inno Setup 6

#define MyAppName "GestiParc"
#define MyAppVersion "2.0.0"
#define MyAppPublisher "Revrum"
#define MyAppExeName "GestiParc_v2.0.0.exe"

[Setup]
; Informations de l'application
AppId={{A7B3C9E2-5F1D-4A8B-9E2C-7D6F4A1B8E3C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir=installer
OutputBaseFilename=GestiParc_Setup_v{#MyAppVersion}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern

; Icônes et images
; SetupIconFile=GestionParc.ico
; WizardImageFile=wizard.bmp

; Privilèges requis
PrivilegesRequired=admin

; Architecture
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Fichiers de l'application
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: N'utilisez pas "Flags: ignoreversion" sur les fichiers système

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Désinstaller {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[Code]
// Vérification de .NET 9 Runtime
function IsDotNetInstalled(): Boolean;
var
  ResultCode: Integer;
begin
  // Vérifie si .NET 9 Desktop Runtime est installé
  Result := Exec('dotnet', '--list-runtimes', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) and (ResultCode = 0);
end;

function InitializeSetup(): Boolean;
begin
  Result := True;
  
  if not IsDotNetInstalled() then
  begin
    if MsgBox('Microsoft .NET 9 Desktop Runtime est requis pour exécuter cette application.' + #13#10 + 
              'Voulez-vous télécharger et installer .NET 9 Runtime maintenant ?', 
              mbConfirmation, MB_YESNO) = IDYES then
    begin
      ShellExec('open', 'https://dotnet.microsoft.com/download/dotnet/9.0', '', '', SW_SHOW, ewNoWait, ResultCode);
      Result := False;
    end
    else
    begin
      Result := False;
    end;
  end;
end;
