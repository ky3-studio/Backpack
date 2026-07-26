#pragma once
#include <cstdint>

struct NamedProp {
    uint32_t    type;
    const char* key;
    bool        div100;
};

static constexpr NamedProp kWanted[] = {
    { 10015, "原石",     false },
    { 10016, "摩拉",     false },
    { 10025, "创世结晶", false },
    { 10020, "原粹树脂", false },
    { 10042, "洞天宝钱", false },
    { 10013, "冒险等阶", false },
    { 10014, "冒险阅历", false },
    { 10019, "世界等级", false },
    { 10010, "当前体力", true  },
    { 10011, "体力上限", true  },
};

static constexpr int kWantedCount   = sizeof(kWanted) / sizeof(kWanted[0]);
static constexpr int kMaxEntryCount = 128;
