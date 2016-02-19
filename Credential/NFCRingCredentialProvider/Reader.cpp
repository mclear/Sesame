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

	// this is where we'd start the thread to check for a valid ring
	_readerThread = std::thread(&Reader::CheckNFC, this);

	return hr;
}

void Reader::CheckNFC()
{
	LONG rv;

	SCARDCONTEXT hContext;
	LPTSTR mszReaders;
	SCARDHANDLE hCard;
	DWORD dwReaders, dwActiveProtocol, dwRecvLength;

	SCARD_IO_REQUEST pioSendPci;
	BYTE pbRecvBuffer[50];
	BYTE cmd1[] = { 0xFF, 0xCA, 0x00, 0x00, 0x00 };
	SHA1 sha1;
	std::string hashed = "";
	std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> converter;
	// if all readers have come back empty
	bool seenallblank = false;

	while (_checkLoop)
	{
		try
		{
			bool thispassallblank = true;
			std::this_thread::sleep_for(std::chrono::milliseconds(100));
			// get a context
			rv = SCardEstablishContext(SCARD_SCOPE_SYSTEM, NULL, NULL, &hContext);
			if (rv == SCARD_S_SUCCESS)
			{
#ifdef SCARD_AUTOALLOCATE
				dwReaders = SCARD_AUTOALLOCATE;

				rv = SCardListReaders(hContext, NULL, (LPTSTR)&mszReaders, &dwReaders);
#else
				rv = SCardListReaders(hContext, NULL, NULL, &dwReaders);
				if (rv == SCARD_S_SUCCESS)
				{
					mszReaders = calloc(dwReaders, sizeof(char));
					rv = SCardListReaders(hContext, NULL, mszReaders, &dwReaders);
				}
#endif
				if (rv == SCARD_S_SUCCESS)
				{
					for (LPTSTR pszz = mszReaders; *pszz; pszz += lstrlen(pszz) + 1)
					{
						try
						{
							rv = SCardConnect(hContext, pszz, SCARD_SHARE_SHARED,
								SCARD_PROTOCOL_T0 | SCARD_PROTOCOL_T1, &hCard, &dwActiveProtocol);
							if (rv == SCARD_S_SUCCESS)
							{
								thispassallblank = false;
								// token found
								if (seenallblank)
								{
									switch (dwActiveProtocol)
									{
									case SCARD_PROTOCOL_T0:
										pioSendPci = *SCARD_PCI_T0;
										break;

									case SCARD_PROTOCOL_T1:
										pioSendPci = *SCARD_PCI_T1;
										break;
									}
									dwRecvLength = sizeof(pbRecvBuffer);
									rv = SCardTransmit(hCard, &pioSendPci, cmd1, sizeof(cmd1),
										NULL, pbRecvBuffer, &dwRecvLength);
									if (rv == SCARD_S_SUCCESS)
									{
										// got data from a card
										_kerbrosCredentialRetrieved = false;
										_key = L"";

										char hex[108];
										hex[dwRecvLength * 2] = '\0';

										for (int i = 0; i < dwRecvLength; i++)
											sprintf(&hex[2 * i], "%02X ", pbRecvBuffer[i]);

										sprintf(&hex[2 * dwRecvLength], "%s", "02164873");

										hashed = sha1(hex, (dwRecvLength * 2) + 8);
										hashed = sha1(hashed);
										_key = converter.from_bytes(hashed);

										_kerbrosCredentialRetrieved = true;

										// fire "CredentialsChanged" event
										if (_pProvider != NULL)
											_pProvider->OnNFCStatusChanged();
									}
								}
								rv = SCardDisconnect(hCard, SCARD_LEAVE_CARD);
							}
						}
						catch (...)
						{
							// getting a card from a reader failed. thats bad right?
						}
					}
					if (thispassallblank)
						seenallblank = true;
#ifdef SCARD_AUTOALLOCATE
					rv = SCardFreeMemory(hContext, mszReaders);
#else
					free(mszReaders);
#endif

				}
				// clean up context
				rv = SCardReleaseContext(hContext);
			}
			// dont need to do anything
		}
		catch (...)
		{
			// getting a context or list of readers errored
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
