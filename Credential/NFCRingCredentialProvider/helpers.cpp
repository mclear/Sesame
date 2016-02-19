//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) 2006 Microsoft Corporation. All rights reserved.
//
// Helper functions for copying parameters and packaging the buffer
// for GetSerialization.


#include "helpers.h"
#include <intsafe.h>
#include <wincred.h>

// 
// Copies the field descriptor pointed to by rcpfd into a buffer allocated 
// using CoTaskMemAlloc. Returns that buffer in ppcpfd.
// 
HRESULT FieldDescriptorCoAllocCopy(
	const CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR& rcpfd,
	CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR** ppcpfd
	)
{
	HRESULT hr;
	DWORD cbStruct = sizeof(CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR);

	CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR* pcpfd =
		(CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR*)CoTaskMemAlloc(cbStruct);

	if (pcpfd)
	{
		pcpfd->dwFieldID = rcpfd.dwFieldID;
		pcpfd->cpft = rcpfd.cpft;

		if (rcpfd.pszLabel)
		{
			hr = SHStrDupW(rcpfd.pszLabel, &pcpfd->pszLabel);
		}
		else
		{
			pcpfd->pszLabel = NULL;
			hr = S_OK;
		}
	}
	else
	{
		hr = E_OUTOFMEMORY;
	}
	if (SUCCEEDED(hr))
	{
		*ppcpfd = pcpfd;
	}
	else
	{
		CoTaskMemFree(pcpfd);
		*ppcpfd = NULL;
	}


	return hr;
}

//
// Coppies rcpfd into the buffer pointed to by pcpfd. The caller is responsible for
// allocating pcpfd. This function uses CoTaskMemAlloc to allocate memory for 
// pcpfd->pszLabel.
//
HRESULT FieldDescriptorCopy(
	const CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR& rcpfd,
	CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR* pcpfd
	)
{
	HRESULT hr;
	CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR cpfd;

	cpfd.dwFieldID = rcpfd.dwFieldID;
	cpfd.cpft = rcpfd.cpft;

	if (rcpfd.pszLabel)
	{
		hr = SHStrDupW(rcpfd.pszLabel, &cpfd.pszLabel);
	}
	else
	{
		cpfd.pszLabel = NULL;
		hr = S_OK;
	}

	if (SUCCEEDED(hr))
	{
		*pcpfd = cpfd;
	}

	return hr;
}

//
// This function copies the length of pwz and the pointer pwz into the UNICODE_STRING structure
// This function is intended for serializing a credential in GetSerialization only.
// Note that this function just makes a copy of the string pointer. It DOES NOT ALLOCATE storage!
// Be very, very sure that this is what you want, because it probably isn't outside of the
// exact GetSerialization call where the sample uses it.
//
HRESULT UnicodeStringInitWithString(
	PWSTR pwz,
	UNICODE_STRING* pus
	)
{
	HRESULT hr;
	if (pwz)
	{
		size_t lenString;
		hr = StringCchLengthW(pwz, USHORT_MAX, &(lenString));

		if (SUCCEEDED(hr))
		{
			USHORT usCharCount;
			hr = SizeTToUShort(lenString, &usCharCount);
			if (SUCCEEDED(hr))
			{
				USHORT usSize;
				hr = SizeTToUShort(sizeof(WCHAR), &usSize);
				if (SUCCEEDED(hr))
				{
					hr = UShortMult(usCharCount, usSize, &(pus->Length)); // Explicitly NOT including NULL terminator
					if (SUCCEEDED(hr))
					{
						pus->MaximumLength = pus->Length;
						pus->Buffer = pwz;
						hr = S_OK;
					}
					else
					{
						hr = HRESULT_FROM_WIN32(ERROR_ARITHMETIC_OVERFLOW);
					}
				}
			}
		}
	}
	else
	{
		hr = E_INVALIDARG;
	}
	return hr;
}

//
// The following function is intended to be used ONLY with the Kerb*Pack functions.  It does
// no bounds-checking because its callers have precise requirements and are written to respect 
// its limitations.
// You can read more about the UNICODE_STRING type at:
// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/secauthn/security/unicode_string.asp
//
static void _UnicodeStringPackedUnicodeStringCopy(
	const UNICODE_STRING& rus,
	PWSTR pwzBuffer,
	UNICODE_STRING* pus
	)
{
	pus->Length = rus.Length;
	pus->MaximumLength = rus.Length;
	pus->Buffer = pwzBuffer;

	CopyMemory(pus->Buffer, rus.Buffer, pus->Length);
}

