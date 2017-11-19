; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#include <idp.iss>

#define MyAppName "NFCRing_test"
#define MyAppVersion "1.0.0.1"
#define MyAppPublisher "Sesame Company, Inc."
#define MyAppURL "http://www.example.com/"
#define MyAppExeName "NFCRing.UI.View.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)

AppId={{BCB6CF6D-434A-4F06-81FB-495090F9DFBE}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

DefaultDirName={pf}\{#MyAppName}
DisableProgramGroupPage=yes

OutputDir=C:\Dev\github\Sesame\Setup\Result
OutputBaseFilename=Setup

Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; exe file
Source: "C:\Dev\github\Sesame\UI\NFCRing.UI.View\bin\Release\NFCRing.UI.View.exe"; DestDir: "{app}"; Flags: ignoreversion
; Application files
Source: "c:\Dev\github\Sesame\UI\NFCRing.UI.View\bin\Release\*"; DestDir: "{app}"; Flags: ignoreversion
; Service files
Source: "C:\Dev\github\Sesame\Service\NFCRingServiceHost\bin\Release\*"; DestDir: "{app}"; Flags: ignoreversion

; NOTE: Don't use "Flags: ignoreversion" on any shared system files
; Source: "E:\install\dotNetFx45_Full_setup.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: not IsRequiredDotNetDetected

[Icons]
Name: "{commonprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Code]
#include "dotnet45.pas"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
; Filename: {tmp}\dotNetFx45_Full_setup.exe; Parameters: "/q:a /c:""install /l /q"""; Check: not IsRequiredDotNetDetected; StatusMsg: Microsoft Framework 4.5 is installed. Please wait...
Filename: "{app}\NFCRingServiceHost.exe"; Parameters: "--install"

[UninstallRun]
Filename: "{app}\NFCRingServiceHost.exe"; Parameters: "--uninstall"
