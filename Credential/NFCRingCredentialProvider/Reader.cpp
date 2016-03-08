#include "Reader.h"
#include <strsafe.h>
#include "sha1.h"
#include <codecvt>
//#pragma comment ( lib, "cryptlib" )
//#include "..\External\include\sha.h"
//#include "..\External\include\aes.h"

#pragma warning(disable : 4996)

Reader::Reader(void)
{
	_hInst = NULL;
	_pProvider = NULL;
}

Reader::~Reader(void)
{

	Stop();

	// make sure to release any reference we have to the provider.
	if (_pProvider != NULL)
	{
		_pProvider->Release();
		_pProvider = NULL;
	}
}

void Reader::Stop()
{
	// end thread
	_checkLoop = false;
	closesocket(_soc);
	WSACleanup();
	if (_readerThread.joinable())
		_readerThread.join();
}

// Performs the work required to spin off our message so we can listen for events.
HRESULT Reader::Initialize(NFCCredentialProvider *pProvider)
{
	HRESULT hr = S_OK;

	// Be sure to add a release any existing provider we might have, then add a reference
	// to the provider we're working with now.
	if (_pProvider != NULL)
	{
		_pProvider->Release();
	}
	_pProvider = pProvider;
	_pProvider->AddRef();

	// start listening thread for reader events
	_checkLoop = true;

	int result;
	result = WSAStartup(MAKEWORD(2, 2), &wsaData);
	if (result != 0) {
		printf("WSAStartup failed with error: %d\n", result);
		//return 1;
		return E_UNEXPECTED; //failed
	}
	_soc = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
	if (_soc == INVALID_SOCKET)
	{
		//std::cout << "socket is bullshit and didnt start" << std::endl;
		WSACleanup();
		//return 1;
		return E_UNEXPECTED;
	}

	//result = connect(_soc, (struct sockaddr *)&destination, sizeof(destination));

	//if (result != 0)
	//	return E_UNEXPECTED;
	//else
	_serviceFound = true;
	// this is where we'd start the thread to check for a valid ring
	_readerThread = std::thread(&Reader::CheckNFC, this);

	return hr;
}

void Reader::CheckNFC()
{
	struct sockaddr_in destination;
	destination.sin_family = AF_INET;
	destination.sin_port = htons(28416);
	destination.sin_addr.s_addr = inet_addr("127.0.0.1");

	int result = connect(_soc, (struct sockaddr *)&destination, sizeof(destination));

	for (int i = 0; i < 15; i++)
	{
		if (result != 0)
			result = connect(_soc, (struct sockaddr *)&destination, sizeof(destination));
		else
			break;
		std::this_thread::sleep_for(std::chrono::seconds(2));
	}

	std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> converter;
	SHA1 sha1;
	std::string hashed = "";
	char hex[108];
	int newData = 0;
	if (_soc != INVALID_SOCKET)
	{
		newData = recv(_soc, hex, 100, 0);
		if (newData > 0)
		{
			_kerbrosCredentialRetrieved = false;
			_key = L"";

			sprintf(&hex[newData], "%s", "02164873");

			hashed = sha1(hex, newData + 8);
			hashed = sha1(hashed);
			_key = converter.from_bytes(hashed);

			_kerbrosCredentialRetrieved = true;

			// fire "CredentialsChanged" event
			if (_pProvider != NULL)
				_pProvider->OnNFCStatusChanged();
		}
	}
}

bool Reader::HasLogin()
{
	return _kerbrosCredentialRetrieved;
}

