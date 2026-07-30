#pragma once
#include <cstdint>

namespace ArtifactSetDb {

struct ItemInfo {
    uint32_t    id;
    uint8_t     rank;
    uint8_t     initSubStats;  // appendPropNum: initial sub-stat count (0-4)
    const char* setName;
    const char* pieceName;
};

#include "artifact_set_db_5star.h"
#include "artifact_set_db_4star.h"
#include "artifact_set_db_low.h"

namespace detail {
inline const ItemInfo* BinSearch(const ItemInfo* t, size_t n, uint32_t key) {
    int lo = 0, hi = static_cast<int>(n) - 1;
    while (lo <= hi) {
        int mid = (lo + hi) / 2;
        if (t[mid].id == key) return &t[mid];
        if (t[mid].id  < key) lo = mid + 1;
        else                  hi = mid - 1;
    }
    return nullptr;
}
}

inline const ItemInfo* Lookup5Star(uint32_t id) {
    return detail::BinSearch(kItemTable5, kItemTable5Count, id);
}

inline const ItemInfo* Lookup4Star(uint32_t id) {
    return detail::BinSearch(kItemTable4, kItemTable4Count, id);
}

inline const ItemInfo* LookupLow(uint32_t id) {
    return detail::BinSearch(kItemTableLow, kItemTableLowCount, id);
}

inline const ItemInfo* LookupItem(uint32_t id) {
    if (const ItemInfo* p = Lookup5Star(id)) return p;
    if (const ItemInfo* p = Lookup4Star(id)) return p;
    return LookupLow(id);
}

}
