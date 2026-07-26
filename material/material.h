#pragma once
#include <cstdint>
#include <string>

namespace Material {
    std::string OnPacket(const uint8_t* body, uint32_t len);
}
