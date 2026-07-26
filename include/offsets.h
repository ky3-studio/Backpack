#pragma once
#include <cstdint>

namespace Offsets {

// BitConverter.ToInt32 — 从 [rcx+startIndex] 读取 4 字节大端 int32
static constexpr const char* kToInt32Pattern =
    "48 83 EC 28 "    // sub  rsp, 28h          函数序言
    "48 85 C9 "       // test rcx, rcx           空指针检查
    "75 ?? "          // jnz  short              跳过
    "B9 0F 00 00 00 " // mov  ecx, 0Fh           NullReferenceException 异常码
    "E8 ?? ?? ?? ?? " // call <抛异常辅助函数>   相对偏移
    "41 89 D0 "       // mov  r8d, edx            保存 startIndex
    "4C 8B 49 18 "    // mov  r9, [rcx+18h]       取数组长度字段
    "49 63 C1 "       // movsxd rax, r9d          符号扩展长度
    "4C 39 C0 "       // cmp  rax, r8             越界检查
    "7E ?? "          // jle  short              跳过
    "41 8D 41 FC "    // lea  eax, [r9-4]         4 字节边界（ToInt32 特有）
    "39 D0 "          // cmp  eax, edx            startIndex 上界检查
    "7C ?? "          // jl   short              跳过
    "41 39 D1 "       // cmp  r9d, edx            长度与 startIndex 比较
    "76 ?? "          // jbe  short              跳过
    "48 63 C2 "       // movsxd rax, edx          startIndex 符号扩展
    "F6 C2 03";       // test dl, 3              4 字节对齐检查

}

namespace Pkt {
    static constexpr int      kStartIndex   = 6;
    static constexpr uint32_t kDataOffset   = 0x20;
    static constexpr uint32_t kMagic        = 0x6745;
    static constexpr uint32_t kBodyPrefix   = 10;

    static constexpr uint16_t kCmdStore     = 25494;
    static constexpr uint16_t kCmdProp      = 2643;

    static constexpr uint32_t kMaxStoreLen  = 0x2000000u;
    static constexpr uint32_t kMaxPropLen   = 0x100000u;
}
