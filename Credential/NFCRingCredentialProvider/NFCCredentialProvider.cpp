//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) 2006 Microsoft Corporation. All rights reserved.
//
// PasswordResetProvider implements ICredentialProvider, which is the main
// interface that logonUI uses to decide which tiles to display.

#include "Windows.h"
#include "VersionHelpers.h"
#include <credentialprovider.h>
#include "NFCCredential.h"
#include "Reader.h"
#include "guid.h"
#include "log.h"

// NFCCredentialProvider ////////////////////////////////////////////////////////

NFCCredentialProvider::NFCCredentialProvider() :
	_cRef(1)
{
	MAZ_LOG(LogMessageType::Information, "NFCCredentialProvider::Constructor");
	DllAddRef();
	_reader = NULL;
	_pCredential = NULL;
	_credentialProviderEvents = NULL;
}

NFCCredentialProvider::~NFCCredentialProvider()
{
	MAZ_LOG(LogMessageType::Information, "NFCCredentialProvider::Destructor");

	if (_pCredential != NULL)
		_pCredential->Release();
	_pCredential = NULL;

	if (_reader != NULL)
		delete _reader;

	DllRelease();
}

void NFCCredentialProvider::OnNFCStatusChanged()
{
	MAZ_LOG(LogMessageType::Information, "NFCCredentialProvider::OnNFCStatusChanged");

	if (_credentialProviderEvents != NULL)
	{
		_defaultProvider = 0;
		_credentialProviderEvents->CredentialsChanged(_adviseContext);
	}
}

// SetUsageScenario is the provider's cue that it's going to be asked for tiles
// in a subsequent call.  In this sample we have chosen to precreate the credentials 
// for the usage scenario passed in cpus instead of saving off cpus and only creating
// the credentials when we're asked to.
// This sample only handles the logon and unlock scenarios as those are the most common.
HRESULT NFCCredentialProvider::SetUsageScenario(
	CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus,
	DWORD dwFlags
)
{
	UNREFERENCED_PARAMETER(dwFlags);
	MAZ_LOG(LogMessageType::Information, "NFCCredentialProvider::SetUsageScenario");

	HRESULT hr;
	_cpus = cpus;

	// Decide which scenarios to support here. Returning E_NOTIMPL simply tells the caller
	// that we're not designed for that scenario.
	switch (cpus)
	{
	case CPUS_LOGON:
	case CPUS_UNLOCK_WORKSTATION:
		if (!_pCredential && !_reader)
		{
			_pCredential = new NFCCredential();
			if (_pCredential != NULL)
			{
				_reader = new Reader();
				if (_reader != NULL)
				{
					hr = _reader->Initialize(this);

					if (SUCCEEDED(hr))
					{
						hr = _pCredential->Initialize(_cpus, s_rgCredProvFieldDescriptors, s_rgFieldStatePairs, _reader);
					}
					else
					{
						hr = E_ABORT;
					}
				}
				else
				{
					hr = E_OUTOFMEMORY;
				}
			}
			else
			{
				hr = E_OUTOFMEMORY;
			}
			if (FAILED(hr))
			{
				if (_reader != NULL)
				{
					delete _reader;
					_reader = NULL;
				}
				if (_pCredential != NULL)
				{
					_pCredential->Release();
					_pCredential = NULL;
				}
			}
		}
		else
		{
			hr = S_OK;
		}
		break;
	case CPUS_CREDUI:
	case CPUS_CHANGE_PASSWORD:
		hr = E_NOTIMPL;
		break;

	default:
		hr = E_INVALIDARG;
		break;
	}

	return hr;
}

// SetSerialization takes the kind of buffer that you would normally return to LogonUI for
// an authentication attempt.  It's the opposite of ICredentialProviderCredential::GetSerialization.
// GetSerialization is implement by a credential and serializes that credential.  Instead,
// SetSerialization takes the serialization and uses it to create a credential.
//
// SetSerialization is called for two main scenarios.  The first scenario is in the credui case
// where it is prepopulating a tile with credentials that the user chose to store in the OS.
// The second situation is in a remote logon case where the remote client may wish to 
// prepopulate a tile with a username, or in some cases, completely populate the tile and
// use it to logon without showing any UI.
//
// Since this sample doesn't support CPUS_CREDUI, we have not implemented the credui specific
// pieces of this function.  For information on that, please see the credUI sample.
STDMETHODIMP NFCCredentialProvider::SetSerialization(
	const CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION* pcpcs
)
{
	UNREFERENCED_PARAMETER(pcpcs);
	MAZ_LOG(LogMessageType::Information, "NFCCredentialProvider::SetSerialization");

	return E_NOTIMPL;
}

