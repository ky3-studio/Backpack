#define AppName       "Backpack Viewer"
#define AppVersion GetVersionNumbersString(".\..\publish\BackpackViewer.exe")
#define AppPublisher  "KY3 STUDIO"
#define AppExe        "BackpackViewer.exe"
#define AppId         "{{4B2E1C9A-6F3D-4E7B-8A2C-1D5E9F3B7C4A}"
#define SrcDir        ".\..\publish"
#define NativeDir     ".\..\x64\Release"
#define IconFile      ".\..\viewer\Assets\logo.ico"
#define OutDir        ".\..\..\..\release-output"

[Setup]
AppId={#AppId}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
AppCopyright=Copyright (C) 2026 {#AppPublisher}
VersionInfoVersion={#AppVersion}
VersionInfoProductVersion={#AppVersion}
VersionInfoDescription={#AppName} Installer

DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
DisableDirPage=no
DisableReadyPage=no
ShowLanguageDialog=no

ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
MinVersion=10.0.19041

OutputDir={#OutDir}
OutputBaseFilename=backpack-viewer-setup-{#AppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
LZMAUseSeparateProcess=yes

WizardStyle=modern
WizardSizePercent=110
SetupIconFile={#IconFile}
UninstallDisplayIcon={app}\{#AppExe}
UninstallDisplayName={#AppName}

CloseApplications=force
RestartApplications=no
AllowNoIcons=yes

[Languages]
Name: "chs"; MessagesFile: "compiler:ChineseSimplified.isl"

[Tasks]
Name: "desktopicon"; Description: "创建桌面快捷方式"; GroupDescription: "附加快捷方式:"

[Files]
; viewer 主体（publish 全量）
Source: "{#SrcDir}\{#AppExe}";  DestDir: "{app}"; Flags: ignoreversion
Source: "{#SrcDir}\*";          DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "*.pdb"

; C++ 解析 DLL
Source: "{#NativeDir}\backpack.dll"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#AppName}";       Filename: "{app}\{#AppExe}"; WorkingDir: "{app}"
Name: "{group}\卸载 {#AppName}";  Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExe}"; Description: "立即启动 {#AppName}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}\Cache"
Type: dirifempty;     Name: "{app}"

[Code]
var
  GDesktopIconExists: Boolean;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  UninstStr: string;
  UninstKey: string;
  ResultCode: Integer;
  DesktopPath: string;
  ShortcutPath: string;
begin
  Result := '';
  DesktopPath := ExpandConstant('{autodesktop}');
  ShortcutPath := AddBackslash(DesktopPath) + ExpandConstant('{#AppName}') + '.lnk';
  GDesktopIconExists := FileExists(ShortcutPath);
  if GDesktopIconExists and (WizardIsTaskSelected('desktopicon') = False) then
    WizardSelectTasks('desktopicon');
  UninstKey := 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\' +
               ExpandConstant('{#SetupSetting("AppId")}') + '_is1';
  if not RegQueryStringValue(HKLM, UninstKey, 'UninstallString', UninstStr) then
    RegQueryStringValue(HKCU, UninstKey, 'UninstallString', UninstStr);
  if UninstStr <> '' then
  begin
    UninstStr := RemoveQuotes(UninstStr);
    Exec(UninstStr, '/VERYSILENT /NORESTART /SUPPRESSMSGBOXES', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  end;
end;

procedure ApplyCustomFont(C: TControl);
var
  I: Integer;
begin
  if C is TLabel              then TLabel(C).Font.Name             := 'Microsoft YaHei UI'
  else if C is TNewStaticText  then TNewStaticText(C).Font.Name    := 'Microsoft YaHei UI'
  else if C is TNewCheckListBox then TNewCheckListBox(C).Font.Name := 'Microsoft YaHei UI'
  else if C is TNewListBox     then TNewListBox(C).Font.Name       := 'Microsoft YaHei UI'
  else if C is TNewMemo        then TNewMemo(C).Font.Name          := 'Microsoft YaHei UI'
  else if C is TNewEdit        then TNewEdit(C).Font.Name          := 'Microsoft YaHei UI'
  else if C is TNewComboBox    then TNewComboBox(C).Font.Name      := 'Microsoft YaHei UI'
  else if C is TNewCheckBox    then TNewCheckBox(C).Font.Name      := 'Microsoft YaHei UI'
  else if C is TNewRadioButton then TNewRadioButton(C).Font.Name   := 'Microsoft YaHei UI'
  else if C is TNewButton      then TNewButton(C).Font.Name        := 'Microsoft YaHei UI'
  else if C is TButton         then TButton(C).Font.Name           := 'Microsoft YaHei UI'
  else if C is TForm           then TForm(C).Font.Name             := 'Microsoft YaHei UI';
  if C is TWinControl then
    for I := 0 to TWinControl(C).ControlCount - 1 do
      ApplyCustomFont(TWinControl(C).Controls[I]);
end;

procedure InitializeWizard;
begin
  ApplyCustomFont(WizardForm);
end;

procedure InitializeUninstallProgressForm;
begin
  ApplyCustomFont(UninstallProgressForm);
end;
