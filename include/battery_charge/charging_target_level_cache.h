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

#ifndef INCLUDE_BATTERY_CHARGE_CHARGING_TARGET_LEVEL_CACHE_H_
#define INCLUDE_BATTERY_CHARGE_CHARGING_TARGET_LEVEL_CACHE_H_

#include <mutex>

#include "battery_charge/icharging_target_level_cache.h"

namespace vocconv {

/**
 * \class ChargingTargetLevelCache
 *
 * \brief Caches Charging Target Level data
 */
class ChargingTargetLevelCache : public IChargingTargetLevelCache {
 public:
    ChargingTargetLevelCache();
    ChargingTargetLevelCache(const ChargingTargetLevelCache&) = delete;
    ChargingTargetLevelCache(ChargingTargetLevelCache&&) = delete;
    ChargingTargetLevelCache& operator=(const ChargingTargetLevelCache&) = delete;
    ChargingTargetLevelCache& operator=(ChargingTargetLevelCache&&) = delete;
    ~ChargingTargetLevelCache() = default;

    SetCacheStatus SetTargetLevel(const uint8_t charge_target_level) override;
    uint8_t GetTargetLevel() override;

 private:
    mutable std::mutex storage_mutex_;

    uint8_t charge_target_level_{100};
};

}  // namespace vocconv

#endif     // INCLUDE_BATTERY_CHARGE_CHARGING_TARGET_LEVEL_CACHE_H_
/** \} */  // end of addtogroup
