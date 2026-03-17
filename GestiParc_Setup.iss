; Script Inno Setup pour GestiParc UI
; Emballe le client lourd Windows qui consomme l'API distante configurée dans GestiParc.Ui/App.config.
; Ce setup attend une publication self-contained win-x64 de l'UI.

#define MyAppName "GestiParc"
#define MyAppPublisher "Dr-Revdev"
#define MyAppExeName "GestiParc.exe"
#define MyAppSourceDir "GestiParc.Ui\\bin\\Release\\net9.0-windows\\win-x64\\publish"
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
