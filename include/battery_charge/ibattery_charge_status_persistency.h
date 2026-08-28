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

/** \addtogroup vocconv
 *  \{
 */

#ifndef INCLUDE_BATTERY_CHARGE_IBATTERY_CHARGE_STATUS_PERSISTENCY_H_
#define INCLUDE_BATTERY_CHARGE_IBATTERY_CHARGE_STATUS_PERSISTENCY_H_

#include <cmath>
#include <cstdint>
#include <cstdio>
#include <sstream>
#include <string>

#include "vc_message_payloads.hpp"

namespace vocconv {

struct ChargeStatusData {
    float battery_charge_level = 0.0f;
    uint16_t estimated_distance_to_empty_km = 0;
    float estimated_distance_to_empty_miles = 0.0f;
    float average_energy_consumption = 0.0f;
    vc::BatteryOnboardCharger onboard_charger_handle_status =
            vc::BatteryOnboardCharger::BATTERY_ONBOARDCHARGER_DISCONNECTED;
    vc::HmiChargeLockStatus charge_connector_lock_status = vc::HmiChargeLockStatus::HMI_CHARGE_LOCK_STATUS_IDLE;
    vc::BatteryChargerState charger_state = vc::BatteryChargerState::BATTERY_CHARGERSTATE_IDLE_MODE;
    vc::BatteryChargingType charging_type = vc::BatteryChargingType::BATTERY_CHARGINGTYPE_DISCONNECTED;
    uint16_t estimated_charging_time = 0;
    bool cancel_hold_charge_request = false;
    vc::HldChargingReq hold_charge_request = vc::HldChargingReq::HLD_CHARGING_ALLOW;
    uint32_t charging_power_watts = 0;
    int16_t charging_current = 0;
    uint16_t charging_voltage = 0;

    bool operator==(const ChargeStatusData& other) const {
        return battery_charge_level == other.battery_charge_level &&
               estimated_distance_to_empty_km == other.estimated_distance_to_empty_km &&
               estimated_distance_to_empty_miles == other.estimated_distance_to_empty_miles &&
               average_energy_consumption == other.average_energy_consumption &&
               onboard_charger_handle_status == other.onboard_charger_handle_status &&
               charge_connector_lock_status == other.charge_connector_lock_status &&
               charger_state == other.charger_state && charging_type == other.charging_type &&
               estimated_charging_time == other.estimated_charging_time &&
               cancel_hold_charge_request == other.cancel_hold_charge_request &&
               hold_charge_request == other.hold_charge_request &&
               charging_power_watts == other.charging_power_watts &&
               charging_current == other.charging_current && charging_voltage == other.charging_voltage;
    }

    bool operator!=(const ChargeStatusData& other) const { return !operator==(other); }

    std::string ToString() const {
        std::stringstream ss;
        ss << "battery_charge_level:" << battery_charge_level
           << ", estimated_distance_to_empty_km:" << estimated_distance_to_empty_km
           << ", estimated_distance_to_empty_miles:" << estimated_distance_to_empty_miles
           << ", average_energy_consumption:" << average_energy_consumption
           << ", onboard_charger_handle_status:" << onboard_charger_handle_status
           << ", charge_connector_lock_status:" << charge_connector_lock_status << ", charger_state:" << charger_state
           << ", estimated_charging_time:" << estimated_charging_time
           << ", cancel_hold_charge_request:" << cancel_hold_charge_request
           << ", hold_charge_request:" << hold_charge_request
           << ", charging_power_watts:" << charging_power_watts
           << ", charging_current:" << charging_current << ", charging_voltage:" << charging_voltage;
        return ss.str();
    }
};

/**
 * \class IBatteryChargeStatusPersistency
 */
class IBatteryChargeStatusPersistency {
 public:
    virtual ~IBatteryChargeStatusPersistency() = default;

    IBatteryChargeStatusPersistency(const IBatteryChargeStatusPersistency&) = delete;
    IBatteryChargeStatusPersistency(IBatteryChargeStatusPersistency&&) = delete;
    IBatteryChargeStatusPersistency& operator=(const IBatteryChargeStatusPersistency&) = delete;
    IBatteryChargeStatusPersistency& operator=(IBatteryChargeStatusPersistency&&) = delete;

    virtual bool Store(const ChargeStatusData& status_data) = 0;
    virtual ChargeStatusData Read() const = 0;

 protected:
    IBatteryChargeStatusPersistency() = default;
};

}  // namespace vocconv

#endif  // INCLUDE_BATTERY_CHARGE_IBATTERY_CHARGE_STATUS_PERSISTENCY_H_
/** \} */  // end of addtogroup
