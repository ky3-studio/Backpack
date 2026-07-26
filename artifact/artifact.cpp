#include "artifact.h"
#include "../include/proto.h"
#include "../include/output.h"
#include "../db/artifact_db.h"
#include "../db/artifact_set_db.h"

#include <cstdint>
#include <cstdio>
#include <cstring>
#include <algorithm>
#include <string>
#include <vector>

namespace Artifact {

struct SubStat {
    char                type[64];
    char                propType[64];
    std::vector<double> rollValues;
};

struct Inst {
    uint32_t          id;
    uint64_t          guid;
    bool              equipped;
    int               level;
    int               rank;
    int               slotDigit;
    char              slot[32];
    char              setName[64];
    char              pieceName[64];
    char              mainType[64];
    char              mainShort[32];
    std::vector<SubStat> subs;
};

static const ArtifactDb::AffixEntry* LookupAffix(uint32_t id) {
    for (size_t i = 0; i < ArtifactDb::kAffixCount; ++i)
        if (ArtifactDb::kAffixTable[i].id == id) return &ArtifactDb::kAffixTable[i];
    return nullptr;
}

static const char* LookupMainProp(uint32_t id) {
    for (size_t i = 0; i < ArtifactDb::kMainPropCount; ++i)
        if (ArtifactDb::kMainPropTable[i].id == id)
            return ArtifactDb::kMainPropTable[i].propType;
    return nullptr;
}

static std::vector<uint32_t> ReadPackedU32(const uint8_t* buf, size_t len) {
    std::vector<uint32_t> out;
    size_t pos = 0;
    while (pos < len) {
        uint64_t v = 0;
        if (!Proto::detail::ReadVarint(buf, len, pos, v)) break;
        out.push_back(static_cast<uint32_t>(v));
    }
    return out;
}

static std::string BuildJson(const std::vector<Inst>& arts) {
    std::vector<size_t> idx(arts.size());
    for (size_t i = 0; i < idx.size(); ++i) idx[i] = i;
    std::sort(idx.begin(), idx.end(), [&](size_t a, size_t b) {
        int sa = ArtifactDb::SlotOrder(arts[a].slotDigit), sb = ArtifactDb::SlotOrder(arts[b].slotDigit);
        if (sa != sb) return sa < sb;
        if (arts[a].rank  != arts[b].rank)  return arts[a].rank  > arts[b].rank;
        return arts[a].level > arts[b].level;
    });

    std::string out;
    out.reserve(arts.size() * 1024 + 32);
    out += Output::kArtifactHeader;
    for (size_t ii = 0; ii < idx.size(); ++ii) {
        const Inst& a = arts[idx[ii]];
        char buf[512];

        out += Output::kArtItemOpen;
        sprintf_s(buf, Output::kArtId,       a.id);                                         out += buf;
        sprintf_s(buf, Output::kArtGuid,     static_cast<unsigned long long>(a.guid));      out += buf;
        sprintf_s(buf, Output::kArtSetName,  a.setName);                                    out += buf;
        sprintf_s(buf, Output::kArtName,     a.pieceName);                                  out += buf;
        sprintf_s(buf, Output::kArtSlot,     a.slot);                                       out += buf;
        sprintf_s(buf, Output::kArtEquipped, a.equipped ? "true" : "false");               out += buf;
        sprintf_s(buf, Output::kArtLevel,    a.level);                                      out += buf;
        sprintf_s(buf, Output::kArtRank,     a.rank);                                       out += buf;
        sprintf_s(buf, Output::kArtMainStat, a.mainShort, a.mainType);                      out += buf;

        out += Output::kArtSubStatsOpen;
        for (size_t si = 0; si < a.subs.size(); ++si) {
            const SubStat& ss = a.subs[si];
            bool   isPct = ArtifactDb::PropIsPercent(ss.propType);
            double total = 0.0;
            for (double v : ss.rollValues) total += v;
            char valBuf[32];
            sprintf_s(valBuf, isPct ? Output::kArtSubStatFmtPct : Output::kArtSubStatFmtInt, total);
            sprintf_s(buf, Output::kArtSubStatHead, ss.type, ss.propType, valBuf);
            out += buf;
            for (size_t ri = 0; ri < ss.rollValues.size(); ++ri) {
                char rv[32];
                sprintf_s(rv, isPct ? "%.2f" : "%.0f", ss.rollValues[ri]);
                if (ri > 0) out += ",";
                out += rv;
            }
            sprintf_s(buf, Output::kArtSubStatTail, (si + 1 < a.subs.size()) ? "," : "");
            out += buf;
        }
        out += Output::kArtSubStatsClose;
        sprintf_s(buf, Output::kArtClose, (ii + 1 < idx.size()) ? "," : "");
        out += buf;
    }
    out += Output::kArrayFooter;
    return out;
}

std::string OnPacket(const uint8_t* body, uint32_t len) {
    std::vector<Inst> arts;

    Proto::Walk(body, len, [&](const Proto::Field& top) -> bool {
        if (top.num != 2 || top.wt != 2) return true;

        uint32_t itemId        = 0;
        uint64_t guid          = 0;
        bool     isRel         = false;
        int      rankFromAffix = 0;
        Inst     inst{};
        strcpy_s(inst.slot,      ArtifactDb::kDefaultSlot);
        strcpy_s(inst.mainType,  ArtifactDb::kDefaultMainType);
        strcpy_s(inst.mainShort, ArtifactDb::kDefaultMainShort);

        Proto::Walk(top.ptr, top.len, [&](const Proto::Field& item) -> bool {
            if      (item.num == 1 && item.wt == 0) itemId = static_cast<uint32_t>(item.u64);
            else if (item.num == 2 && item.wt == 0) guid   = item.u64;
            else if (item.num == 6 && item.wt == 2) {
                Proto::Walk(item.ptr, item.len, [&](const Proto::Field& eq) -> bool {
                    if (eq.num == 3 && eq.wt == 0) {
                        inst.equipped = (eq.u64 != 0);
                    } else if (eq.num == 1 && eq.wt == 2) {
                        isRel         = true;
                        rankFromAffix = 0;
                        uint32_t mainPropId = 0;
                        std::vector<uint32_t> appendIds;

                        Proto::Walk(eq.ptr, eq.len, [&](const Proto::Field& rel) -> bool {
                            if      (rel.num == 1 && rel.wt == 0) inst.level = static_cast<int>(rel.u64) - 1;
                            else if (rel.num == 4 && rel.wt == 0) mainPropId = static_cast<uint32_t>(rel.u64);
                            else if (rel.num == 5 && rel.wt == 2) appendIds  = ReadPackedU32(rel.ptr, rel.len);
                            return true;
                        });

                        const char* mpt = LookupMainProp(mainPropId);
                        if (mpt) {
                            strncpy_s(inst.mainType,  mpt, _TRUNCATE);
                            strncpy_s(inst.mainShort, ArtifactDb::PropShortName(mpt), _TRUNCATE);
                        } else {
                            sprintf_s(inst.mainType,  ArtifactDb::kFallbackId, mainPropId);
                            sprintf_s(inst.mainShort, ArtifactDb::kFallbackId, mainPropId);
                        }

                        for (uint32_t aid : appendIds) {
                            if (rankFromAffix == 0) {
                                int r = static_cast<int>(aid / 1000) / 100;
                                if (r >= 1 && r <= 5) rankFromAffix = r;
                            }
                            const ArtifactDb::AffixEntry* ae = LookupAffix(aid);
                            const char*  pt = ae ? ae->propType : nullptr;
                            const double dv = (ae && ArtifactDb::PropIsPercent(pt))
                                ? static_cast<double>(ae->value) * 100.0
                                : static_cast<double>(ae ? ae->value : 0.0f);

                            bool found = false;
                            for (SubStat& ss : inst.subs) {
                                if (strcmp(ss.propType, pt ? pt : "") == 0) {
                                    ss.rollValues.push_back(dv);
                                    found = true;
                                    break;
                                }
                            }
                            if (!found) {
                                SubStat ss{};
                                if (pt) {
                                    strncpy_s(ss.propType, pt, _TRUNCATE);
                                    strncpy_s(ss.type, ArtifactDb::PropShortName(pt), _TRUNCATE);
                                } else {
                                    sprintf_s(ss.propType, ArtifactDb::kFallbackId, aid);
                                    sprintf_s(ss.type,     ArtifactDb::kFallbackId, aid);
                                }
                                ss.rollValues.push_back(dv);
                                inst.subs.push_back(std::move(ss));
                            }
                        }
                    }
                    return true;
                });
            }
            return true;
        });

        if (isRel && itemId > 0) {
            inst.id        = itemId;
            inst.guid      = guid;
            inst.slotDigit = static_cast<int>((itemId / 10) % 10);
            strncpy_s(inst.slot, ArtifactDb::SlotName(itemId), _TRUNCATE);

            const ArtifactSetDb::ItemInfo* si = ArtifactSetDb::LookupItem(itemId);
            if (si) {
                strncpy_s(inst.setName,   si->setName,   _TRUNCATE);
                strncpy_s(inst.pieceName, si->pieceName, _TRUNCATE);
                inst.rank = si->rank;
            } else {
                strncpy_s(inst.setName,   ArtifactDb::kDefaultSetName,   _TRUNCATE);
                strncpy_s(inst.pieceName, ArtifactDb::kDefaultPieceName, _TRUNCATE);
                inst.rank = rankFromAffix;
            }
            arts.push_back(std::move(inst));
        }
        return true;
    });

    return BuildJson(arts);
}

}
