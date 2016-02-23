# Sesame
An Open Source Microsoft Windows NFC Login and Logout

*In case of emergencies: *

If you are having trouble logging in, open an RDP session from another machine and then use your username and password to login again.

Prerequisites:

Windows 7, 8, 8.1 or 10 64-bit Windows (It will not work on a 32-bit system). 

.Net 4.5 (included with Windows 8.1 or higher) https://www.microsoft.com/en-au/download/details.aspx?id=40779 

Visual C++ Redistributable Packages for Visual Studio 2013 https://www.microsoft.com/en-au/download/details.aspx?id=4078


Tested reader units include:

ACS ACR122U

HID OMNIKEY 6321

HID OMNIKEY 5021


Installation Instructions:

There are three seperate parts to this, if any fails then you will need to check that you have the dependencies correctly installed and are running programs in Administrator mode where applicable.

Registering the credential provider:

Extract "\NFC Credential\NFCRingCredentialProvider.dll" and "\NFC Credential\tileimage.bmp" to "C:\Windows\System32" Run "Register.reg". You may need to run it as Administrator. Windows may pop up a smart screen message. You'll need to allow this to run for the provider to function.

To test the credential provider is installed, Run "CredUILauncher.exe" and it should show a login prompt.

Because no NFC token is registered yet, click "Cancel" to get rid of this.


Registering a token:

Extract the "NFC Credential Registration" folder somewhere. In this folder, right-click and run "RegistryWriter.exe" as Administrator

While holding your NFC token on the reader, click the "Read NFC Tag" button and it should fill the first field with your token's ID (It'll look something like 0400AF49363A719000)

Enter your username

Enter your password.

Click "Enrol", and if successful, you should get a message box that says "Credential saved"

Close this application.

To test, run "CredUILauncher.exe" from the previous instructions and this time you should be able to swipe your NFC token to close the window.


Installing the Service:

Extract the entire "NFC Ring Service" folder somewhere.

Right-click the "NFCRingServiceHost.exe" and all DLL files in the service folder select "Properties". At the bottom of this page, you may need to select "Unblock".

In the plugins folder, right-click each file and select "Properties" and "Unblock" as well.

In this folder, right-click and run "InstallService.bat" as Administrator (the last line of the command window should say the task completed successfully).

Press any key to close the command window.

Open the services console (Windows Key + R and type "services.msc" without the quotes and press enter) Scroll down to the "NFCRingService" and select "Start", its status should change to "Running".

If you swipe your token now, the system should lock.


Currently known bugs:

Swiping an unregistered tag multiple times may require you to remotely log into the computer with the correct user name and password in order to reset this.

In some instances with Windows 10 the service will stop after a minute on the lock screen. You then need to log in manually and restart the service.
