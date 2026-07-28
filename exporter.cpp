#include "include/parsers.h"
#include "include/output.h"
#include "include/io.h"

#include <string>

namespace Exporter {

void Write(const char* outDir) {
    std::string account    = Prop::ExportJson();
    std::string characters = Avatar::ExportJson();
    std::string weapons    = Weapon::ExportJson();
    std::string artifacts  = Artifact::ExportJson();
    std::string materials  = Material::ExportJson();

    std::string out;
    out.reserve(128 + account.size() + characters.size() +
                weapons.size() + artifacts.size() + materials.size());
    out += "{\n";
    out += "  \"source\": \"ky3-backpack\",\n";
    out += "  \"version\": 1,\n";
    out += "  \"account\": ";    out += account;    out += ",\n";
    out += "  \"characters\": "; out += characters; out += ",\n";
    out += "  \"weapons\": ";    out += weapons;    out += ",\n";
    out += "  \"artifacts\": ";  out += artifacts;  out += ",\n";
    out += "  \"materials\": ";  out += materials;  out += "\n";
    out += "}\n";
    IO::WriteJson(outDir, Output::kBackpack, out.c_str(), out.size());
}

}
