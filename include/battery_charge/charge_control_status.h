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

#ifndef INCLUDE_BATTERY_CHARGE_CHARGE_CONTROL_STATUS_H_
#define INCLUDE_BATTERY_CHARGE_CHARGE_CONTROL_STATUS_H_

#include <cstdint>

namespace vocconv {

enum class ChargeControlStatus {
    kUnspecified = 0,
    kChargingTowardsMinSoc,
    kManualPlugInCharging,
    kManualScheduleCharging,
    kSmartCharging,
    kChargeNowActive,
};

struct ChargeControllerStatusUpdate {
    ChargeControlStatus status;
    uint32_t amp_limit;

    friend bool operator==(const ChargeControllerStatusUpdate& lhs, const ChargeControllerStatusUpdate& rhs) {
        return lhs.status == rhs.status && lhs.amp_limit == rhs.amp_limit;
    }
};


inline const char* ChargeControlStatusToString(const ChargeControlStatus status) {
    switch (status) {
        case ChargeControlStatus::kUnspecified:
            return "kUnspecified";
        case ChargeControlStatus::kChargingTowardsMinSoc:
            return "kChargingTowardsMinSoc";
        case ChargeControlStatus::kManualPlugInCharging:
            return "kManualPlugInCharging";
        case ChargeControlStatus::kManualScheduleCharging:
            return "kManualScheduleCharging";
        case ChargeControlStatus::kSmartCharging:
            return "kSmartCharging";
        case ChargeControlStatus::kChargeNowActive:
            return "kChargeNowActive";
        default:
            return "kErrorDefault";
    }
}
}  // namespace vocconv
#endif  // INCLUDE_BATTERY_CHARGE_CHARGE_CONTROL_STATUS_H_
