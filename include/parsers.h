#pragma once
#include <cstdint>
#include <string>

namespace Weapon   { std::string OnPacket(const uint8_t* body, uint32_t len); }
namespace Artifact { std::string OnPacket(const uint8_t* body, uint32_t len); }
namespace Material { std::string OnPacket(const uint8_t* body, uint32_t len); }
namespace Prop     { std::string OnPacket(const uint8_t* body, uint32_t len); }
namespace Avatar   { std::string OnPacket(const uint8_t* body, uint32_t len); }