//
// Initialize the members of a KERB_INTERACTIVE_UNLOCK_LOGON with weak references to the
// passed-in strings.  This is useful if you will later use KerbInteractiveUnlockLogonPack
// to serialize the structure.  
//
// The password is stored in encrypted form for CPUS_LOGON and CPUS_UNLOCK_WORKSTATION
// because the system can accept encrypted credentials.  It is not encrypted in CPUS_CREDUI
// because we cannot know whether our caller can accept encrypted credentials.
//
HRESULT KerbInteractiveUnlockLogonInit(
	PWSTR pwzDomain,
	PWSTR pwzUsername,
	PWSTR pwzPassword,
	CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus,
	KERB_INTERACTIVE_UNLOCK_LOGON* pkiul
	)
{
	KERB_INTERACTIVE_UNLOCK_LOGON kiul;
	ZeroMemory(&kiul, sizeof(kiul));

	KERB_INTERACTIVE_LOGON* pkil = &kiul.Logon;

	// Note: this method uses custom logic to pack a KERB_INTERACTIVE_UNLOCK_LOGON with a
	// serialized credential.  We could replace the calls to UnicodeStringInitWithString
	// and KerbInteractiveUnlockLogonPack with a single cal to CredPackAuthenticationBuffer,
	// but that API has a drawback: it returns a KERB_INTERACTIVE_UNLOCK_LOGON whose
	// MessageType is always KerbInteractiveLogon.  
	//
	// If we only handled CPUS_LOGON, this drawback would not be a problem.  For 
	// CPUS_UNLOCK_WORKSTATION, we could cast the output buffer of CredPackAuthenticationBuffer
	// to KERB_INTERACTIVE_UNLOCK_LOGON and modify the MessageType to KerbWorkstationUnlockLogon,
	// but such a cast would be unsupported -- the output format of CredPackAuthenticationBuffer
	// is not officially documented.

	// Initialize the UNICODE_STRINGS to share our username and password strings.
	HRESULT hr = UnicodeStringInitWithString(pwzDomain, &pkil->LogonDomainName);

	if (SUCCEEDED(hr))
	{
		hr = UnicodeStringInitWithString(pwzUsername, &pkil->UserName);

		if (SUCCEEDED(hr))
		{
			if (SUCCEEDED(hr))
			{
				hr = UnicodeStringInitWithString(pwzPassword, &pkil->Password);
			}

			if (SUCCEEDED(hr))
			{
				// Set a MessageType based on the usage scenario.
				switch (cpus)
				{
				case CPUS_UNLOCK_WORKSTATION:
					pkil->MessageType = KerbWorkstationUnlockLogon;
					hr = S_OK;
					break;

				case CPUS_LOGON:
					pkil->MessageType = KerbInteractiveLogon;
					hr = S_OK;
					break;

				case CPUS_CREDUI:
					pkil->MessageType = (KERB_LOGON_SUBMIT_TYPE)0; // MessageType does not apply to CredUI
					hr = S_OK;
					break;

				default:
					hr = E_FAIL;
					break;
				}

				if (SUCCEEDED(hr))
				{
					// KERB_INTERACTIVE_UNLOCK_LOGON is just a series of structures.  A
					// flat copy will properly initialize the output parameter.
					CopyMemory(pkiul, &kiul, sizeof(*pkiul));
				}
			}
		}
	}

	return hr;
}

//
// WinLogon and LSA consume "packed" KERB_INTERACTIVE_UNLOCK_LOGONs.  In these, the PWSTR members of each
// UNICODE_STRING are not actually pointers but byte offsets into the overall buffer represented
// by the packed KERB_INTERACTIVE_UNLOCK_LOGON.  For example:
// 
// rkiulIn.Logon.LogonDomainName.Length = 14                                    -> Length is in bytes, not characters
// rkiulIn.Logon.LogonDomainName.Buffer = sizeof(KERB_INTERACTIVE_UNLOCK_LOGON) -> LogonDomainName begins immediately
//                                                                              after the KERB_... struct in the buffer
// rkiulIn.Logon.UserName.Length = 10
// rkiulIn.Logon.UserName.Buffer = sizeof(KERB_INTERACTIVE_UNLOCK_LOGON) + 14   -> UNICODE_STRINGS are NOT null-terminated
//
// rkiulIn.Logon.Password.Length = 16
// rkiulIn.Logon.Password.Buffer = sizeof(KERB_INTERACTIVE_UNLOCK_LOGON) + 14 + 10
// 
// THere's more information on this at:
// http://msdn.microsoft.com/msdnmag/issues/05/06/SecurityBriefs/#void
//

