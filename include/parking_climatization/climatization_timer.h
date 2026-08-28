/*
 * Copyright (C) 2020 - Volvo Car Corporation
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

#ifndef INCLUDE_PARKING_CLIMATIZATION_CLIMATIZATION_TIMER_H_
#define INCLUDE_PARKING_CLIMATIZATION_CLIMATIZATION_TIMER_H_

#include <json/json.h>
#include <cstdint>
#include <memory>
#include <string>
#include <vector>

#include "parking_climatization/climate_timer_list.h"
#include "signals/protobuf/messages/parkingclimatization/parking_climatization_timers.pb.h"

namespace vocconv {

enum class TimerStatus : uint8_t {
    Off = 0,
    On = 1,
    NotSet = 2
};

enum class RunOnDay : uint8_t {
    No = 0,
    Yes = 1,
    Done = 2
};

/**
 * Class representing a parking climatization timer.
 **/
class ClimatizationTimer {
 public:
    static constexpr size_t someip_serialized_size = 11;
    static constexpr int kDaysPerWeek = 7;

    ClimatizationTimer();
    ClimatizationTimer(TimerStatus status, uint16_t minutes_since_midnight, bool repeated,
                       const std::array<RunOnDay, kDaysPerWeek> days);
    explicit ClimatizationTimer(const remote_common::ClimateTimer& climate_timer);
    explicit ClimatizationTimer(const remote_control_ParkingClimatizationTimerObject& timer_object);

    ClimatizationTimer(const ClimatizationTimer&) = default;
    ClimatizationTimer& operator=(const ClimatizationTimer&) = default;
    ClimatizationTimer(ClimatizationTimer&&) = default;
    ClimatizationTimer& operator=(ClimatizationTimer&&) = default;

    /**
     * Equality operator.
     * \param[in] rhs the object to compare against.
     * \returns true if and only if this object and (rhs) are equal.
     **/
    bool operator==(const ClimatizationTimer& rhs) const;

    /**
     * Inequality operator.
     * \param[in] rhs the object to compare against.
     * \returns true if and only if this object and (rhs) are not equal.
     **/
    bool operator!=(const ClimatizationTimer& rhs) const;

    /**
     * Converts the state of this object into a JSON string.
     * \returns a JSON string.
     **/
    std::string ToJson() const;

    /**
     * Convert the state of this object into a ClimateTimer which is compatible with the protobuf signals.
     * \returns a JSON string.
     **/
    remote_common::ClimateTimer ToClimateTimer() const;

    /**
     * Converts the state of this timer object into a string.
     * \returns a string representation of this timer.
     **/
    std::string ToString() const;

    /**
     * Creates a ClimatizationTimer object from a JSON root object.
     * \param[in] root root object.
     * \returns a ClimatizationTimer object.
     * \note This function will throw std::runtime_exception if parsing fails
     **/
    static ClimatizationTimer FromJson(const Json::Value& root);

    /**
     * Creates a ClimatizationTimer object from a JSON V1 root object.
     * \param[in] root root object.
     * \returns a ClimatizationTimer object.
     * \note This function will throw std::runtime_exception if parsing failes
     **/
    static ClimatizationTimer FromJsonV1(const Json::Value& root);

    /**
     * Creates a ClimatizationTimer object from a JSON V2 root object.
     * \param[in] root root object.
     * \returns a ClimatizationTimer object.
     * \note This function will throw std::runtime_exception if parsing failes
     **/
    static ClimatizationTimer FromJsonV2(const Json::Value& root);

    /**
     * Converts the state of this object into a byte vector.
     * \returns timer payload as a byte vector.
     **/
    std::vector<uint8_t> SerializeSomeip() const;

    /**
     * Sets the state of this object from a byte vector.
     * \param[in] bytes specific timer payload as a byte vector.
     **/
    void DeserializeSomeip(const std::vector<uint8_t>& bytes);

    /**
     * Updates this object with other upon inequality.
     * \param[in] other the object to compare against.
     * \returns true if and only if this object was updated.
     **/
    bool UpdateIfChanged(const ClimatizationTimer& other);

    /**
     * Changes RunOnDay state for (day) from (from) to (to),
     * if (from) matches the actual value of this object.
     * \param[in] weekday actual day.
     * \param[in] from RunOnDay state to compare against.
     * \param[in] to new RunOnDay state.
     **/
    void ChangeRunOnDay(int weekday, RunOnDay from, RunOnDay to);

    /**
     * Recalculates minutes_since_midnight from local time
     * to UTC time by using (timezone_offset) and adjusts all
     * active RunOnDays either one day forward or one day backward,
     * if the calculation results in 'time gliding'.
     * \param[in] timezone_offset actual offset in minutes.
     **/
    void ToUTCTime(int32_t timezone_offset);

    /**
     * Checks whether all days of the week are non active.
     **/
    bool AreAllDaysInWeekNonActive() const;

    /**
     * Gets RunOnDay for the actual timer on the actual weekday.
     * \param[in] weekday actual weekday
     * \returns the state of the timer on that day.
     **/
    RunOnDay GetRunOnDay(int weekday) const;

    /**
     * Change timer status
     * \param[in] status the new status of this timer
     **/
    void SetStatus(const TimerStatus status);

    /**
     * Perform changes needed to to reset timer whenever all days are non active.
     * This marks days that are `Done` to `Yes` and if the timer is not repeated
     * then it changes the status to `Off` effectively marking the times as completely
     * done as all scheduled days have been handled.
     **/
    bool HandleAllDaysInactive();

    /**
     * Mark a day is done if the timer is not repeated
     * \param[in] weekday the day that should be changed from `Yes` to `Done`
     **/
    bool MarkDayAsDone(int weekday);

    TimerStatus status() const;
    bool repeated() const;
    uint16_t minutes_since_midnight() const;
    const std::array<RunOnDay, kDaysPerWeek>& days() const;

#ifndef UNIT_TESTS

 private:
#endif
    void AdjustRunOnDayBackwardOneDay();
    void AdjustRunOnDayForwardOneDay();

    TimerStatus status_;
    uint16_t minutes_since_midnight_;
    bool repeated_;
    std::array<RunOnDay, kDaysPerWeek> days_;
};

}  // namespace vocconv
#endif  // INCLUDE_PARKING_CLIMATIZATION_CLIMATIZATION_TIMER_H_
/** \} */  // end of addtogroup
