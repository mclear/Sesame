#ifndef MAZ_NET_AU_WINAPI_WRAPPER
#define MAZ_NET_AU_WINAPI_WRAPPER

//#include <stdio.h>
//#include <stdlib.h>
#include <winscard.h>
#include <string>
//#include <iomanip>
#include <sstream>
//#include <wincred.h>


extern "C"
{
	//__declspec(dllexport) int __cdecl CredProtectWrapper(wchar_t* inputBuffer, long inputLength, wchar_t* outputBuffer);
	__declspec(dllexport) int __cdecl PCSC_GetID(char* outputBuffer, char* errorCode);
}
#endif // MAZ_NET_AU_WINAPI_WRAPPER