HRESULT KerbInteractiveUnlockLogonPack(
	const KERB_INTERACTIVE_UNLOCK_LOGON& rkiulIn,
	BYTE** prgb,
	DWORD* pcb
	)
{
	HRESULT hr;

	const KERB_INTERACTIVE_LOGON* pkilIn = &rkiulIn.Logon;

	// alloc space for struct plus extra for the three strings
	DWORD cb = sizeof(rkiulIn) +
		pkilIn->LogonDomainName.Length +
		pkilIn->UserName.Length +
		pkilIn->Password.Length;

	KERB_INTERACTIVE_UNLOCK_LOGON* pkiulOut = (KERB_INTERACTIVE_UNLOCK_LOGON*)CoTaskMemAlloc(cb);

	if (pkiulOut)
	{
		ZeroMemory(&pkiulOut->LogonId, sizeof(LUID));

		//
		// point pbBuffer at the beginning of the extra space
		//
		BYTE* pbBuffer = (BYTE*)pkiulOut + sizeof(*pkiulOut);

		//
		// set up the Logon structure within the KERB_INTERACTIVE_UNLOCK_LOGON
		//
		KERB_INTERACTIVE_LOGON* pkilOut = &pkiulOut->Logon;

		pkilOut->MessageType = pkilIn->MessageType;

		//
		// copy each string,
		// fix up appropriate buffer pointer to be offset,
		// advance buffer pointer over copied characters in extra space
		//
		_UnicodeStringPackedUnicodeStringCopy(pkilIn->LogonDomainName, (PWSTR)pbBuffer, &pkilOut->LogonDomainName);
		pkilOut->LogonDomainName.Buffer = (PWSTR)(pbBuffer - (BYTE*)pkiulOut);
		pbBuffer += pkilOut->LogonDomainName.Length;

		_UnicodeStringPackedUnicodeStringCopy(pkilIn->UserName, (PWSTR)pbBuffer, &pkilOut->UserName);
		pkilOut->UserName.Buffer = (PWSTR)(pbBuffer - (BYTE*)pkiulOut);
		pbBuffer += pkilOut->UserName.Length;

		_UnicodeStringPackedUnicodeStringCopy(pkilIn->Password, (PWSTR)pbBuffer, &pkilOut->Password);
		pkilOut->Password.Buffer = (PWSTR)(pbBuffer - (BYTE*)pkiulOut);

		*prgb = (BYTE*)pkiulOut;
		*pcb = cb;

		hr = S_OK;
	}
	else
	{
		hr = E_OUTOFMEMORY;
	}

	return hr;
}

// 
// This function packs the string pszSourceString in pszDestinationString
// for use with LSA functions including LsaLookupAuthenticationPackage.
//
HRESULT LsaInitString(PSTRING pszDestinationString, PCSTR pszSourceString)
{
	size_t cchLength;
	HRESULT hr = StringCchLength(pszSourceString, USHORT_MAX, &cchLength);
	if (SUCCEEDED(hr))
	{
		USHORT usLength;
		hr = SizeTToUShort(cchLength, &usLength);

		if (SUCCEEDED(hr))
		{
			pszDestinationString->Buffer = (PCHAR)pszSourceString;
			pszDestinationString->Length = usLength;
			pszDestinationString->MaximumLength = pszDestinationString->Length + 1;
			hr = S_OK;
		}
	}
	return hr;
}

