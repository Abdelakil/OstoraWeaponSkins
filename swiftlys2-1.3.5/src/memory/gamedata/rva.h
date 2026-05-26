#include <cstdint>

extern "C" {  
void* LoadBinaryModule(const char* path);
void UnloadBinaryModule(void* handle);
uint64_t ConvertFileOffsetToRVA(void* handle, uint64_t fileOffset);
}