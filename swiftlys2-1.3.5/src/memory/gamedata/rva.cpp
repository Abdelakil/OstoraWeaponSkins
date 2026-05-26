#include <cstdint>
#include <cstring>
#include <fstream>
#include <vector>
#include "rva.h"

#ifdef _WIN32
#include <windows.h>

struct PEModule {
    std::vector<uint8_t> data;
    IMAGE_DOS_HEADER* dosHeader;
    IMAGE_NT_HEADERS* ntHeaders;
    IMAGE_SECTION_HEADER* sections;
    uint16_t sectionCount;
    
    bool Load(const char* path) {
        std::ifstream file(path, std::ios::binary | std::ios::ate);
        if (!file.is_open()) return false;
        
        size_t size = file.tellg();
        file.seekg(0, std::ios::beg);
        
        data.resize(size);
        file.read(reinterpret_cast<char*>(data.data()), size);
        file.close();
        
        dosHeader = reinterpret_cast<IMAGE_DOS_HEADER*>(data.data());
        if (dosHeader->e_magic != IMAGE_DOS_SIGNATURE) return false;
        
        ntHeaders = reinterpret_cast<IMAGE_NT_HEADERS*>(data.data() + dosHeader->e_lfanew);
        if (ntHeaders->Signature != IMAGE_NT_SIGNATURE) return false;
        
        sections = IMAGE_FIRST_SECTION(ntHeaders);
        sectionCount = ntHeaders->FileHeader.NumberOfSections;
        
        return true;
    }
    
    uint64_t FileOffsetToRVA(uint64_t fileOffset) {
        for (uint16_t i = 0; i < sectionCount; i++) {
            IMAGE_SECTION_HEADER& section = sections[i];
            uint64_t sectionStart = section.PointerToRawData;
            uint64_t sectionEnd = sectionStart + section.SizeOfRawData;
            
            if (fileOffset >= sectionStart && fileOffset < sectionEnd) {
                return (fileOffset - sectionStart) + section.VirtualAddress;
            }
        }
        return 0;
    }
};

#else

struct ELF64_Ehdr {
    uint8_t  e_ident[16];
    uint16_t e_type;
    uint16_t e_machine;
    uint32_t e_version;
    uint64_t e_entry;
    uint64_t e_phoff;
    uint64_t e_shoff;
    uint32_t e_flags;
    uint16_t e_ehsize;
    uint16_t e_phentsize;
    uint16_t e_phnum;
    uint16_t e_shentsize;
    uint16_t e_shnum;
    uint16_t e_shstrndx;
};

struct ELF64_Shdr {
    uint32_t sh_name;
    uint32_t sh_type;
    uint64_t sh_flags;
    uint64_t sh_addr;
    uint64_t sh_offset;
    uint64_t sh_size;
    uint32_t sh_link;
    uint32_t sh_info;
    uint64_t sh_addralign;
    uint64_t sh_entsize;
};

struct ELFModule {
    std::vector<uint8_t> data;
    ELF64_Ehdr* elfHeader;
    ELF64_Shdr* sections;
    uint16_t sectionCount;
    
    bool Load(const char* path) {
        std::ifstream file(path, std::ios::binary | std::ios::ate);
        if (!file.is_open()) return false;
        
        size_t size = file.tellg();
        file.seekg(0, std::ios::beg);
        
        data.resize(size);
        file.read(reinterpret_cast<char*>(data.data()), size);
        file.close();
        
        elfHeader = reinterpret_cast<ELF64_Ehdr*>(data.data());
        if (elfHeader->e_ident[0] != 0x7F || 
            elfHeader->e_ident[1] != 'E' ||
            elfHeader->e_ident[2] != 'L' ||
            elfHeader->e_ident[3] != 'F') return false;
        
        sections = reinterpret_cast<ELF64_Shdr*>(data.data() + elfHeader->e_shoff);
        sectionCount = elfHeader->e_shnum;
        
        return true;
    }
    
    uint64_t FileOffsetToRVA(uint64_t fileOffset) {
        for (uint16_t i = 0; i < sectionCount; i++) {
            ELF64_Shdr& section = sections[i];
            uint64_t sectionStart = section.sh_offset;
            uint64_t sectionEnd = sectionStart + section.sh_size;
            
            if (fileOffset >= sectionStart && fileOffset < sectionEnd) {
                return (fileOffset - sectionStart) + section.sh_addr;
            }
        }
        return 0;
    }
};

#endif

extern "C" {

void* LoadBinaryModule(const char* path) {
#ifdef _WIN32
    PEModule* module = new PEModule();
    if (module->Load(path)) {
        return module;
    }
    delete module;
    return nullptr;
#else
    ELFModule* module = new ELFModule();
    if (module->Load(path)) {
        return module;
    }
    delete module;
    return nullptr;
#endif
}

void UnloadBinaryModule(void* handle) {
#ifdef _WIN32
    delete static_cast<PEModule*>(handle);
#else
    delete static_cast<ELFModule*>(handle);
#endif
}

uint64_t ConvertFileOffsetToRVA(void* handle, uint64_t fileOffset) {
    if (!handle) return 0;
    
#ifdef _WIN32
    return static_cast<PEModule*>(handle)->FileOffsetToRVA(fileOffset);
#else
    return static_cast<ELFModule*>(handle)->FileOffsetToRVA(fileOffset);
#endif
}

}
