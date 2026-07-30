#include "hook.h"

static DWORD WINAPI MainThread(LPVOID pSelf) {
    while (!FindWindowA("UnityWndClass", nullptr)) Sleep(100);
    Sleep(8000);

    HMODULE hMod = GetModuleHandleA(nullptr);
    if (!hMod) return 0;

    char outDir[MAX_PATH];
    GetModuleFileNameA(hMod, outDir, MAX_PATH);
    char* slash = strrchr(outDir, '\\');
    if (slash) *(slash + 1) = '\0';
    strcat_s(outDir, "output\\");
    CreateDirectoryA(outDir, nullptr);

    Hook::Install(reinterpret_cast<uintptr_t>(hMod), outDir);

    while (true) Sleep(10000);
    return 0;
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD reason, LPVOID) {
    if (reason == DLL_PROCESS_ATTACH) {
        DisableThreadLibraryCalls(hModule);
        CreateThread(nullptr, 0, MainThread, reinterpret_cast<LPVOID>(hModule), 0, nullptr);
    } else if (reason == DLL_PROCESS_DETACH) {
        Hook::Uninstall();
    }
    return TRUE;
}
