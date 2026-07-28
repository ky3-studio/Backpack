#include "../../include/proto.h"
#include "../../include/output.h"
#include "../../include/offsets.h"

#include <cstdint>
#include <cstdio>
#include <cstring>
#include <algorithm>
#include <string>
#include <vector>

namespace Avatar {

struct SkillLv  { uint32_t id;  uint32_t level; };
struct ExtraLv  { uint32_t gid; uint32_t extra; };

struct Inst {
    uint32_t            id      = 0;
    uint64_t            guid    = 0;
    uint32_t            level   = 0;
    uint32_t            promote = 0;
    uint32_t            fetter  = 0;
    std::vector<uint32_t>  talents;
    std::vector<SkillLv>   skills;
    std::vector<ExtraLv>   extras;
    std::vector<uint64_t>  equips;
};

static void ParsePackedVarints(const uint8_t* p, size_t len, std::vector<uint64_t>& out) {
    size_t pos = 0;
    while (pos < len) {
        uint64_t v = 0;
        if (!Proto::detail::ReadVarint(p, len, pos, v)) break;
        out.push_back(v);
    }
}

static uint32_t ParsePropIval(const uint8_t* p, size_t len) {
    uint32_t ival = 0;
    Proto::Walk(p, len, [&](const Proto::Field& f) -> bool {
        if (f.num == 2 && f.wt == 0) ival = static_cast<uint32_t>(f.u64);
        return true;
    });
    return ival;
}

static std::string BuildJson(const std::vector<Inst>& avatars) {
    std::string out;
    out.reserve(avatars.size() * 512 + 32);
    out += Output::kAvatarHeader;

    for (size_t i = 0; i < avatars.size(); ++i) {
        const Inst& a = avatars[i];

        std::string talents_str = "[";
        for (size_t j = 0; j < a.talents.size(); ++j) {
            char buf[16]; sprintf_s(buf, "%u", a.talents[j]);
            talents_str += buf;
            if (j + 1 < a.talents.size()) talents_str += ",";
        }
        talents_str += "]";

        std::string skills_str = "[";
        for (size_t j = 0; j < a.skills.size(); ++j) {
            char buf[32]; sprintf_s(buf, "[%u,%u]", a.skills[j].id, a.skills[j].level);
            skills_str += buf;
            if (j + 1 < a.skills.size()) skills_str += ",";
        }
        skills_str += "]";

        std::string extras_str = "[";
        for (size_t j = 0; j < a.extras.size(); ++j) {
            char buf[32]; sprintf_s(buf, "[%u,%u]", a.extras[j].gid, a.extras[j].extra);
            extras_str += buf;
            if (j + 1 < a.extras.size()) extras_str += ",";
        }
        extras_str += "]";

        std::string equips_str = "[";
        for (size_t j = 0; j < a.equips.size(); ++j) {
            char buf[32]; sprintf_s(buf, "\"%llu\"", static_cast<unsigned long long>(a.equips[j]));
            equips_str += buf;
            if (j + 1 < a.equips.size()) equips_str += ",";
        }
        equips_str += "]";

        char line[4096];
        sprintf_s(line, Output::kAvatarItem,
            a.id, static_cast<unsigned long long>(a.guid),
            a.level, a.promote, a.fetter,
            talents_str.c_str(), skills_str.c_str(),
            extras_str.c_str(), equips_str.c_str(),
            (i + 1 < avatars.size()) ? "," : "");
        out += line;
    }

    out += Output::kArrayFooter;
    return out;
}

std::string OnPacket(const uint8_t* body, uint32_t len) {
    std::vector<Inst> avatars;
    avatars.reserve(64);

    Proto::Walk(body, len, [&](const Proto::Field& top) -> bool {
        if (top.num != 14 || top.wt != 2) return true;

        Inst inst;

        Proto::Walk(top.ptr, top.len, [&](const Proto::Field& f) -> bool {
            if (f.num == 1 && f.wt == 0) {
                inst.id = static_cast<uint32_t>(f.u64);

            } else if (f.num == 2 && f.wt == 0) {
                inst.guid = f.u64;

            } else if (f.num == 3 && f.wt == 2) {
                uint32_t propType = 0;
                const uint8_t* pvPtr = nullptr;
                size_t         pvLen = 0;
                Proto::Walk(f.ptr, f.len, [&](const Proto::Field& pm) -> bool {
                    if (pm.num == 1 && pm.wt == 0) propType = static_cast<uint32_t>(pm.u64);
                    if (pm.num == 2 && pm.wt == 2) { pvPtr = pm.ptr; pvLen = pm.len; }
                    return true;
                });
                if (pvPtr) {
                    uint32_t ival = ParsePropIval(pvPtr, pvLen);
                    if (propType == 4001) inst.level   = ival;
                    if (propType == 1002) inst.promote = ival;
                }

            } else if (f.num == 5 && f.wt == 2) {
                ParsePackedVarints(f.ptr, f.len, inst.equips);

            } else if (f.num == 6 && f.wt == 2) {
                std::vector<uint64_t> raw;
                ParsePackedVarints(f.ptr, f.len, raw);
                for (auto v : raw) inst.talents.push_back(static_cast<uint32_t>(v));

            } else if (f.num == 12 && f.wt == 2) {
                Proto::Walk(f.ptr, f.len, [&](const Proto::Field& fi) -> bool {
                    if (fi.num == 2 && fi.wt == 0) inst.fetter = static_cast<uint32_t>(fi.u64);
                    return true;
                });

            } else if (f.num == 15 && f.wt == 2) {
                SkillLv sk{0, 0};
                Proto::Walk(f.ptr, f.len, [&](const Proto::Field& s) -> bool {
                    if (s.num == 1 && s.wt == 0) sk.id    = static_cast<uint32_t>(s.u64);
                    if (s.num == 2 && s.wt == 0) sk.level = static_cast<uint32_t>(s.u64);
                    return true;
                });
                if (sk.id) inst.skills.push_back(sk);

            } else if (f.num == 17 && f.wt == 2) {
                ExtraLv ex{0, 0};
                Proto::Walk(f.ptr, f.len, [&](const Proto::Field& e) -> bool {
                    if (e.num == 1 && e.wt == 0) ex.gid   = static_cast<uint32_t>(e.u64);
                    if (e.num == 2 && e.wt == 0) ex.extra = static_cast<uint32_t>(e.u64);
                    return true;
                });
                if (ex.gid) inst.extras.push_back(ex);
            }

            return true;
        });

        if (inst.id >= AvatarId::kMin && inst.id <= AvatarId::kMax) {
            bool skip = false;
            for (auto s : AvatarId::kSkip) if (s == inst.id) { skip = true; break; }
            if (!skip) avatars.push_back(std::move(inst));
        }

        return true;
    });

    std::sort(avatars.begin(), avatars.end(), [](const Inst& a, const Inst& b) {
        return a.level != b.level ? a.level > b.level : a.id < b.id;
    });

    return BuildJson(avatars);
}

}
