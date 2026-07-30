#pragma once
#include <cstdint>
#include <cstddef>

struct MatEntry {
    uint32_t    id;
    const char* name;
    const char* category;
};

static const MatEntry kMaterialList[] = {
#include "rows/ascension.h"
#include "rows/weapon_asc.h"
#include "rows/talent.h"
#include "rows/char_exp.h"
#include "rows/weapon_enhance.h"
#include "rows/refine.h"
#include "rows/specialty.h"
#include "rows/cooking.h"
#include "rows/food.h"
#include "rows/material.h"
#include "rows/ore.h"
#include "rows/fish.h"
#include "rows/bait.h"
#include "rows/precious.h"
#include "rows/qiyu.h"
#include "rows/adventure.h"
#include "rows/emblem.h"
#include "rows/wish.h"
#include "rows/voucher_hi.h"
#include "rows/voucher_lo.h"
#include "rows/currency.h"
#include "rows/gadget.h"
#include "rows/consumable.h"
#include "rows/quest.h"
};

static constexpr size_t kMaterialCount = sizeof(kMaterialList) / sizeof(kMaterialList[0]);
