//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) 2006 Microsoft Corporation. All rights reserved.
//
//

#ifndef WIN32_NO_STATUS
#include <ntstatus.h>
#define WIN32_NO_STATUS
#endif

#include <tchar.h>
#include "NFCCredential.h"

#ifdef WIN32
#undef UNICODE
#endif

#include <stdio.h>
#include <stdlib.h>

#include <winscard.h>
#include "log.h"

#ifdef WIN32
static char *pcsc_stringify_error(LONG rv)
{
	static char out[20];
	sprintf_s(out, sizeof(out), "0x%08X", rv);

	return out;
}
#endif

#define CHECK(f, rv) \
 if (SCARD_S_SUCCESS != rv) \
   { \
  printf(f ": %s\n", pcsc_stringify_error(rv)); \
  return -1; \
   }

extern HINSTANCE hInstance;

NFCCredential::NFCCredential() :
	_cRef(1),
	_pCredProvCredentialEvents(NULL)
{
	MAZ_LOG(LogMessageType::Information, "NFCCredential::Constructor");

	DllAddRef();
	_reader = NULL;
	ZeroMemory(_rgCredProvFieldDescriptors, sizeof(_rgCredProvFieldDescriptors));
	ZeroMemory(_rgFieldStatePairs, sizeof(_rgFieldStatePairs));
	ZeroMemory(_rgFieldStrings, sizeof(_rgFieldStrings));
}

NFCCredential::~NFCCredential()
{
	MAZ_LOG(LogMessageType::Information, "NFCCredential::Destructor");

	for (int i = 0; i < ARRAYSIZE(_rgFieldStrings); i++)
	{
		CoTaskMemFree(_rgFieldStrings[i]);
		CoTaskMemFree(_rgCredProvFieldDescriptors[i].pszLabel);
	}

	DllRelease();
}

// Initializes one credential with the field information passed in.
// Set the value of the SFI_FIMTITLE field to pwzUsername.
// Optionally takes a password for the SetSerialization case.
HRESULT NFCCredential::Initialize(
	CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus,
	const CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR* rgcpfd,
	const FIELD_STATE_PAIR* rgfsp,
	Reader* rdr
	)
{
	MAZ_LOG(LogMessageType::Information, "NFCCredential::Initialize");

	HRESULT hr = S_OK;

	_reader = rdr;
	_cpus = cpus;

	// Copy the field descriptors for each field. This is useful if you want to vary the 
	// field descriptors based on what Usage scenario the credential was created for.
	for (DWORD i = 0; SUCCEEDED(hr) && i < ARRAYSIZE(_rgCredProvFieldDescriptors); i++)
	{
		_rgFieldStatePairs[i] = rgfsp[i];
		hr = FieldDescriptorCopy(rgcpfd[i], &_rgCredProvFieldDescriptors[i]);
	}

	// Initialize the String values of all the fields.
	if (SUCCEEDED(hr))
	{
		hr = SHStrDupW(L"NFC Ring Login", &_rgFieldStrings[SFI_FIMTITLE]);
	}
	if (SUCCEEDED(hr))
	{
		hr = SHStrDupW(L"Submit", &_rgFieldStrings[SFI_SUBMIT_BUTTON]);
	}

	return S_OK;
}

// LogonUI calls this in order to give us a callback in case we need to notify it of anything.
HRESULT NFCCredential::Advise(
	ICredentialProviderCredentialEvents* pcpce
	)
{
	MAZ_LOG(LogMessageType::Information, "NFCCredential::Advise");

	if (_pCredProvCredentialEvents != NULL)
	{
		_pCredProvCredentialEvents->Release();
	}
	_pCredProvCredentialEvents = pcpce;
	_pCredProvCredentialEvents->AddRef();

	return S_OK;
}

// LogonUI calls this to tell us to release the callback.
HRESULT NFCCredential::UnAdvise()
{
	MAZ_LOG(LogMessageType::Information, "NFCCredential::UnAdvise");

	if (_pCredProvCredentialEvents)
	{
		_pCredProvCredentialEvents->Release();
	}
	_pCredProvCredentialEvents = NULL;

	return S_OK;
}

// LogonUI calls this function when our tile is selected (zoomed).
// If you simply want fields to show/hide based on the selected state,
// there's no need to do anything here - you can set that up in the 
// field definitions.  But if you want to do something
// more complicated, like change the contents of a field when the tile is
// selected, you would do it here.
HRESULT NFCCredential::SetSelected(BOOL* pbAutoLogon)
{
	MAZ_LOG(LogMessageType::Information, "NFCCredential::SetSelected");

	*pbAutoLogon = TRUE;  

	return S_OK;
}