// Called by LogonUI to give you a callback.  Providers often use the callback if they
// some event would cause them to need to change the set of tiles that they enumerated
HRESULT NFCCredentialProvider::Advise(
	ICredentialProviderEvents* pcpe,
	UINT_PTR upAdviseContext
)
{
	MAZ_LOG(LogMessageType::Information, "NFCCredentialProvider::Advise");

	//FILE *filesd;
	//filesd = fopen("C:\\cplog.txt", "a+");
	//fprintf(filesd,"tater\n");
	//fclose(filesd);	

	if (_credentialProviderEvents != NULL)
	{
		_credentialProviderEvents->Release();
	}
	_credentialProviderEvents = pcpe;
	_credentialProviderEvents->AddRef();

	_adviseContext = upAdviseContext;

	if (_reader != NULL)
		_reader->Start();

	return S_OK;
}

// Called by LogonUI when the ICredentialProviderEvents callback is no longer valid.
HRESULT NFCCredentialProvider::UnAdvise()
{
	//FILE *filesd;
	//filesd = fopen("C:\\cplog.txt", "a+");
	//fprintf(filesd,"masher\n");
	//fclose(filesd);	
	MAZ_LOG(LogMessageType::Information, "NFCCredentialProvider::UnAdvise");
	_defaultProvider = CREDENTIAL_PROVIDER_NO_DEFAULT;
	if (_credentialProviderEvents != NULL)
		_credentialProviderEvents->Release();

	_credentialProviderEvents = NULL;

	if (_reader != NULL)
		_reader->Stop();

	return S_OK;
}

// Called by LogonUI to determine the number of fields in your tiles.  This
// does mean that all your tiles must have the same number of fields.
// This number must include both visible and invisible fields. If you want a tile
// to have different fields from the other tiles you enumerate for a given usage
// scenario you must include them all in this count and then hide/show them as desired 
// using the field descriptors.
HRESULT NFCCredentialProvider::GetFieldDescriptorCount(
	DWORD* pdwCount
)
{
	MAZ_LOG(LogMessageType::Information, "NFCCredentialProvider::GetFieldDescriptorCount");

	*pdwCount = SFI_NUM_FIELDS;

	return S_OK;
}

// Gets the field descriptor for a particular field
HRESULT NFCCredentialProvider::GetFieldDescriptorAt(
	DWORD dwIndex,
	CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR** ppcpfd
)
{
	MAZ_LOG(LogMessageType::Information, "NFCCredentialProvider::GetFieldDescriptorAt");

	HRESULT hr;

	// Verify dwIndex is a valid field.
	if ((dwIndex < SFI_NUM_FIELDS) && ppcpfd)
	{
		hr = FieldDescriptorCoAllocCopy(s_rgCredProvFieldDescriptors[dwIndex], ppcpfd);
	}
	else
	{
		hr = E_INVALIDARG;
	}

	return hr;
}

// Sets pdwCount to the number of tiles that we wish to show at this time.
// Sets pdwDefault to the index of the tile which should be used as the default.
//
// The default tile is the tile which will be shown in the zoomed view by default. If 
// more than one provider specifies a default tile the behavior is the last used cred
// prov gets to specify the default tile to be displayed
//
// If *pbAutoLogonWithDefault is TRUE, LogonUI will immediately call GetSerialization
// on the credential you've specified as the default and will submit that credential
// for authentication without showing any further UI.
HRESULT NFCCredentialProvider::GetCredentialCount(
	DWORD* pdwCount,
	DWORD* pdwDefault,
	BOOL* pbAutoLogonWithDefault
)
{
	MAZ_LOG(LogMessageType::Information, "NFCCredentialProvider::GetCredentialCount");

	*pdwCount = 1;
	if (IsWindowsVersionOrGreater(10, 0, 0))
		*pdwDefault = _defaultProvider;
	else
		*pdwDefault = 0;
	*pbAutoLogonWithDefault = FALSE;
	return S_OK;
}

// Returns the credential at the index specified by dwIndex. This function is called by logonUI to enumerate
// the tiles.
HRESULT NFCCredentialProvider::GetCredentialAt(
	DWORD dwIndex,
	ICredentialProviderCredential** ppcpc
)
{
	MAZ_LOG(LogMessageType::Information, "NFCCredentialProvider::GetCredentialAt");
	HRESULT hr;

	// Validate parameters.
	if ((dwIndex == 0) && ppcpc)
	{
		hr = _pCredential->QueryInterface(IID_ICredentialProviderCredential, reinterpret_cast<void**>(ppcpc));
	}
	else
	{
		hr = E_INVALIDARG;
	}

	return hr;
}


// Boilerplate code to create our provider.
HRESULT NFCRingCredentialProvider_CreateInstance(REFIID riid, void** ppv)
{
	MAZ_LOG(LogMessageType::Information, "NFCCredentialProvider::NFCRingCredentialProvider_CreateInstance");

	HRESULT hr;

	NFCCredentialProvider* pProvider = new NFCCredentialProvider();

	if (pProvider)
	{
		hr = pProvider->QueryInterface(riid, ppv);
		pProvider->Release();
	}
	else
	{
		hr = E_OUTOFMEMORY;
	}

	return hr;
}
