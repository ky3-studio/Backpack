#pragma once
#include <cstdint>
#include <cstddef>

struct MatEntry {
    uint32_t    id;
    const char* name;
    const char* category;
};

static const MatEntry kMaterialList[] = {
#include "rows/exp.h"
#include "rows/ascension.h"
#include "rows/talent.h"
#include "rows/char_lvl.h"
#include "rows/enhance.h"
#include "rows/weapon_asc.h"
#include "rows/weapon_enhance.h"
#include "rows/forging.h"
#include "rows/specialty_mondstadt.h"
#include "rows/specialty_liyue.h"
#include "rows/specialty_inazuma.h"
#include "rows/specialty_sumeru.h"
#include "rows/specialty_fontaine.h"
#include "rows/specialty_natlan.h"
#include "rows/specialty_nodkrai.h"
#include "rows/cooking.h"
#include "rows/common.h"
#include "rows/adventure.h"
#include "rows/consumable.h"
#include "rows/potion.h"
#include "rows/gadget.h"
#include "rows/sigil.h"
#include "rows/quest.h"
};

static constexpr size_t kMaterialCount = sizeof(kMaterialList) / sizeof(kMaterialList[0]);