HRESULT Reader::GetLogin(
	CREDENTIAL_PROVIDER_GET_SERIALIZATION_RESPONSE* pcpgsr,
	CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION* pcpcs,
	PWSTR* ppwszOptionalStatusText,
	CREDENTIAL_PROVIDER_STATUS_ICON* pcpsiOptionalStatusIcon,
	CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus
	)
{
	if (!_kerbrosCredentialRetrieved || _key == L"")
		return E_FAIL;

	std::wstring regpath = std::wstring(L"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Authentication\\Credential Providers\\{8EB4E5F7-9DFB-4674-897C-2A584934CDBE}\\" + _key);

	ULONG res;
	std::wstring un;
	std::wstring pw;

	res = GetDataForNFCToken(regpath, &un, &pw);

	if (res != ERROR_SUCCESS)
		return E_FAIL;

	_kerbrosCredentialRetrieved = false;
	_key = L"";

	HRESULT hr = S_OK;
	WCHAR wsz[MAX_COMPUTERNAME_LENGTH + 1];
	DWORD cch = ARRAYSIZE(wsz);
	if (GetComputerNameW(wsz, &cch))
	{
		PWSTR pwzProtectedPassword;
		// cpus
		hr = ProtectIfNecessaryAndCopyPassword(&pw[0], CPUS_LOGON, &pwzProtectedPassword);

		if (SUCCEEDED(hr))
		{
			KERB_INTERACTIVE_UNLOCK_LOGON kiul;

			// Initialize kiul with weak references to our credential.
			hr = KerbInteractiveUnlockLogonInit(wsz, &un[0], pwzProtectedPassword, cpus, &kiul);

			if (SUCCEEDED(hr))
			{
				// We use KERB_INTERACTIVE_UNLOCK_LOGON in both unlock and logon scenarios.  It contains a
				// KERB_INTERACTIVE_LOGON to hold the creds plus a LUID that is filled in for us by Winlogon
				// as necessary.
				hr = KerbInteractiveUnlockLogonPack(kiul, &pcpcs->rgbSerialization, &pcpcs->cbSerialization);

				if (SUCCEEDED(hr))
				{
					ULONG ulAuthPackage;
					hr = RetrieveNegotiateAuthPackage(&ulAuthPackage);
					if (SUCCEEDED(hr))
					{
						pcpcs->ulAuthenticationPackage = ulAuthPackage;
						pcpcs->clsidCredentialProvider = CLSID_NFCRingProvider;

						// At this point the credential has created the serialized credential used for logon
						// By setting this to CPGSR_RETURN_CREDENTIAL_FINISHED we are letting logonUI know
						// that we have all the information we need and it should attempt to submit the 
						// serialized credential.
						*pcpgsr = CPGSR_RETURN_CREDENTIAL_FINISHED;
					}
				}
			}

			CoTaskMemFree(pwzProtectedPassword);
		}
	}
	else
	{
		DWORD dwErr = GetLastError();
		hr = HRESULT_FROM_WIN32(dwErr);
	}
	return hr;
}

ULONG Reader::GetDataForNFCToken(std::wstring path, std::wstring* username, std::wstring* password)
{
	HKEY hKey;
	WCHAR uszBuffer[512];
	DWORD udwBufferSize = sizeof(uszBuffer);
	WCHAR pszBuffer[512];
	DWORD pdwBufferSize = sizeof(pszBuffer);
	ULONG lRes = RegOpenKeyExW(HKEY_LOCAL_MACHINE, path.c_str(), 0, KEY_READ, &hKey);
	if (lRes == ERROR_SUCCESS)
	{
		lRes = RegQueryValueEx(hKey, L"User", 0, NULL, (LPBYTE)uszBuffer, &udwBufferSize);
		if (lRes == ERROR_SUCCESS)
		{
			*username = uszBuffer;

			// only continue if the NFC token and username existed
			//lRes = RegOpenKeyExW(HKEY_LOCAL_MACHINE, path.c_str(), 0, KEY_READ, &hKey);
			if (lRes == ERROR_SUCCESS)
			{
				lRes = RegQueryValueEx(hKey, L"Data", 0, NULL, (LPBYTE)pszBuffer, &pdwBufferSize);
				if (lRes == ERROR_SUCCESS)
					*password = pszBuffer;
			}
		}
	}
	return lRes;
}
