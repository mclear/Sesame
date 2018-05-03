![Logo](https://raw.githubusercontent.com/mclear/Sesame/blob/master/UI/NFCRing.UI.View/Icon.ico)

NFC Sesame provides NFC based login and logout functionality for the Microsoft Windows Operating System.  

## Important Disclaimer
This software is in active development and should not be used in any environment you need stability.  You may get locked out of your PC.  Your Unicorns may be exploited.  You have been warned.  We're going to be [raising funds](https://www.kickstarter.com/projects/mclear/526261309?token=201aa2e8) to complete development.

## In case of emergencies

If you are having trouble logging in, open an RDP session from another machine and then use your username and password to login again.

## Prerequisites
* Windows 7, 8, 8.1 or 10 64-bit Windows (It will not work on a 32-bit system).
* [.Net 4.5, included with Windows 8.1 or higher](https://www.microsoft.com/en-au/download/details.aspx?id=40779)
* [Visual C++ Redistributable Packages for Visual Studio 2015](https://www.microsoft.com/en-au/download/details.aspx?id=48145) and install "vc_redist.x64.exe"

## Installation Instructions

There are three separate parts to this, if any fails then you will need to check that you have the dependencies correctly installed and are running programs in Administrator mode where applicable.  *These installation instructions are temporary and will be replaced with a simple MSI installer in the future.*

### Building the binaries (If you have downloaded the source)

1. Open ``\Sesame\Sesame.sln`` with Visual Studio 2015.

1. Build the entire solution (Release or debug, but only x64). This creates a ``\bin\`` folder in the root Sesame directory.

1. For the rest of the instructions, I'll assume you built for Release. If you chose Debug instead, then replace "Release" in the following instructions with "Debug".

1. Make sure your NFC reader is connected to the PC.


### Installing the Service

1. Browse to ``\Sesame\bin\Release\Service`` and Right-click "InstallService.bat" and select "Run as Administrator" and accept the UAC prompt. The last line of the command window should say the task completed successfully. 

1. Press any key to exit the command prompt window.


### Registering the credential provider

1. Copy ``\Sesame\bin\Release\Credential\NFCRingCredentialProvider.dll`` to ``C:\Windows\System32``.

1. Run ``\Sesame\bin\Release\Credential\Register.reg``. You may need to run it as Administrator. Allow the UAC prompt if it pops up. If unsuccessful, run "regedit.exe" as administrator and select "File -> Import" and browse to "Register.reg"


### Registering a token

1. Run ``\Sesame\bin\Release\UI\NFCRing.UI.View.exe``.

1. Start by selecting "Add new NFC Ring" and follow the wizard steps.

1. Swipe your ring when prompted.

1. Enter your password.

1. Swipe your ring again to encrypt the password.



Congratulations, you have now setup NFC Fence. Swipe your ring on your NFC reader to lock the PC. Swipe again to unlock.

