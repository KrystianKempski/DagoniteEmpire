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

#ifndef INCLUDE_BATTERY_CHARGE_ICHARGE_SUPPORT_SENDER_H_
#define INCLUDE_BATTERY_CHARGE_ICHARGE_SUPPORT_SENDER_H_

#include "battery_charge/charge_control_status.h"
#include "battery_charge/charging_location.h"
#include "battery_charge/target_soc_types.h"

namespace vocconv {

class IChargeSupportSender {
 public:
    virtual ~IChargeSupportSender() = default;

    virtual void SendChargeControlStatusUpdate(const ChargeControllerStatusUpdate& status) = 0;
    virtual void SendChargeControlStatusUpdateToCloud(const ChargeControllerStatusUpdate& status) = 0;
    virtual void SendChargingLocationUpdate(const charging::ChargingLocation& location, bool deleted) = 0;
    virtual void SendChargingLocationActualUpdate(const charging::ChargingLocation& location, bool valid) = 0;
    virtual void SendTargetSocUpdate(const uint8_t target_soc, const TargetSocSetting setting) = 0;
    virtual void SendTargetSocUpdateToCloud(const uint8_t target_soc, const TargetSocSetting setting) = 0;
    virtual void SendOptimizedScheduleInvalidatedUpdate(
            const charging::OptimizedChargingScheduleInvalidationReason reason) = 0;
};

}  // namespace vocconv
#endif  // INCLUDE_BATTERY_CHARGE_ICHARGE_SUPPORT_SENDER_H_
/** \} */  // end of addtogroup
