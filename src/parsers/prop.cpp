#include "../include/proto.h"
#include "../include/output.h"
#include "../include/offsets.h"

#include <cstdint>
#include <cstdio>
#include <string>
#include <vector>

namespace Prop {

struct Entry {
    uint32_t id;
    int64_t  value;
};

static bool IsRelevant(uint32_t id) {
    switch (id) {
        case PropId::kPlayerLevel:
        case PropId::kPrimogem:
        case PropId::kMora:
        case PropId::kWorldLevel:
        case PropId::kResin:
        case PropId::kGenesisCrystal:
        case PropId::kLegendaryKey:
        case PropId::kHomeCoin:
        case PropId::kToyToken:
        case PropId::kQiyuCoin:
        case PropId::kReshowCrystal:
            return true;
        default:
            return false;
    }
}

static std::vector<Entry> g_cache;

static const char* PropKey(uint32_t id) {
    switch (id) {
        case PropId::kPlayerLevel:    return "playerLevel";
        case PropId::kPrimogem:       return "primogem";
        case PropId::kMora:           return "mora";
        case PropId::kWorldLevel:     return "worldLevel";
        case PropId::kResin:          return "resin";
        case PropId::kGenesisCrystal: return "genesisCrystal";
        case PropId::kLegendaryKey:   return "legendaryKey";
        case PropId::kHomeCoin:       return "homeCoin";
        case PropId::kToyToken:       return "toyToken";
        case PropId::kQiyuCoin:       return "qiyuCoin";
        case PropId::kReshowCrystal:  return "reshowCrystal";
        default:                      return nullptr;
    }
}

static std::string BuildJson(const std::vector<Entry>& entries) {
    std::string out = "{ ";
    bool first = true;
    for (const auto& e : entries) {
        const char* key = PropKey(e.id);
        if (!key) continue;
        if (!first) out += ", ";
        char buf[64];
        sprintf_s(buf, "\"%s\": %lld", key, static_cast<long long>(e.value));
        out += buf;
        first = false;
    }
    out += " }";
    return out;
}

std::string ExportJson() { return BuildJson(g_cache); }

std::string OnPacket(const uint8_t* body, uint32_t len) {
    std::vector<Entry> entries;

    Proto::Walk(body, len, [&](const Proto::Field& top) -> bool {
        if (top.num != 11 || top.wt != 2) return true;

        uint32_t propId = 0;
        int64_t  ival   = 0;

        Proto::Walk(top.ptr, top.len, [&](const Proto::Field& f) -> bool {
            if (f.num == 1 && f.wt == 0) {
                propId = static_cast<uint32_t>(f.u64);
            } else if (f.num == 2 && f.wt == 2) {
                Proto::Walk(f.ptr, f.len, [&](const Proto::Field& pv) -> bool {
                    if (pv.num == 2 && pv.wt == 0)
                        ival = static_cast<int64_t>(pv.u64);
                    return true;
                });
            }
            return true;
        });

        if (propId > 0 && IsRelevant(propId))
            entries.push_back({ propId, ival });

        return true;
    });

    g_cache = entries;
    return BuildJson(g_cache);
}

}