//
// Retrieves the 'negotiate' AuthPackage from the LSA. In this case, Kerberos
// For more information on auth packages see this msdn page:
// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/secauthn/security/msv1_0_lm20_logon.asp
//
HRESULT RetrieveNegotiateAuthPackage(ULONG * pulAuthPackage)
{
	HRESULT hr;
	HANDLE hLsa;

	NTSTATUS status = LsaConnectUntrusted(&hLsa);
	if (SUCCEEDED(HRESULT_FROM_NT(status)))
	{

		ULONG ulAuthPackage;
		LSA_STRING lsaszKerberosName;
		LsaInitString(&lsaszKerberosName, NEGOSSP_NAME);

		status = LsaLookupAuthenticationPackage(hLsa, &lsaszKerberosName, &ulAuthPackage);
		if (SUCCEEDED(HRESULT_FROM_NT(status)))
		{
			*pulAuthPackage = ulAuthPackage;
			hr = S_OK;
		}
		else
		{
			hr = HRESULT_FROM_NT(status);
		}
		LsaDeregisterLogonProcess(hLsa);
	}
	else
	{
		hr = HRESULT_FROM_NT(status);
	}

	return hr;
}

//
// Return a copy of pwzToProtect encrypted with the CredProtect API.
//
// pwzToProtect must not be NULL or the empty string.
//
static HRESULT ProtectAndCopyString(
	PWSTR pwzToProtect,
	PWSTR* ppwzProtected
	)
{
	*ppwzProtected = NULL;

	HRESULT hr = E_FAIL;

	// The first call to CredProtect determines the length of the encrypted string.
	// Because we pass a NULL output buffer, we expect the call to fail.
	//
	// Note that the third parameter to CredProtect, the number of characters of pwzToProtect
	// to encrypt, must include the NULL terminator!
	DWORD cchProtected = 0;
	if (!CredProtectW(FALSE, pwzToProtect, (DWORD)wcslen(pwzToProtect) + 1, NULL, &cchProtected, NULL))
	{
		DWORD dwErr = GetLastError();

		if ((ERROR_INSUFFICIENT_BUFFER == dwErr) && (0 < cchProtected))
		{
			// Allocate a buffer long enough for the encrypted string.
			PWSTR pwzProtected = (PWSTR)CoTaskMemAlloc(cchProtected * sizeof(WCHAR));

			if (pwzProtected)
			{
				// The second call to CredProtect actually encrypts the string.
				if (CredProtectW(FALSE, pwzToProtect, (DWORD)wcslen(pwzToProtect) + 1, pwzProtected, &cchProtected, NULL))
				{
					*ppwzProtected = pwzProtected;
					hr = S_OK;
				}
				else
				{
					CoTaskMemFree(pwzProtected);

					dwErr = GetLastError();
					hr = HRESULT_FROM_WIN32(dwErr);
				}
			}
			else
			{
				hr = E_OUTOFMEMORY;
			}
		}
		else
		{
			hr = HRESULT_FROM_WIN32(dwErr);
		}
	}

	return hr;
}

//
// If pwzPassword should be encrypted, return a copy encrypted with CredProtect.
// 
// If not, just return a copy.
//
HRESULT ProtectIfNecessaryAndCopyPassword(
	PWSTR pwzPassword,
	CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus,
	PWSTR* ppwzProtectedPassword
	)
{
	*ppwzProtectedPassword = NULL;

	HRESULT hr;

	// ProtectAndCopyString is intended for non-empty strings only.  Empty passwords
	// do not need to be encrypted.
	if (pwzPassword && *pwzPassword)
	{
		bool bCredAlreadyEncrypted = false;
		CRED_PROTECTION_TYPE protectionType;

		// If the password is already encrypted, we should not encrypt it again.
		// An encrypted password may be received through SetSerialization in the 
		// CPUS_LOGON scenario during a Terminal Services connection, for instance.
		if (CredIsProtectedW(pwzPassword, &protectionType))
		{
			if (CredUnprotected != protectionType)
			{
				bCredAlreadyEncrypted = true;
			}
		}

		// Passwords should not be encrypted in the CPUS_CREDUI scenario.  We
		// cannot know if our caller expects or can handle an encryped password.
		if (CPUS_CREDUI == cpus || bCredAlreadyEncrypted)
		{
			hr = SHStrDupW(pwzPassword, ppwzProtectedPassword);
		}
		else
		{
			hr = ProtectAndCopyString(pwzPassword, ppwzProtectedPassword);
		}
	}
	else
	{
		hr = SHStrDupW(L"", ppwzProtectedPassword);
	}

	return hr;
}
