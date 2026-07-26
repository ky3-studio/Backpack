#include "weapon.h"
#include "../include/proto.h"
#include "../include/output.h"
#include "../db/weapon_db.h"

#include <cstdint>
#include <cstdio>
#include <cstring>
#include <algorithm>
#include <string>
#include <vector>

namespace Weapon {

struct Inst {
    uint32_t id;
    uint64_t guid;
    uint32_t level;
    uint32_t promote;
    uint32_t refine;
};

static const WeaponDbEntry* Lookup(uint32_t id) {
    for (size_t i = 0; i < kWeaponDbSize; ++i)
        if (kWeaponDb[i].id == id) return &kWeaponDb[i];
    return nullptr;
}

static std::string BuildJson(const std::vector<Inst>& weapons) {
    std::vector<size_t> idx(weapons.size());
    for (size_t i = 0; i < idx.size(); ++i) idx[i] = i;
    std::sort(idx.begin(), idx.end(), [&](size_t a, size_t b) {
        const WeaponDbEntry* ia = Lookup(weapons[a].id);
        const WeaponDbEntry* ib = Lookup(weapons[b].id);
        const int ra = ia ? ia->rank : 0;
        const int rb = ib ? ib->rank : 0;
        if (ra != rb) return ra > rb;
        return weapons[a].level > weapons[b].level;
    });

    std::string out;
    out.reserve(weapons.size() * 256 + 32);
    out += Output::kWeaponHeader;
    for (size_t i = 0; i < idx.size(); ++i) {
        const Inst&          w    = weapons[idx[i]];
        const WeaponDbEntry* info = Lookup(w.id);
        char line[1024];
        sprintf_s(line, Output::kWeaponItem,
            w.id,
            static_cast<unsigned long long>(w.guid),
            info ? info->name        : "",
            info ? info->type        : "",
            info ? static_cast<unsigned>(info->rank) : 0u,
            info ? info->specialProp : "",
            w.level, w.promote, w.refine,
            (i + 1 < idx.size()) ? "," : "");
        out += line;
    }
    out += Output::kArrayFooter;
    return out;
}

std::string OnPacket(const uint8_t* body, uint32_t len) {
    std::vector<Inst> weapons;

    Proto::Walk(body, len, [&](const Proto::Field& top) -> bool {
        if (top.num != 2 || top.wt != 2) return true;

        uint32_t itemId   = 0;
        uint64_t guid     = 0;
        bool     isWeapon = false;
        Inst     inst{0, 0, 1, 0, 1};

        Proto::Walk(top.ptr, top.len, [&](const Proto::Field& item) -> bool {
            if      (item.num == 1 && item.wt == 0) itemId = static_cast<uint32_t>(item.u64);
            else if (item.num == 2 && item.wt == 0) guid   = item.u64;
            else if (item.num == 6 && item.wt == 2) {
                Proto::Walk(item.ptr, item.len, [&](const Proto::Field& eq) -> bool {
                    if (eq.num != 2 || eq.wt != 2) return true;
                    isWeapon = true;
                    Proto::Walk(eq.ptr, eq.len, [&](const Proto::Field& wp) -> bool {
                        if (wp.wt == 0) {
                            if (wp.num == 1) inst.level   = static_cast<uint32_t>(wp.u64);
                            if (wp.num == 3) inst.promote = static_cast<uint32_t>(wp.u64);
                        } else if (wp.num == 4 && wp.wt == 2) {
                            Proto::Walk(wp.ptr, wp.len, [&](const Proto::Field& af) -> bool {
                                if (af.num == 2 && af.wt == 0)
                                    inst.refine = static_cast<uint32_t>(af.u64) + 1;
                                return true;
                            });
                        }
                        return true;
                    });
                    return true;
                });
            }
            return true;
        });

        if (isWeapon && itemId > 0) {
            inst.id   = itemId;
            inst.guid = guid;
            weapons.push_back(inst);
        }
        return true;
    });

    return BuildJson(weapons);
}

}
