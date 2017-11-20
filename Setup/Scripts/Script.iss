; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#include <idp.iss>

#define MyAppName "NFCRing"
#define MyAppVersion "1.0.0.1"
#define MyAppPublisher "Sesame Company, Inc."
#define MyAppURL "http://www.example.com/"
#define MyAppExeName "NFCRing.UI.View.exe"
#define AppGuid "{F7D4EF32-2D80-441A-A499-3E6000BFCEBA}"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
PrivilegesRequired=admin

AppId={{#AppGuid}
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
OutputBaseFilename=NFCRing_1.0

Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; exe file
Source: "..\..\UI\NFCRing.UI.View\bin\Release\NFCRing.UI.View.exe"; DestDir: "{app}"; Flags: ignoreversion

; Application files
Source: "..\..\UI\NFCRing.UI.View\bin\Release\NFCRing.UI.View.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\UI\NFCRing.UI.View\bin\Release\NLog.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\UI\NFCRing.UI.View\bin\Release\Autofac.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\UI\NFCRing.UI.View\bin\Release\Autofac.Extras.CommonServiceLocator.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\UI\NFCRing.UI.View\bin\Release\GalaSoft.MvvmLight.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\UI\NFCRing.UI.View\bin\Release\GalaSoft.MvvmLight.Extras.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\UI\NFCRing.UI.View\bin\Release\GalaSoft.MvvmLight.Platform.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\UI\NFCRing.UI.View\bin\Release\Microsoft.Practices.ServiceLocation.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\UI\NFCRing.UI.View\bin\Release\NFCRing.UI.ViewModel.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\UI\NFCRing.UI.View\bin\Release\NLog.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\UI\NFCRing.UI.View\bin\Release\Icon.ico"; DestDir: "{app}"; Flags: ignoreversion

; Service files
Source: "..\..\Service\NFCRingServiceHost\bin\Release\NFCRingServiceHost.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Service\NFCRingServiceHost\bin\Release\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Service\NFCRingServiceHost\bin\Release\NFCRingServiceCommon.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Service\NFCRingServiceHost\bin\Release\NFCRingServiceCore.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Service\NFCRingServiceHost\bin\Release\NFCRingServiceHost.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Release\WinAPIWrapper.dll"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{commonprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Code]
#include "checkinstalled.pas"
#include "dotnet45.pas"        

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
Filename: "{app}\NFCRingServiceHost.exe"; Flags: runascurrentuser; Parameters: "--install"

[UninstallRun]
Filename: "{app}\NFCRingServiceHost.exe"; Parameters: "--uninstall"