// Similarly to SetSelected, LogonUI calls this when your tile was selected
// and now no longer is. The most common thing to do here (which we do below)
// is to clear out the password field.
HRESULT NFCCredential::SetDeselected()
{
	MAZ_LOG(LogMessageType::Information, "NFCCredential::SetDeselected");

	// kill thread here
	HRESULT hr = S_OK;

	//if (_rgFieldStrings[SFI_USERNAME])
	//{
	//	//CoTaskMemFree(_rgFieldStrings[SFI_USERNAME]);
	//	hr = wcscpy_s(_rgFieldStrings[SFI_USERNAME], wcslen(L"") + 1, L"");

	//	if (SUCCEEDED(hr) && _pCredProvCredentialEvents)
	//	{
	//		_pCredProvCredentialEvents->SetFieldString(this, SFI_USERNAME, _rgFieldStrings[SFI_USERNAME]);
	//	}
	//}

	return hr;
}

// Gets info for a particular field of a tile. Called by logonUI to get information to 
// display the tile.
HRESULT NFCCredential::GetFieldState(
	DWORD dwFieldID,
	CREDENTIAL_PROVIDER_FIELD_STATE* pcpfs,
	CREDENTIAL_PROVIDER_FIELD_INTERACTIVE_STATE* pcpfis
	)
{
	MAZ_LOG(LogMessageType::Information, "NFCCredential::GetFieldState");

	HRESULT hr;

	// Validate paramters.
	if ((dwFieldID < ARRAYSIZE(_rgFieldStatePairs)) && pcpfs && pcpfis)
	{
		*pcpfs = _rgFieldStatePairs[dwFieldID].cpfs;
		*pcpfis = _rgFieldStatePairs[dwFieldID].cpfis;

		hr = S_OK;
	}
	else
	{
		hr = E_INVALIDARG;
	}

	return hr;
}

// Sets ppwsz to the string value of the field at the index dwFieldID.
HRESULT NFCCredential::GetStringValue(
	DWORD dwFieldID, 
	PWSTR* ppwsz
	)
{
	MAZ_LOG(LogMessageType::Information, "NFCCredential::GetStringValue");

	HRESULT hr;

	// Check to make sure dwFieldID is a legitimate index.
	if (dwFieldID < ARRAYSIZE(_rgCredProvFieldDescriptors) && ppwsz) 
	{
		// Make a copy of the string and return that. The caller
		// is responsible for freeing it.
		hr = SHStrDupW(_rgFieldStrings[dwFieldID], ppwsz);
	}
	else
	{
		hr = E_INVALIDARG;
	}

	return hr;
}

// Gets the image to show in the user tile.
HRESULT NFCCredential::GetBitmapValue(
	DWORD dwFieldID, 
	HBITMAP* phbmp
	)
{
	MAZ_LOG(LogMessageType::Information, "NFCCredential::GetBitmapValue");

	HRESULT hr;

	// Validate paramters.
	if ((SFI_TILEIMAGE == dwFieldID) && phbmp)
	{
		HBITMAP hbmp = LoadBitmap(HINST_THISDLL, MAKEINTRESOURCE(IDB_TILE_IMAGE));
		if (hbmp != NULL)
		{
			hr = S_OK;
			*phbmp = hbmp;
		}
		else
		{
			hr = HRESULT_FROM_WIN32(GetLastError());
		}
	}
	else
	{
		hr = E_INVALIDARG;
	}

	return hr;
}

// Sets pdwAdjacentTo to the index of the field the submit button should be 
// adjacent to. We recommend that the submit button is placed next to the last
// field which the user is required to enter information in. Optional fields
// should be below the submit button.
HRESULT NFCCredential::GetSubmitButtonValue(
	DWORD dwFieldID,
	DWORD* pdwAdjacentTo
	)
{
	MAZ_LOG(LogMessageType::Information, "NFCCredential::GetSubmitButtonValue");

	HRESULT hr;

	// Validate parameters.
	if ((SFI_SUBMIT_BUTTON == dwFieldID) && pdwAdjacentTo)
	{
		// pdwAdjacentTo is a pointer to the fieldID you want the submit button to appear next to.
		*pdwAdjacentTo = SFI_FIMTITLE;
		hr = S_OK;
	}
	else
	{
		hr = E_INVALIDARG;
	}

	return hr;
}

// Sets the value of a field which can accept a string as a value.
// This is called on each keystroke when a user types into an edit field.
HRESULT NFCCredential::SetStringValue(
	DWORD dwFieldID, 
	PCWSTR pwz      
	)
{
	MAZ_LOG(LogMessageType::Information, "NFCCredential::SetStringValue");

	HRESULT hr;

	// Validate parameters.
	if (dwFieldID < ARRAYSIZE(_rgCredProvFieldDescriptors) && 
		(CPFT_EDIT_TEXT == _rgCredProvFieldDescriptors[dwFieldID].cpft || 
		CPFT_PASSWORD_TEXT == _rgCredProvFieldDescriptors[dwFieldID].cpft)) 
	{
		PWSTR* ppwszStored = &_rgFieldStrings[dwFieldID];
		CoTaskMemFree(*ppwszStored);
		hr = SHStrDupW(pwz, ppwszStored);
	}
	else
	{
		hr = E_INVALIDARG;
	}

	return hr;
}

