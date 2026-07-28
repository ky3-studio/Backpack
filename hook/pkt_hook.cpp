#include "hook.h"
#include "../include/parsers.h"
#include "../include/offsets.h"
#include "../include/sigscan.h"
#include "../include/io.h"
#include "../include/ipc.h"
#include "../include/output.h"
#include "../include/exporter.h"

#include "../third_party/MinHook/MinHook.h"
#include <intrin.h>
#include <cstring>
#include <string>
#include <atomic>

namespace Hook {

typedef int(__fastcall* FnToInt32)(uint8_t*, int);
static FnToInt32 g_orig    = nullptr;
static char      g_outDir[MAX_PATH] = {};

static std::atomic<uint8_t> g_received{0};
static std::atomic<bool>    g_finished{false};
static constexpr uint8_t kFlagStore  = 1 << 0;
static constexpr uint8_t kFlagAvatar = 1 << 1;
static constexpr uint8_t kFlagProp   = 1 << 2;
static constexpr uint8_t kFlagAll    = kFlagStore | kFlagAvatar | kFlagProp;

static void CheckFinish() {
    if ((g_received.load() & kFlagAll) != kFlagAll) return;
    if (g_finished.exchange(true)) return;
    Exporter::Write(g_outDir);
    IPC::Push("finish", "{}", 2);
    ExitProcess(0);
}

static int __fastcall Detour(uint8_t* val, int startIndex) {
    const int ret = g_orig(val, startIndex);
    if (startIndex != Pkt::kStartIndex) return ret;

    const uint8_t* p = val + Pkt::kDataOffset;
    if (*reinterpret_cast<const uint16_t*>(p) != Pkt::kMagic) return ret;

    const uint16_t cmdId   = _byteswap_ushort(*reinterpret_cast<const uint16_t*>(p + 2));
    const uint16_t headLen = _byteswap_ushort(*reinterpret_cast<const uint16_t*>(p + 4));
    const uint32_t dataLen = _byteswap_ulong (*reinterpret_cast<const uint32_t*>(p + 6));

    if (cmdId == Pkt::kCmdStore && dataLen > 0 && dataLen < Pkt::kMaxStoreLen) {
        const uint8_t* body = p + Pkt::kBodyPrefix + headLen;
        auto emit = [&](const char* ev, const char* file, const std::string& json) {
            IO::WriteJson(g_outDir, file, json.c_str(), json.size());
            IPC::Push(ev, json.c_str(), json.size());
        };
        emit("weapon",   Output::kWeapon,   Weapon::OnPacket(body, dataLen));
        emit("artifact", Output::kArtifact, Artifact::OnPacket(body, dataLen));
        emit("material", Output::kMaterial, Material::OnPacket(body, dataLen));
        g_received.fetch_or(kFlagStore);
        CheckFinish();
    }
    if (cmdId == Pkt::kCmdAvatar && dataLen > 0 && dataLen < Pkt::kMaxAvatarLen) {
        const uint8_t* body = p + Pkt::kBodyPrefix + headLen;
        const std::string json = Avatar::OnPacket(body, dataLen);
        IO::WriteJson(g_outDir, Output::kAvatar, json.c_str(), json.size());
        IPC::Push("avatar", json.c_str(), json.size());
        g_received.fetch_or(kFlagAvatar);
        CheckFinish();
    }
    if (cmdId == Pkt::kCmdProp && dataLen > 0 && dataLen < Pkt::kMaxPropLen) {
        const uint8_t* body = p + Pkt::kBodyPrefix + headLen;
        const std::string json = Prop::OnPacket(body, dataLen);
        IO::WriteJson(g_outDir, Output::kProp, json.c_str(), json.size());
        IPC::Push("prop", json.c_str(), json.size());
        g_received.fetch_or(kFlagProp);
        CheckFinish();
    }
    return ret;
}

bool Install(uintptr_t base, const char* outDir) {
    strncpy_s(g_outDir, outDir, _TRUNCATE);

    uint8_t* target = SigScan::Find(base, Offsets::kToInt32Pattern);
    if (!target) return false;

    if (MH_Initialize() != MH_OK) return false;
    if (MH_CreateHook(target,
                      reinterpret_cast<LPVOID>(Detour),
                      reinterpret_cast<LPVOID*>(&g_orig)) != MH_OK) return false;
    return MH_EnableHook(target) == MH_OK;
}

void Uninstall() {
    MH_DisableHook(MH_ALL_HOOKS);
    MH_Uninitialize();
}

}
