/*
 * Copyright (C) 2024 - Volvo Car Corporation
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

#ifndef INCLUDE_BATTERY_CHARGE_CHARGING_LOCATION_CONSTANTS_H_
#define INCLUDE_BATTERY_CHARGE_CHARGING_LOCATION_CONSTANTS_H_

#include "battery_charge/target_soc_types.h"
#include "utilities/itime_provider.h"

namespace vocconv {
namespace charging {

constexpr TimePoint kInvalidTime = TimePoint::max();
constexpr const float kChargeLevelUpdateThreshold = 0.5f;
constexpr const uint32_t kDefaultAmpLimit = 32;
constexpr const uint32_t kMaxAmpLimit = 48;  // This is the highest allowed amp limit. Greater values are invalid.
constexpr const uint32_t kMinAmpLimit = 6;  // This is the lowest allowed amp limit. Smaller values are invalid.
constexpr const uint32_t kDefaultMinSoC = 0;
constexpr const uint32_t kDefaultTargetSoC = 100;
constexpr TargetSocSetting kDefaultTargetSocSetting{TargetSocSetting::kCustom};
constexpr const std::size_t kMaxNumberOfOptimzedChargingTimers = 100;
constexpr uint16_t kVfcActivationTimerMs = 5000;

}  // namespace charging
}  // namespace vocconv
#endif  // INCLUDE_BATTERY_CHARGE_CHARGING_LOCATION_CONSTANTS_H_
/** \} */  // end of addtogroup
