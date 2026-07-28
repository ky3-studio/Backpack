#pragma once
#include <cstdint>
#include <string>

namespace Weapon   { std::string OnPacket(const uint8_t* body, uint32_t len); std::string ExportJson(); }
namespace Artifact { std::string OnPacket(const uint8_t* body, uint32_t len); std::string ExportJson(); }
namespace Material { std::string OnPacket(const uint8_t* body, uint32_t len); std::string ExportJson(); }
namespace Prop     { std::string OnPacket(const uint8_t* body, uint32_t len); std::string ExportJson(); }
namespace Avatar   { std::string OnPacket(const uint8_t* body, uint32_t len); std::string ExportJson(); }
