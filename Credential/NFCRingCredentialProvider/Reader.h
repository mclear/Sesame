//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) 2006 Microsoft Corporation. All rights reserved.
//
// CCommandWindow provides a way to emulate external "connect" and "disconnect" 
// events, which are invoked via toggle button on a window. The window is launched
// and managed on a separate thread, which is necessary to ensure it gets pumped.
//

#pragma once

#include <windows.h>
#include <thread>
#include "NFCCredentialProvider.h"
#include "guid.h"
#include <string>

#pragma comment(lib, "Ws2_32.lib")

class Reader
{
public:
	Reader(void);
	~Reader(void);
	HRESULT Initialize(NFCCredentialProvider *pProvider);
	void Stop();
	void Start();

	bool HasLogin();
	void ClearLogin();
	HRESULT GetLogin(
		CREDENTIAL_PROVIDER_GET_SERIALIZATION_RESPONSE* pcpgsr,
		CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION* pcpcs,
		PWSTR* ppwszOptionalStatusText,
		CREDENTIAL_PROVIDER_STATUS_ICON* pcpsiOptionalStatusIcon,
		CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus
		);

private:
	void CheckNFC();

	std::thread					_readerThread;
	bool						_checkLoop = false;
	bool						_kerbrosCredentialRetrieved = false;
	SOCKET						_soc;
	bool						_serviceFound = false;
	WSADATA						wsaData;
	NFCCredentialProvider       *_pProvider;        // Pointer to our owner.
	HINSTANCE                    _hInst;                // Current instance
	std::string					_username;
	std::string					_password;
	std::string					_domain;
};
