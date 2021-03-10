#pragma once

#ifdef SAGECOMPRESSION_EXPORTS
#define SAGECOMPRESSION_DLL_EXPORT __declspec(dllexport)
#else
#define SAGECOMPRESSION_DLL_EXPORT __declspec(dllimport)
#endif

#include <stdio.h>

#define MIN_REFPACK_BUFFER 0x00400000
#define REFPACK_PREBUFFER 0x00800000

typedef unsigned int uint;

extern "C" void SAGECOMPRESSION_DLL_EXPORT Decompress(unsigned char* buffer, const char* filename, uint fileOffset, uint length, uint offset);