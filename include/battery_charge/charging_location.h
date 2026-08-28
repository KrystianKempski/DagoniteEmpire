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

#ifndef INCLUDE_BATTERY_CHARGE_CHARGING_LOCATION_H_
#define INCLUDE_BATTERY_CHARGE_CHARGING_LOCATION_H_

#include <chrono>
#include <cmath>
#include <cstdint>
#include <vector>
#include <string>

#include "dlt/dlt_types.h"
#include "json/value.h"

#include "battery_charge/charging_location_constants.h"
#include "charging_common/charging_location_types.h"
#include "signals/protobuf/messages/batterycharge/icup_charging.pb.h"

namespace vocconv {

using remote_common::battery_charge::ChargingTimer;
using remote_common::battery_charge::DepartureTime;
using remote_common::battery_charge::Position;

namespace charging {
/**
 * \brief Charging schedule types
 */
enum class ChargingScheduleType : uint8_t {
    kUnspecified,           //!< Smart charging departure time should be used
    kNone,                  //!< No schedule should be used
    kManualSchedule,        //!< Manual schedule should be used
    kSmartDepartureTime,    //!< Smart charging departure time should be used
};

/**
 * \brief Internal representation of the timers used in Optimized Charging Schedules for Smart Charging functionality.
 */
struct OptimizedChargingTimer {
    TimePoint start_time{};  // Unix time - UTC time since Unix epoch 1970-01-01T00:00:00Z
    TimePoint stop_time{};   // Unix time - UTC time since Unix epoch 1970-01-01T00:00:00Z
    uint32_t amp_limit{};

    OptimizedChargingTimer() = default;

    bool operator==(const OptimizedChargingTimer& other) const {
        return start_time == other.start_time &&
                stop_time == other.stop_time &&
                amp_limit == other.amp_limit;
    }

    bool operator!=(const OptimizedChargingTimer& other) const {
        return !(*this == other);
    }

    static bool Decode(pb_istream_t *stream, const pb_field_t *field, void **arg);

    static OptimizedChargingTimer FromJson(const Json::Value& json);
    Json::Value ToJson() const;

 private:
    static OptimizedChargingTimer FromJsonV1(const Json::Value& json);
};

enum class OptimizedChargingScheduleInvalidationReason {
    kUnknown,                   //!<  Default
    kExpired,                   //!<  Last timer in schedule has passed
    KChargerDisconnected,       //!<  Charger was disconnected which ends the session and invalidates the schedule
    kDriving,                   //!<  Usage mode went to driving which is fallback for session ended
    kManualScheduleActivated,   //!<  Manual schedule was selected to be used instead of departure times
};

std::string OptimizedChargingScheduleInvalidationReasonToString(OptimizedChargingScheduleInvalidationReason reason);

/**
 * \brief Charging schedule to be used in ChargingLocation
 */
struct ChargingSchedule {
    ChargingScheduleType active_schedule{ChargingScheduleType::kNone};
    std::vector<ChargingTimer> manual_schedules{};   //!< List of manual schedules (max 7)
    std::vector<DepartureTime> departure_times{};    //!< List of smart charging departure times (max 7)

    ChargingSchedule() = default;

    explicit ChargingSchedule(const remote_common::battery_charge::ChargingLocation& schedule) {
        active_schedule = schedule.is_optimised_charging_enabled ? ChargingScheduleType::kSmartDepartureTime
                                                                 : ChargingScheduleType::kManualSchedule;
        manual_schedules = schedule.charging_timers;
        departure_times = schedule.departure_times;
    }

    bool operator==(const ChargingSchedule& other) const {
        return active_schedule == other.active_schedule &&
            manual_schedules == other.manual_schedules &&
            departure_times == other.departure_times;
    }

    bool operator!=(const ChargingSchedule& other) const {
        return !(*this == other);
    }

    bool Manual() const {
        return active_schedule == ChargingScheduleType::kManualSchedule;
    }

    bool Smart() const {
        return active_schedule == ChargingScheduleType::kSmartDepartureTime;
    }

    void Log(DltLogLevelType level = DltLogLevelType::DLT_LOG_DEBUG, const std::string& prefix = "") const;

    static ChargingSchedule FromJson(const Json::Value& json);
    Json::Value ToJson() const;

    icup_charging_Schedule ToPbIhu() const;

 private:
    static ChargingSchedule FromJsonV1(const Json::Value& json);
};

/**
 * \brief Internal representation of a ChargingLocation
 */
class ChargingLocation {
 public:
    std::string uuid{};
    std::string alias{};
    bool smart_charging_supported{false};
    uint32_t amp_limit{charging::kDefaultAmpLimit};
    uint32_t minimum_soc{charging::kDefaultMinSoC};
    ChargingSchedule schedule{};
    Position position{};

    ChargingLocation() = default;

    explicit ChargingLocation(const remote_common::battery_charge::ChargingLocation& location) {
        uuid = location.uuid;
        alias = location.location_alias;

        if (location.available_optimised_charging !=
            remote_common::battery_charge::OptimisedChargingType::kUnavailable) {
            smart_charging_supported = true;
        } else {
            smart_charging_supported = false;
        }

        // If amp_limit is disabled, use default value instead of the provided value
        amp_limit = location.amp_limit_enabled ? location.amp_limit : charging::kDefaultAmpLimit;
        minimum_soc = location.min_state_of_charge;
        schedule = ChargingSchedule(location);
        position.latitude = location.position.latitude;
        position.longitude = location.position.longitude;
    }

    /**
     * @brief Determine if a position is within a radius of a position
     */
    bool IsWithinRadius(const Position& ref_pos, double radius) const {
        return position.IsWithinRadius(ref_pos, radius);
    }

    bool operator==(const ChargingLocation& other) const {
        return uuid == other.uuid &&
            alias == other.alias &&
            smart_charging_supported == other.smart_charging_supported &&
            amp_limit == other.amp_limit &&
            minimum_soc == other.minimum_soc &&
            schedule == other.schedule &&
            position == other.position;
    }

    bool EqualExceptPosition(const ChargingLocation& other) const {
        return uuid == other.uuid &&
            alias == other.alias &&
            smart_charging_supported == other.smart_charging_supported &&
            amp_limit == other.amp_limit &&
            minimum_soc == other.minimum_soc &&
            schedule == other.schedule;
    }

    bool operator!=(const ChargingLocation& other) const {
        return !(*this == other);
    }

    icup_charging_ChargingLocationUpdate ToPbIhuUpdate() const;
    icup_charging_ChargingLocationDeletedUpdate ToPbIhuDeletedUpdate() const;
    icup_charging_ChargingLocationActualUpdate ToPbIhuActualUpdate() const;
    icup_charging_ChargingLocationDefaultUpdate ToPbIhuDefaultUpdate() const;
    remote_common::battery_charge::ChargingLocation ToCloud(bool valid) const;

    void Log(DltLogLevelType level = DltLogLevelType::DLT_LOG_DEBUG, const std::string& prefix = "") const;

    static ChargingLocation FromJson(const Json::Value& json);
    Json::Value ToJson() const;

 private:
    static ChargingLocation FromJsonV1(const Json::Value& json);
};

}  // namespace charging
}  // namespace vocconv
#endif  // INCLUDE_BATTERY_CHARGE_CHARGING_LOCATION_H_
/** \} */  // end of addtogroup
