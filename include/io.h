#pragma once
#include <windows.h>
#include <cstdio>
#include <cstddef>

namespace IO {

namespace detail {

inline void WriteBOM(HANDLE f) {
    static const unsigned char kBom[] = { 0xEF, 0xBB, 0xBF };
    DWORD w;
    WriteFile(f, kBom, 3, &w, nullptr);
}

inline HANDLE Open(const char* outDir, const char* filename) {
    char path[MAX_PATH];
    sprintf_s(path, "%s\\%s", outDir, filename);
    return CreateFileA(path, GENERIC_WRITE, 0, nullptr,
        CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, nullptr);
}

}

inline void WriteJson(const char* outDir, const char* filename, const char* json, size_t len) {
    HANDLE f = detail::Open(outDir, filename);
    if (f == INVALID_HANDLE_VALUE) return;
    detail::WriteBOM(f);
    DWORD w;
    WriteFile(f, json, static_cast<DWORD>(len), &w, nullptr);
    CloseHandle(f);
}

}
