//#ifdef WIN32
//#undef UNICODE
//#endif

#include "Wrapper.h"
extern "C"
{
	//__declspec(dllexport) int __cdecl CredProtectWrapper(wchar_t* inputBuffer, long inputLength,  wchar_t* outputBuffer)
	//{
	//	PWSTR pwzToProtect = inputBuffer;
	//	PWSTR* ppwzProtected;
	//	*ppwzProtected = NULL;

	//	HRESULT hr = E_FAIL;
	//	int len = 0;
	//	// The first call to CredProtect determines the length of the encrypted string.
	//	// Because we pass a NULL output buffer, we expect the call to fail.
	//	//
	//	// Note that the third parameter to CredProtect, the number of characters of pwzToProtect
	//	// to encrypt, must include the NULL terminator!
	//	DWORD cchProtected = 0;
	//	if (!CredProtectW(FALSE, pwzToProtect, (DWORD)wcslen(pwzToProtect) + 1, NULL, &cchProtected, NULL))
	//	{
	//		DWORD dwErr = GetLastError();

	//		if ((ERROR_INSUFFICIENT_BUFFER == dwErr) && (0 < cchProtected))
	//		{
	//			// Allocate a buffer long enough for the encrypted string.
	//			PWSTR pwzProtected = (PWSTR)CoTaskMemAlloc(cchProtected * sizeof(WCHAR));

	//			if (pwzProtected)
	//			{
	//				// The second call to CredProtect actually encrypts the string.
	//				if (CredProtectW(FALSE, pwzToProtect, (DWORD)wcslen(pwzToProtect) + 1, pwzProtected, &cchProtected, NULL))
	//				{
	//					*ppwzProtected = pwzProtected;
	//					memcpy(outputBuffer, pwzProtected, cchProtected*2);
	//					outputBuffer[cchProtected] = '\0';
	//					//for (int i = 0; i < cchProtected; i++)
	//					//{
	//					//	outputBuffer[i * 2] = pwzProtected[i];
	//					//}
	//					len = cchProtected;
	//					hr = S_OK;
	//				}
	//				else
	//				{
	//					CoTaskMemFree(pwzProtected);

	//					dwErr = GetLastError();
	//					hr = HRESULT_FROM_WIN32(dwErr);
	//				}
	//			}
	//			else
	//			{
	//				hr = E_OUTOFMEMORY;
	//			}
	//		}
	//		else
	//		{
	//			hr = HRESULT_FROM_WIN32(dwErr);
	//		}
	//	}

	//	if (hr == S_OK)
	//		return len;
	//}
	std::string int_to_hex(LONG i)
	{
		std::stringstream stream;
		stream << "0x" << std::hex << i;
		return stream.str();
	}

	__declspec(dllexport) int __cdecl PCSC_GetID(char* outputBuffer, char* errorCode)
	{
		LONG rv;

		SCARDCONTEXT hContext;
		LPTSTR mszReaders;
		SCARDHANDLE hCard;
		DWORD dwReaders, dwActiveProtocol, dwRecvLength;

		SCARD_IO_REQUEST pioSendPci;
		BYTE pbRecvBuffer[49];
		BYTE cmd1[] = { 0xFF, 0xCA, 0x00, 0x00, 0x00 };
		errorCode[0] = '\0'; // i've preallocated 500 bytes in the .net side but use a string operation to read this back so needs a null
		int resultLen = 0;
		try
		{
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
								// token found
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
									char hex[108];
									hex[dwRecvLength * 2] = '\0';

									for (int i = 0; i < dwRecvLength; i++)
										sprintf(&hex[2 * i], "%02X ", pbRecvBuffer[i]);

									memcpy(outputBuffer, hex, (dwRecvLength * 2) + 1);
									resultLen = dwRecvLength * 2;
									errorCode[0] = '\0'; // no error on this card. call it a success
								}
								else
								{
									std::string instr = int_to_hex(rv);
									memcpy(errorCode, instr.c_str(), instr.length() + 1);

									resultLen = -4;
								}
								rv = SCardDisconnect(hCard, SCARD_LEAVE_CARD);
							}
							else
							{
								std::string instr = int_to_hex(rv);
								memcpy(errorCode, instr.c_str(), instr.length() + 1);

								resultLen = -3;
							}
							if (resultLen > 0)
								break;
						}
						catch (...)
						{
						}
					}
#ifdef SCARD_AUTOALLOCATE
					rv = SCardFreeMemory(hContext, mszReaders);
#else
					free(mszReaders);
#endif

				}
				else
				{
					std::string instr = int_to_hex(rv);
					memcpy(errorCode, instr.c_str(), instr.length() + 1);

					if (rv == SCARD_E_READER_UNAVAILABLE)
						resultLen = -5;
					else if (rv == SCARD_E_NO_READERS_AVAILABLE)
						resultLen = -6;
				}
				// clean up context
				rv = SCardReleaseContext(hContext);
			}
			else
			{
				resultLen = -1;
			}
			// dont need to do anything
			return resultLen;
		}
		catch (...)
		{
			return -111;
		}
	}
}