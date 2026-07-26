#pragma once
#include <cstdint>
#include <string>

namespace Prop {
    std::string OnPacket(const uint8_t* body, uint32_t len);
}
