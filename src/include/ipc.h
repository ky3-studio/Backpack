#pragma once
#include <windows.h>
#include <cstdint>
#include <cstring>

namespace IPC {

static constexpr const char* kPipeName = "\\\\.\\pipe\\ky3-backpack";

inline void Push(const char* event, const char* json, size_t len) {
    HANDLE h = CreateFileA(kPipeName, GENERIC_WRITE, 0, nullptr,
        OPEN_EXISTING, 0, nullptr);
    if (h == INVALID_HANDLE_VALUE) return;
    char hdr[16] = {};
    const uint32_t n = static_cast<uint32_t>(len);
    memcpy(hdr, &n, 4);
    strncpy_s(hdr + 4, 12, event, _TRUNCATE);
    DWORD w;
    WriteFile(h, hdr, 16, &w, nullptr);
    WriteFile(h, json, n, &w, nullptr);
    CloseHandle(h);
}

}
