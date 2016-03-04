NFC Sesame provides NFC based login and logout functionality for the Microsoft Windows Operating System.  

## Important Disclaimer
This software is in active development and should not be used in any environment you need stability.  You may get locked out of your PC.  Your Unicorns may be exploited.  You have been warned.  We're going to be [raising funds](https://www.kickstarter.com/projects/mclear/526261309?token=201aa2e8) to complete development.

## In case of emergencies

If you are having trouble logging in, open an RDP session from another machine and then use your username and password to login again.

## Prerequisites
* Windows 7, 8, 8.1 or 10 64-bit Windows (It will not work on a 32-bit system). 
* [.Net 4.5, included with Windows 8.1 or higher](https://www.microsoft.com/en-au/download/details.aspx?id=40779)
* [Visual C++ Redistributable Packages for Visual Studio 2013](https://www.microsoft.com/en-au/download/details.aspx?id=4078)


## Installation Instructions

There are three seperate parts to this, if any fails then you will need to check that you have the dependencies correctly installed and are running programs in Administrator mode where applicable.

### Registering the credential provider

1. Extract ``\NFC Credential\NFCRingCredentialProvider.dll`` and ``\NFC Credential\tileimage.bmp`` to ``C:\Windows\System32`` 

1. Run ``Register.reg``. You may need to run it as Administrator. Windows may pop up a smart screen message. You'll need to allow this to run for the provider to function.

1. To test the credential provider is installed, Run ``CredUILauncher.exe`` and it should show a login prompt.

1. Because no NFC token is registered yet, click "Cancel" to get rid of this.

### Registering a token

1. Extract the ``NFC Credential Registration`` folder somewhere. In this folder, right-click and run ``RegistryWriter.exe`` as Administrator

1. While holding your NFC token on the reader, click the "Read NFC Tag" button and it should fill the first field with your token's ID (It'll look something like ``0400AF49363A719000``)

1. Enter your username and password.

1. Click "Enrol", and if successful, you should get a message box that says "Credential saved"

1. Close this application.

To test, run ``CredUILauncher.exe`` from the previous instructions and this time you should be able to swipe your NFC token to close the window.

### Installing the Service

1. Extract the entire ``NFC Ring Service`` folder somewhere.

1. Right-click the ``NFCRingServiceHost.exe`` and all DLL files in the service folder select "Properties". At the bottom of this page, you may need to select "Unblock".

1. In the plugins folder, right-click each file and select "Properties" and "Unblock" as well.

1. In this folder, right-click and run ``InstallService.bat`` as Administrator (the last line of the command window should say the task completed successfully).

1. Press any key to close the command window.

1. Open the services console (Windows Key + R and type ``services.msc`` without the quotes and press enter) Scroll down to the ``NFCRingService`` and select "Start", its status should change to "Running".

If you swipe your token now, the system should lock.

Confirmed working NFC reader units:
* ACS ACR122U
* HID OMNIKEY 6321
* HID OMNIKEY 5021
