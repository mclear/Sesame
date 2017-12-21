; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "NFCRing Fence"
#define GitCommitHash "5afa111"
#define MyAppVersion "1.0.0.7"
#define MyAppPublisher "McLear Ltd"
#define MyAppURL "http://www.example.com/"
#define MyAppExeName "NFCRing.UI.View.exe"
#define AppGuid "{F7D4EF32-2D80-441A-A499-3E6000BFCEBA}"
#define AppPath = "{app}\App"
#define ServiceCredentialPath = "{app}\Service\Credential"
#define ServiceManagementPath = "{app}\Service\Management"
#define ServiceAppPath = "{app}\Service\Service"
#define ServiceAppPluginsPath = "{app}\Service\Service\Plugins"
#define RegistryKey = "{{8EB4E5F7-9DFB-4674-897C-2A584934CDBE}"
#define ProviderNameKey = "NFCRingCredentialProvider"
#define VCmsg "Installing Microsoft Visual C++ Redistributable...."

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64

AppId={{#AppGuid}
AppName= {#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

DefaultDirName={pf}\{#MyAppName}
DisableProgramGroupPage=yes

OutputDir=..\Result
OutputBaseFilename={#MyAppName}_{#GitCommitHash}_{#MyAppVersion}

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
Source: "..\..\bin\Release\Service\NFCRingServiceHost.exe.config"; DestDir: {#ServiceAppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Service\Newtonsoft.Json.dll"; DestDir: {#ServiceAppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Service\NFCRingServiceCommon.dll"; DestDir: {#ServiceAppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Service\NFCRingServiceCore.dll"; DestDir: {#ServiceAppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Service\NFCRingServiceHost.exe"; DestDir: {#ServiceAppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Service\WinAPIWrapper.dll"; DestDir: {#ServiceAppPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Service\Plugins\NFCRing.Plugin.Lock.dll"; DestDir: {#ServiceAppPluginsPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Service\Plugins\NFCRing.Plugin.Unlock.dll"; DestDir: {#ServiceAppPluginsPath}; Flags: ignoreversion
;Source: "..\..\bin\Release\Service\Plugins\NFCRingServiceCommon.dll"; DestDir: {#ServiceAppPluginsPath}; Flags: ignoreversion
;Source: "..\..\bin\Release\Service\Plugins\NFCRingServiceCore.dll"; DestDir: {#ServiceAppPluginsPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Service\Plugins\Newtonsoft.Json.dll"; DestDir: {#ServiceAppPluginsPath}; Flags: ignoreversion

Source: "..\..\bin\Release\Management\CredentialRegistration.exe.config"; DestDir: {#ServiceManagementPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Management\Newtonsoft.Json.dll"; DestDir: {#ServiceManagementPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Management\NFCRingServiceCommon.dll"; DestDir: {#ServiceManagementPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Management\CredentialRegistration.exe"; DestDir: {#ServiceManagementPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Management\WinAPIWrapper.dll"; DestDir: {#ServiceManagementPath}; Flags: ignoreversion

Source: "..\..\bin\Release\Credential\CredUILauncher.exe"; DestDir: {#ServiceCredentialPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Credential\NFCRingCredentialProvider.dll"; DestDir: {#ServiceCredentialPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Credential\tileimage.bmp"; DestDir: {#ServiceCredentialPath}; Flags: ignoreversion
Source: "..\..\bin\Release\Credential\NFCRingCredentialProvider.dll"; DestDir: {sys};

; Visual C++ 2015
Source: "vc_redist.x64.exe"; DestDir: {tmp}; Flags: deleteafterinstall

; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{commonprograms}\{#MyAppName}"; Filename: "{app}\App\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\App\{#MyAppExeName}"; Tasks: desktopicon

[Code]
#include "dotnet.pas"
#include "checkinstalled.pas"
#include "vc.pas"

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
Filename: "{tmp}\vc_redist.x64.exe"; StatusMsg: "{#VCmsg}"; Check: IsWin64 and VCRedistNeedsInstall
Filename: "{#AppPath}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
Filename: "{#ServiceAppPath}\NFCRingServiceHost.exe"; Flags: runascurrentuser; Parameters: "--install"

[UninstallRun]
Filename: "{#ServiceAppPath}\NFCRingServiceHost.exe"; Parameters: "--uninstall"
Filename: "{sys}\NFCRingCredentialProvider.dll"

[Registry]
; [HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\Credential Providers\{8EB4E5F7-9DFB-4674-897C-2A584934CDBE}]
; @="NFCRingCredentialProvider"
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\Credential Providers\{#RegistryKey}"; ValueType: string; ValueName: ""; ValueData: "{#ProviderNameKey}"; Flags: uninsdeletekey

; [HKEY_CLASSES_ROOT\CLSID\{8EB4E5F7-9DFB-4674-897C-2A584934CDBE}]
; @="NFCRingCredentialProvider"
Root: HKCR; Subkey: "CLSID\{#RegistryKey}"; ValueType: string; ValueName: ""; ValueData: "{#ProviderNameKey}"; Flags: uninsdeletekey

; [HKEY_CLASSES_ROOT\CLSID\{8EB4E5F7-9DFB-4674-897C-2A584934CDBE}\InprocServer32]
; @="NFCRingCredentialProvider.dll"
; "ThreadingModel"="Apartment"
Root: HKCR; Subkey: "CLSID\{#RegistryKey}\InprocServer32"; ValueType: string; ValueName: ""; ValueData: "{#ProviderNameKey}.dll"
Root: HKCR; Subkey: "CLSID\{#RegistryKey}\InprocServer32"; ValueType: string; ValueName: "ThreadingModel"; ValueData: "Apartment"