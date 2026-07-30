#pragma once
#include <windows.h>
#include <cstdint>

namespace Hook {
    bool Install(uintptr_t base, const char* outDir);
    void Uninstall();
}
