; Rama AI Assistant - Inno Setup Script
; Builds a Windows installer that deploys Rama to Program Files
; with desktop/start menu shortcuts and .rama-skill file association.

#define MyAppName "Rama"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Rama Team"
#define MyAppURL "https://github.com/rama-assistant"
#define MyAppExeName "Rama.exe"
#define MyOutputBaseFilename "RamaSetup"

[Setup]
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/issues
AppUpdatesURL={#MyAppURL}/releases
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
LicenseFile=..\LICENSE.txt
OutputDir=Output
OutputBaseFilename={#MyOutputBaseFilename}
SetupIconFile=..\Rama\Assets\rama-icon.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
WizardImageAlphaFormat=defined
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#MyAppExeName}
PrivilegesRequired=lowest
ChangesAssociations=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "associateSkill"; Description: "Associate .rama-skill files with Rama"; GroupDescription: "File associations:"; Flags: checked

[Files]
; Main application files from dotnet publish output
Source: "publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; License and docs
Source: "..\README.md"; DestDir: "{app}"; Flags: ignoreversion isreadme
Source: "..\SKILLS_GUIDE.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\LEARNING_GUIDE.md"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
; Associate .rama-skill files
Root: HKA; Subkey: "Software\Classes\.rama-skill"; ValueType: string; ValueName: ""; ValueData: "RamaSkillFile"; Flags: uninsdeletevalue; Tasks: associateSkill
Root: HKA; Subkey: "Software\Classes\RamaSkillFile"; ValueType: string; ValueName: ""; ValueData: "Rama Skill Plugin"; Flags: uninsdeletekey; Tasks: associateSkill
Root: HKA; Subkey: "Software\Classes\RamaSkillFile\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: associateSkill
Root: HKA; Subkey: "Software\Classes\RamaSkillFile\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" --install-skill ""%1"""; Tasks: associateSkill

[Code]
// .NET 8 Runtime check
function IsDotNet8Installed(): Boolean;
var
  ResultCode: Integer;
begin
  // Check if dotnet runtime 8.0+ is installed by running dotnet --list-runtimes
  Result := Exec('cmd.exe', '/c dotnet --list-runtimes | findstr "Microsoft.NETCore.App 8."', '',
    SW_HIDE, ewWaitUntilTerminated, ResultCode) and (ResultCode = 0);
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssInstall then
  begin
    if not IsDotNet8Installed() then
    begin
      MsgBox('Rama requires the .NET 8 runtime.' + #13#10 +
             'Please install it from https://dotnet.microsoft.com/download/dotnet/8.0' + #13#10 +
             'The installer will continue, but Rama may not run without .NET 8.',
             mbInformation, MB_OK);
    end;
  end;
end;

// Create Plugins directory on install
procedure CurInstallStepChanged(CurInstallStep: TSetupStep);
begin
  if CurInstallStep = ssPostInstall then
  begin
    CreateDir(ExpandConstant('{app}\Plugins'));
  end;
end;

[Run]
; Option to launch Rama after installation
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
