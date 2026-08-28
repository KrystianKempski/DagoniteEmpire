/*
 * Copyright (C) 2023 - Volvo Car Corporation
 *
 * All Rights Reserved
 *
 * LEGAL NOTICE:  All information (including intellectual and technical concepts) contained herein is,
 * and remains, the property of Volvo Car Corporation.
 * This information is protected by copyright and may be covered by patents or patent applications
 * and include trade secrets.
 * Dissemination of this information or reproduction of this material is strictly forbidden unless
 * prior written permission is obtained from Volvo Car Corporation.
 */

/** \addtogroup VocConv
 *  \{
 */

#ifndef INCLUDE_BATTERY_CHARGE_ICHARGING_TARGET_LEVEL_CACHE_H_
#define INCLUDE_BATTERY_CHARGE_ICHARGING_TARGET_LEVEL_CACHE_H_

#include <cstdint>

namespace vocconv {

class IChargingTargetLevelCache {
 public:
    enum class SetCacheStatus {
        kValueChanged,
        kValueUnchanged,
        kPersistentStorageError,
    };
    virtual ~IChargingTargetLevelCache() = default;

    virtual SetCacheStatus SetTargetLevel(const uint8_t charge_target_level) = 0;
    virtual uint8_t GetTargetLevel() = 0;

 protected:
    IChargingTargetLevelCache() = default;
};

}  // namespace vocconv
#endif     // INCLUDE_BATTERY_CHARGE_ICHARGING_TARGET_LEVEL_CACHE_H_
/** \} */  // end of addtogroup
