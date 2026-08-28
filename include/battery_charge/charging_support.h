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

#ifndef INCLUDE_BATTERY_CHARGE_CHARGING_SUPPORT_H_
#define INCLUDE_BATTERY_CHARGE_CHARGING_SUPPORT_H_

#include <cstdint>

#include "battery_charge/battery_charge_timer.h"

namespace vocconv {
namespace battery_charge {

void SendChargingTargetLevelToPairedDevicesAndDigitalTwin(uint8_t charging_target_level);
void SendAmperageLimitToPairedDevicesAndDigitalTwin(uint32_t amperage_limit);
void SendChargeTimerStatusToPairedDevicesAndDigitalTwin(remote_common::BatteryChargeTimer charge_timer_status);

}  // namespace battery_charge
}  // namespace vocconv
#endif  // INCLUDE_BATTERY_CHARGE_CHARGING_SUPPORT_H_
/** \} */  // end of addtogroup
