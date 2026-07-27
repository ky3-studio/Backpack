#include "../../include/proto.h"
#include "../../include/output.h"
#include "../../include/offsets.h"

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

static std::string BuildJson(const std::vector<Entry>& entries) {
    std::string out;
    out.reserve(entries.size() * 32 + 32);
    out += "{\n  \"props\": {\n";
    for (size_t i = 0; i < entries.size(); ++i) {
        char buf[64];
        sprintf_s(buf, "    \"%u\": %lld%s\n",
            entries[i].id,
            static_cast<long long>(entries[i].value),
            (i + 1 < entries.size()) ? "," : "");
        out += buf;
    }
    out += "  }\n}\n";
    return out;
}

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

    return BuildJson(entries);
}

}
