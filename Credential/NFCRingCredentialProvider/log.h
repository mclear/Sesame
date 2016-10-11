#pragma once

#ifndef LOGGING_LIBRARY_HEADER
#define LOGGING_LIBRARY_HEADER

#include <string>

enum class LogMessageType
{
	Information,
	Warning,
	Error,
	Critical
};

#if _DEBUG
	#define MAZ_LOG(type, message) LogMessage((type), (message));
#else
	#define MAZ_LOG(type, message);
#endif 

void LogMessage(LogMessageType type, const std::string& message);
#endif // LOGGING_LIBRARY_HEADER