#include "../db/material/material_ids.h"
#include "../include/proto.h"
#include "../include/output.h"

#include <cstdint>
#include <cstdio>
#include <cstring>
#include <string>
#include <vector>

namespace Material {

struct Item {
    uint32_t id;
    uint64_t count;
};

static const MatEntry* Lookup(uint32_t id) {
    for (size_t i = 0; i < kMaterialCount; ++i)
        if (kMaterialList[i].id == id) return &kMaterialList[i];
    return nullptr;
}

static std::vector<std::pair<const MatEntry*, uint64_t>> g_cache;

static std::string BuildJson(const std::vector<std::pair<const MatEntry*, uint64_t>>& valid) {
    std::string out;
    out.reserve(valid.size() * 128 + 32);
    out += "[\n";
    for (size_t i = 0; i < valid.size(); ++i) {
        char buf[512];
        sprintf_s(buf,
            "    { \"id\": %u, \"name\": \"%s\", \"category\": \"%s\", \"count\": %llu }%s\n",
            valid[i].first->id,
            valid[i].first->name,
            valid[i].first->category,
            static_cast<unsigned long long>(valid[i].second),
            (i + 1 < valid.size()) ? "," : "");
        out += buf;
    }
    out += "]";
    return out;
}

std::string ExportJson() { return BuildJson(g_cache); }

std::string OnPacket(const uint8_t* body, uint32_t len) {
    std::vector<Item> items;

    Proto::Walk(body, len, [&](const Proto::Field& top) -> bool {
        if (top.num != 2 || top.wt != 2) return true;

        uint32_t itemId = 0;
        uint64_t count  = 0;
        bool     isMat  = false;

        Proto::Walk(top.ptr, top.len, [&](const Proto::Field& f) -> bool {
            if (f.num == 1 && f.wt == 0) {
                itemId = static_cast<uint32_t>(f.u64);
            } else if (f.num == 5 && f.wt == 2) {
                isMat = true;
                Proto::Walk(f.ptr, f.len, [&](const Proto::Field& m) -> bool {
                    if (m.num == 1 && m.wt == 0) count = m.u64;
                    return true;
                });
            }
            return true;
        });

        if (isMat && itemId > 0 && count > 0)
            items.push_back({ itemId, count });

        return true;
    });

    std::vector<std::pair<const MatEntry*, uint64_t>> valid;
    for (const auto& item : items) {
        const MatEntry* e = Lookup(item.id);
        if (e) valid.push_back({ e, item.count });
    }
    g_cache = valid;
    return BuildJson(g_cache);
}

}
