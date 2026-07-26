#include "hook.h"
#include "../weapon/weapon.h"
#include "../artifact/artifact.h"
#include "../material/material.h"
#include "../prop/prop.h"
#include "../include/offsets.h"
#include "../include/sigscan.h"
#include "../include/io.h"
#include "../include/ipc.h"
#include "../include/output.h"

#include "../third_party/MinHook/MinHook.h"
#include <intrin.h>
#include <cstring>
#include <string>

namespace Hook {

typedef int(__fastcall* FnToInt32)(uint8_t*, int);
static FnToInt32 g_orig    = nullptr;
static char      g_outDir[MAX_PATH] = {};

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
        std::string wJson = Weapon::OnPacket(body, dataLen);
        std::string aJson = Artifact::OnPacket(body, dataLen);
        std::string mJson = Material::OnPacket(body, dataLen);
        IO::WriteJson(g_outDir, Output::kWeapon,   wJson.c_str(), wJson.size());
        IO::WriteJson(g_outDir, Output::kArtifact, aJson.c_str(), aJson.size());
        IO::WriteJson(g_outDir, Output::kMaterial, mJson.c_str(), mJson.size());
        IPC::Push("weapon",   wJson.c_str(), wJson.size());
        IPC::Push("artifact", aJson.c_str(), aJson.size());
        IPC::Push("material", mJson.c_str(), mJson.size());
    }
    if (cmdId == Pkt::kCmdProp && dataLen > 0 && dataLen < Pkt::kMaxPropLen) {
        const uint8_t* body = p + Pkt::kBodyPrefix + headLen;
        std::string pJson = Prop::OnPacket(body, dataLen);
        if (!pJson.empty()) {
            IO::WriteJson(g_outDir, Output::kProp, pJson.c_str(), pJson.size());
            IPC::Push("prop", pJson.c_str(), pJson.size());
        }
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
