#include "log.h"
#include <chrono>

void LogMessage(LogMessageType type, const std::string& message)
{
#ifdef DEBUG
#define _CRT_SECURE_NO_WARNINGS
	using Clock = std::chrono::system_clock;
	static const auto cStart = std::chrono::time_point_cast<std::chrono::milliseconds>(Clock::now());
	const auto cEnd = std::chrono::time_point_cast<std::chrono::milliseconds>(Clock::now());

	FILE *filesd;
	filesd = fopen("C:\\cplog.txt", "a+");
	fprintf(filesd, "%08u\t%u\t%s\n", std::chrono::milliseconds(cEnd - cStart).count(), (int)type, message.c_str()); // im just being lazy
	fclose(filesd);
#endif
}