//------------- 
// The following methods are for logonUI to get the values of various UI elements and then communicate
// to the credential about what the user did in that field.  However, these methods are not implemented
// because our tile doesn't contain these types of UI elements
HRESULT NFCCredential::GetCheckboxValue(
	DWORD dwFieldID, 
	BOOL* pbChecked,
	PWSTR* ppwszLabel
	)
{
	MAZ_LOG(LogMessageType::Information, "NFCCredential::GetCheckboxValue");

	UNREFERENCED_PARAMETER(dwFieldID);
	UNREFERENCED_PARAMETER(pbChecked);
	UNREFERENCED_PARAMETER(ppwszLabel);

	return E_NOTIMPL;
}

HRESULT NFCCredential::GetComboBoxValueCount(
	DWORD dwFieldID, 
	DWORD* pcItems, 
	DWORD* pdwSelectedItem
	)
{
	MAZ_LOG(LogMessageType::Information, "NFCCredential::GetComboBoxValueCount");

	UNREFERENCED_PARAMETER(dwFieldID);
	UNREFERENCED_PARAMETER(pcItems);
	UNREFERENCED_PARAMETER(pdwSelectedItem);

	return E_NOTIMPL;
}

HRESULT NFCCredential::GetComboBoxValueAt(
	DWORD dwFieldID, 
	DWORD dwItem,
	PWSTR* ppwszItem
	)
{
	MAZ_LOG(LogMessageType::Information, "NFCCredential::GetComboBoxValueAt");

	UNREFERENCED_PARAMETER(dwFieldID);
	UNREFERENCED_PARAMETER(dwItem);
	UNREFERENCED_PARAMETER(ppwszItem);

	return E_NOTIMPL;
}

HRESULT NFCCredential::SetCheckboxValue(
	DWORD dwFieldID, 
	BOOL bChecked
	)
{
	MAZ_LOG(LogMessageType::Information, "NFCCredential::SetCheckboxValue");

	UNREFERENCED_PARAMETER(dwFieldID);
	UNREFERENCED_PARAMETER(bChecked);

	return E_NOTIMPL;
}

HRESULT NFCCredential::SetComboBoxSelectedValue(
	DWORD dwFieldId,
	DWORD dwSelectedItem
	)
{
	MAZ_LOG(LogMessageType::Information, "NFCCredential::SetComboBoxSelectedValue");

	UNREFERENCED_PARAMETER(dwFieldId);
	UNREFERENCED_PARAMETER(dwSelectedItem);

	return E_NOTIMPL;
}

HRESULT NFCCredential::CommandLinkClicked(DWORD dwFieldID)
{
	MAZ_LOG(LogMessageType::Information, "NFCCredential::CommandLinkClicked");

	UNREFERENCED_PARAMETER(dwFieldID);

	return E_NOTIMPL;
}
//------ end of methods for controls we don't have in our tile ----//


// Collect the username and password into a serialized credential for the correct usage scenario 
// (logon/unlock is what's demonstrated in this sample).  LogonUI then passes these credentials 
// back to the system to log on.
HRESULT NFCCredential::GetSerialization(
	CREDENTIAL_PROVIDER_GET_SERIALIZATION_RESPONSE* pcpgsr,
	CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION* pcpcs, 
	PWSTR* ppwszOptionalStatusText, 
	CREDENTIAL_PROVIDER_STATUS_ICON* pcpsiOptionalStatusIcon
	)
{
	MAZ_LOG(LogMessageType::Information, "NFCCredential::GetSerialization");

	//KERB_INTERACTIVE_LOGON kil;
	//ZeroMemory(&kil, sizeof(kil));

	HRESULT hr = S_OK;

	if (_reader == NULL || !_reader->HasLogin())
		return E_FAIL;

	hr = _reader->GetLogin(pcpgsr, pcpcs, ppwszOptionalStatusText, pcpsiOptionalStatusIcon, _cpus);

	return hr;
}

// ReportResult is completely optional.  Its purpose is to allow a credential to customize the string
// and the icon displayed in the case of a logon failure.  For example, we have chosen to 
// customize the error shown in the case of bad username/password and in the case of the account
// being disabled.
HRESULT NFCCredential::ReportResult(
	NTSTATUS ntsStatus, 
	NTSTATUS ntsSubstatus,
	PWSTR* ppwszOptionalStatusText, 
	CREDENTIAL_PROVIDER_STATUS_ICON* pcpsiOptionalStatusIcon
	)
{
	MAZ_LOG(LogMessageType::Information, "NFCCredential::ReportResult");

	UNREFERENCED_PARAMETER(ntsStatus);
	UNREFERENCED_PARAMETER(ntsSubstatus);
	UNREFERENCED_PARAMETER(ppwszOptionalStatusText);
	UNREFERENCED_PARAMETER(pcpsiOptionalStatusIcon);
	
	return E_NOTIMPL;
}

HRESULT NFCCredential::GetUserSid(LPWSTR *sid)
{
	MAZ_LOG(LogMessageType::Information, "NFCCredential::GetUserSid");

	sid = nullptr;
	return S_FALSE;
}