#pragma once

#ifndef LOGGING_LIBRARY_HEADER
#define LOGGING_LIBRARY_HEADER

#include <string>
#include <chrono>
#include <sstream>
#include <iomanip>
#include <thread>

#if _DEBUG
#define MAZ_LOG(type, message) LogMessage((type), (message));
#else
#define MAZ_LOG(type, message)
#endif 

enum class LogMessageType
{
	Information,
	Warning,
	Error,
	Critical
};

void LogMessage(LogMessageType type, const std::string& message)
{
	// Get timing information
	using Clock = std::chrono::high_resolution_clock;
	static const auto cStart = Clock::now();
	const auto cEnd = Clock::now();

	// Generate full log message
	std::ostringstream os;
	os << std::setfill('0') << std::setw(12);
	os << std::chrono::nanoseconds(cEnd - cStart).count();
	os << "\t" << (int)type << "\t" << std::this_thread::get_id() << "\t" << message << "\n";
	
	FILE *filesd;
	filesd = fopen("C:\\cplog.txt", "a+");
	fprintf(filesd, "%s", os.str()); // im just being lazy
	fclose(filesd);	
}

#endif // LOGGING_LIBRARY_HEADER