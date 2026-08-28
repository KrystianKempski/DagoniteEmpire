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

#ifndef INCLUDE_BATTERY_CHARGE_OBC_SETTINGS_H_
#define INCLUDE_BATTERY_CHARGE_OBC_SETTINGS_H_

#include "timers/icharge_timer_support.h"
#include "utilities/itime_provider.h"

#include "battery_charge/charging_location_constants.h"

namespace vocconv {

/**
 * \brief The OBC settings to send to the ObcTransaction.
 *
 * The OBC settings are used to convey the settings to the ObcTransaction
 * which will then apply the settings to the OBC.
 */
struct ObcSettings {
    bool timer_active{};
    TimePoint start_time{};
    TimePoint stop_time{};
    uint8_t amp_limit{};
    uint8_t target_soc{};

    bool operator==(const ObcSettings& rhs) {
        return timer_active == rhs.timer_active && start_time == rhs.start_time && stop_time == rhs.stop_time &&
               amp_limit == rhs.amp_limit && target_soc == rhs.target_soc;
    }

    void UpdateFromOptimizedTimer(const NextOptimizedChargingTimer& optimized_timer) {
        timer_active = optimized_timer.active;
        start_time = optimized_timer.start_time;
        stop_time = optimized_timer.end_time;
        amp_limit = optimized_timer.amp_limit;
    }

    static ObcSettings DefaultObcSettings() {
        return {.timer_active = false,
                .start_time = charging::kInvalidTime,
                .stop_time = charging::kInvalidTime,
                .amp_limit = charging::kDefaultAmpLimit,
                .target_soc = charging::kDefaultTargetSoC};
    }
};

}  // namespace vocconv

#endif  // INCLUDE_BATTERY_CHARGE_OBC_SETTINGS_H_
