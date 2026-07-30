#pragma once
#include <windows.h>
#include <cstdint>
#include <cstdlib>
#include <cstring>

namespace SigScan {

namespace detail {

struct Pat {
    uint8_t b[256];
    bool    w[256];
    size_t  n = 0;

    explicit Pat(const char* s) {
        while (*s && n < 256) {
            while (*s == ' ') ++s;
            if (!*s) break;
            w[n] = (s[0] == '?' && s[1] == '?');
            b[n] = w[n] ? 0 : static_cast<uint8_t>(strtoul(s, const_cast<char**>(&s), 16));
            if (w[n]) s += 2;
            ++n;
        }
    }
};

}

inline uint8_t* Find(uintptr_t base, const char* pattern) {
    detail::Pat pat(pattern);
    if (!pat.n) return nullptr;

    auto* dos  = reinterpret_cast<PIMAGE_DOS_HEADER>(base);
    auto* nt   = reinterpret_cast<PIMAGE_NT_HEADERS>(base + dos->e_lfanew);
    auto* sect = IMAGE_FIRST_SECTION(nt);

    static constexpr DWORD kExec = IMAGE_SCN_CNT_CODE | IMAGE_SCN_MEM_EXECUTE;
    for (WORD i = 0; i < nt->FileHeader.NumberOfSections; ++i, ++sect) {
        if ((sect->Characteristics & kExec) != kExec) continue;
        if (sect->Misc.VirtualSize < pat.n) continue;

        auto*       begin = reinterpret_cast<uint8_t*>(base) + sect->VirtualAddress;
        uint8_t* const end = begin + sect->Misc.VirtualSize - pat.n;
        for (uint8_t* p = begin; p < end; ++p) {
            bool ok = true;
            for (size_t j = 0; j < pat.n; ++j)
                if (!pat.w[j] && p[j] != pat.b[j]) { ok = false; break; }
            if (ok) return p;
        }
    }
    return nullptr;
}

}
