; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "NFCRing"
#define MyAppVersion "1.0.0.1"
#define MyAppPublisher "Sesame Company, Inc."
#define MyAppURL "http://www.example.com/"
#define MyAppExeName "NFCRing.UI.View.exe"
#define AppGuid "{F7D4EF32-2D80-441A-A499-3E6000BFCEBA}"
#define AppPath = "{app}\App"
#define ServiceCredentialPath = "{app}\Service\Credential"
#define ServiceManagementPath = "{app}\Service\Management"
#define ServiceAppPath = "{app}\Service\Service"
#define ServiceAppPluginsPath = "{app}\Service\Service\Plugins"

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
Source: "..\..\UI\NFCRing.UI.View\bin\Release\NFCRing.UI.View.exe"; DestDir: {#AppPath}; Flags: ignoreversion

; Application files
Source: "..\..\UI\NFCRing.UI.View\bin\Release\NFCRing.UI.View.exe.config"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\UI\NFCRing.UI.View\bin\Release\NLog.config"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\UI\NFCRing.UI.View\bin\Release\Autofac.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\UI\NFCRing.UI.View\bin\Release\Autofac.Extras.CommonServiceLocator.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\UI\NFCRing.UI.View\bin\Release\GalaSoft.MvvmLight.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\UI\NFCRing.UI.View\bin\Release\GalaSoft.MvvmLight.Extras.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\UI\NFCRing.UI.View\bin\Release\GalaSoft.MvvmLight.Platform.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\UI\NFCRing.UI.View\bin\Release\Microsoft.Practices.ServiceLocation.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\UI\NFCRing.UI.View\bin\Release\Newtonsoft.Json.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\UI\NFCRing.UI.View\bin\Release\NFCRing.UI.ViewModel.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\UI\NFCRing.UI.View\bin\Release\NFCRingServiceCommon.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\UI\NFCRing.UI.View\bin\Release\NLog.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\UI\NFCRing.UI.View\bin\Release\System.Windows.Interactivity.dll"; DestDir: {#AppPath}; Flags: ignoreversion
Source: "..\..\UI\NFCRing.UI.View\bin\Release\Icon.ico"; DestDir: {#AppPath}; Flags: ignoreversion

; Service files
Source: "..\..\Service\NFCRingServiceHost\bin\Release\NFCRingServiceHost.exe.config"; DestDir: {#ServiceAppPath}; Flags: ignoreversion
Source: "..\..\Service\NFCRingServiceHost\bin\Release\Newtonsoft.Json.dll"; DestDir: {#ServiceAppPath}; Flags: ignoreversion
Source: "..\..\Service\NFCRingServiceHost\bin\Release\NFCRingServiceCommon.dll"; DestDir: {#ServiceAppPath}; Flags: ignoreversion
Source: "..\..\Service\NFCRingServiceHost\bin\Release\NFCRingServiceCore.dll"; DestDir: {#ServiceAppPath}; Flags: ignoreversion
Source: "..\..\Service\NFCRingServiceHost\bin\Release\NFCRingServiceHost.exe"; DestDir: {#ServiceAppPath}; Flags: ignoreversion
Source: "..\..\Release\WinAPIWrapper.dll"; DestDir: {#ServiceAppPath}; Flags: ignoreversion
Source: "..\..\Service\Plugins\NFCRing.Plugin.Lock\bin\Release\NFCRing.Plugin.Lock.dll"; DestDir: {#ServiceAppPluginsPath}; Flags: ignoreversion
Source: "..\..\Service\Plugins\NFCRing.Plugin.Unlock\bin\Release\NFCRing.Plugin.Unlock.dll"; DestDir: {#ServiceAppPluginsPath}; Flags: ignoreversion
Source: "..\..\Service\Plugins\NFCRing.Plugin.Unlock\bin\Release\NFCRingServiceCommon.dll"; DestDir: {#ServiceAppPluginsPath}; Flags: ignoreversion
Source: "..\..\Service\Plugins\NFCRing.Plugin.Unlock\bin\Release\NFCRingServiceCore.dll"; DestDir: {#ServiceAppPluginsPath}; Flags: ignoreversion
Source: "..\..\Service\Plugins\NFCRing.Plugin.Unlock\bin\Release\Newtonsoft.Json.dll"; DestDir: {#ServiceAppPluginsPath}; Flags: ignoreversion

Source: "..\..\Management\RegistryWriter\bin\Release\CredentialRegistration.exe.config"; DestDir: {#ServiceManagementPath}; Flags: ignoreversion
Source: "..\..\Management\RegistryWriter\bin\Release\Newtonsoft.Json.dll"; DestDir: {#ServiceManagementPath}; Flags: ignoreversion
Source: "..\..\Management\RegistryWriter\bin\Release\NFCRingServiceCommon.dll"; DestDir: {#ServiceManagementPath}; Flags: ignoreversion
Source: "..\..\Management\RegistryWriter\bin\Release\CredentialRegistration.exe"; DestDir: {#ServiceManagementPath}; Flags: ignoreversion
Source: "..\..\Release\WinAPIWrapper.dll"; DestDir: {#ServiceManagementPath}; Flags: ignoreversion

Source: "..\..\bin\Release\Credential\CredUILauncher.exe"; DestDir: {#ServiceCredentialPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Credential\NFCRingCredentialProvider.dll"; DestDir: {#ServiceCredentialPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Credential\tileimage.bmp"; DestDir: {#ServiceCredentialPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Credential\Register.reg"; DestDir: {#ServiceCredentialPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Credential\Unregister.reg"; DestDir: {#ServiceCredentialPath}; Flags: ignoreversion

; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{commonprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Code]
#include "dotnet.pas"
#include "checkinstalled.pas"

function InitializeSetup(): Boolean;
begin
    if not CheckNetFramework() then
    begin
        result := false;
    end else
    begin
        result := CheckInstalledVersion();
    end;
end; 

[Run]
Filename: "{#AppPath}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
Filename: "{#ServiceAppPath}\NFCRingServiceHost.exe"; Flags: runascurrentuser; Parameters: "--install"

[UninstallRun]
Filename: "{#ServiceAppPath}\NFCRingServiceHost.exe"; Parameters: "--uninstall"