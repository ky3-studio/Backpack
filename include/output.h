#pragma once

namespace Output {

static constexpr const char* kWeapon   = "weapon_bag.json";
static constexpr const char* kArtifact = "artifact_bag.json";
static constexpr const char* kMaterial = "material_bag.json";
static constexpr const char* kProp     = "prop_bag.json";

static constexpr const char* kWeaponHeader   = "{\n  \"weapons\": [\n";
static constexpr const char* kArtifactHeader = "{\n  \"artifacts\": [\n";
static constexpr const char* kMaterialHeader = "{\n  \"materials\": [\n";
static constexpr const char* kArrayFooter    = "  ]\n}\n";

static constexpr const char* kWeaponItem =
    "    { \"id\": %u, \"guid\": \"%llu\", \"name\": \"%s\", \"type\": \"%s\","
    " \"rank\": %u, \"specialProp\": \"%s\","
    " \"level\": %u, \"promote\": %u, \"refine\": %u }%s\n";

static constexpr const char* kMaterialItem =
    "    { \"id\": %u, \"name\": \"%s\", \"category\": \"%s\", \"count\": %llu }%s\n";

static constexpr const char* kArtId            = "      \"id\": %u,\n";
static constexpr const char* kArtGuid          = "      \"guid\": \"%llu\",\n";
static constexpr const char* kArtSetName       = "      \"setName\": \"%s\",\n";
static constexpr const char* kArtName          = "      \"name\": \"%s\",\n";
static constexpr const char* kArtSlot          = "      \"slot\": \"%s\",\n";
static constexpr const char* kArtEquipped      = "      \"locked\": %s,\n";
static constexpr const char* kArtLevel         = "      \"level\": %d,\n";
static constexpr const char* kArtRank          = "      \"rank\": %d,\n";
static constexpr const char* kArtMainStat      =
    "      \"mainStat\": { \"type\": \"%s\", \"typeRaw\": \"%s\" },\n";
static constexpr const char* kArtSubStatHead   =
    "        { \"type\": \"%s\", \"typeRaw\": \"%s\", \"value\": %s, \"rolls\": [";
static constexpr const char* kArtSubStatTail   = "]}%s\n";
static constexpr const char* kArtClose         = "    }%s\n";
static constexpr const char* kArtItemOpen      = "    {\n";
static constexpr const char* kArtSubStatsOpen  = "      \"subStats\": [\n";
static constexpr const char* kArtSubStatsClose = "      ]\n";

static constexpr const char* kArtSubStatFmtPct = "%.1f";
static constexpr const char* kArtSubStatFmtInt = "%.0f";

}
