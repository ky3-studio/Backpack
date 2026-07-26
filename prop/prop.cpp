#include "prop.h"
#include "../db/prop_ids.h"
#include "../include/proto.h"
#include "../include/output.h"

#include <cstdint>
#include <cstdio>
#include <cstring>
#include <string>

namespace Prop {

struct Entry {
    uint32_t type;
    int64_t  ival;
    double   fval;
    bool     hasFloat;
};

namespace detail {

inline Entry ParsePropValue(const uint8_t* buf, size_t len) {
    Entry e{}; size_t pos = 0;
    while (pos < len) {
        uint64_t tag = 0;
        if (!Proto::detail::ReadVarint(buf, len, pos, tag)) break;
        const uint32_t fn = static_cast<uint32_t>(tag >> 3);
        const int      wt = static_cast<int>(tag & 7);
        if (fn == 2 && wt == 0) {
            uint64_t v = 0;
            Proto::detail::ReadVarint(buf, len, pos, v);
            e.ival = static_cast<int64_t>(v);
        } else if (fn == 2 && wt == 1) {
            if (pos + 8 <= len) {
                memcpy(&e.fval, buf + pos, 8);
                e.hasFloat = true;
                pos += 8;
            }
        } else if (wt == 0) {
            uint64_t tmp = 0; Proto::detail::ReadVarint(buf, len, pos, tmp);
        } else if (wt == 2) {
            uint64_t sl = 0; Proto::detail::ReadVarint(buf, len, pos, sl); pos += sl;
        } else if (wt == 1) { pos += 8; }
        else if (wt == 5)   { pos += 4; }
        else break;
    }
    return e;
}

}

static const NamedProp* FindProp(uint32_t type) {
    for (int i = 0; i < kWantedCount; ++i)
        if (kWanted[i].type == type) return &kWanted[i];
    return nullptr;
}

static std::string BuildJson(const Entry* all, int count) {
    std::string out;
    out.reserve(static_cast<size_t>(count) * 64 + 16);
    out += Output::kPropHeader;
    int written = 0;
    for (int i = 0; i < count; ++i) {
        const NamedProp* np = FindProp(all[i].type);
        if (!np) continue;
        if (written > 0) out += Output::kPropSep;
        char buf[256];
        if (all[i].hasFloat) {
            double v = np->div100 ? all[i].fval / 100.0 : all[i].fval;
            sprintf_s(buf, Output::kPropFloat, np->key, v);
        } else {
            int64_t v = np->div100 ? all[i].ival / 100 : all[i].ival;
            sprintf_s(buf, Output::kPropInt, np->key, v);
        }
        out += buf;
        written++;
    }
    out += Output::kPropFooter;
    return out;
}

std::string OnPacket(const uint8_t* body, uint32_t len) {
    static Entry entries[kMaxEntryCount];
    int count = 0;

    Proto::Walk(body, len, [&](const Proto::Field& f) -> bool {
        if (f.num != 11 || f.wt != 2) return true;

        uint32_t propType = 0;
        bool hasVal = false;
        const uint8_t* valPtr = nullptr;
        size_t valLen = 0;

        Proto::Walk(f.ptr, f.len, [&](const Proto::Field& kv) -> bool {
            if (kv.num == 1 && kv.wt == 0) {
                propType = static_cast<uint32_t>(kv.u64);
            } else if (kv.num == 2 && kv.wt == 2) {
                hasVal  = true;
                valPtr  = kv.ptr;
                valLen  = kv.len;
            }
            return true;
        });

        if (propType && hasVal && count < kMaxEntryCount) {
            Entry e = detail::ParsePropValue(valPtr, valLen);
            e.type = propType;
            entries[count++] = e;
        }
        return true;
    });

    return count > 0 ? BuildJson(entries, count) : std::string{};
}

}
