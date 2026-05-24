#define MyAppName "School Enrollment Management System"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "School Enrollment Management System"
#define MyAppExeName "School-Management-System.Wpf.exe"

[Setup]
AppId={{F41AFC17-D872-4785-9A80-6D5928D424AD}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=..\dist\installer
OutputBaseFilename=SchoolManagementSystemSetup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x86compatible and x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#MyAppExeName}
SetupIconFile=..\School-Management-System.Wpf\assets\app-logo.ico
MinVersion=6.1sp1
PrivilegesRequired=admin
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Application binaries
Source: "..\School-Management-System.Wpf\bin\Release\net472\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion; Excludes: "*.pdb,*.xml,*.vshost.exe,*.vshost.exe.config,app.publish\*"
; Database deployment scripts (operator-run, optional)
Source: "..\DatabaseScripts\*"; DestDir: "{app}\DatabaseScripts"; Flags: recursesubdirs createallsubdirs ignoreversion; Excludes: "*.bak"
; End-user documentation
Source: "..\USER_MANUAL.TXT"; DestDir: "{app}\docs"; DestName: "User Manual.txt"; Flags: ignoreversion
Source: "..\README.md"; DestDir: "{app}\docs"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#MyAppName}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; IconFilename: "{app}\assets\app-logo.ico"
Name: "{autoprograms}\{#MyAppName}\User Manual"; Filename: "{app}\docs\User Manual.txt"
Name: "{autoprograms}\{#MyAppName}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; IconFilename: "{app}\assets\app-logo.ico"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Application logs and per-user data live under %LOCALAPPDATA%\SchoolManagementSystem
; and are intentionally preserved so backups, photos, and audit logs survive a reinstall.
Type: filesandordirs; Name: "{app}\logs"
