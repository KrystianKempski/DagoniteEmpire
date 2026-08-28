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

#ifndef INCLUDE_PARKING_CLIMATIZATION_CLIMATIZATION_TIMER_LIST_H_
#define INCLUDE_PARKING_CLIMATIZATION_CLIMATIZATION_TIMER_LIST_H_

#include <json/json.h>
#include <cstdint>
#include <memory>
#include <string>
#include <vector>

#include "parking_climatization/climatization_timer.h"
#include "parking_climatization/climate_timer_list.h"

namespace vocconv {

enum class TimerIndex : uint8_t {
    Timer0 = 1,
    Timer1 = 2,
    Timer2 = 3,
    Timer3 = 4,
    Timer4 = 5,
    Timer5 = 6,
    Timer6 = 7,
    Timer7 = 8,
    Undefined = 9
};

namespace climatization_timer_list {
constexpr size_t kSomeipSerializedSize = 89;
constexpr int kNumberOfClimateTimers = 8;
}  // climatization_timer_list

using TimerList = std::array<ClimatizationTimer, climatization_timer_list::kNumberOfClimateTimers>;

/**
 * Class representing a parking climatization timer list (calendar).
 **/
class ClimatizationTimerList {
 public:
    ClimatizationTimerList();
    ClimatizationTimerList(TimerList timers, TimerIndex next_timer, int32_t timezone_offset);
    explicit ClimatizationTimerList(const remote_common::ClimateTimerList& timer_list);

    ClimatizationTimerList(const ClimatizationTimerList&) = default;
    ClimatizationTimerList& operator=(const ClimatizationTimerList&) = default;
    ClimatizationTimerList(ClimatizationTimerList&&) = default;
    ClimatizationTimerList& operator=(ClimatizationTimerList&&) = default;

    /**
     * Equality operator.
     * \param[in] rhs the object to compare against.
     * \returns true if and only if this object and rhs are equal.
     **/
    bool operator==(const ClimatizationTimerList& rhs) const;

    /**
     * Converts the state of this object into a JSON string.
     * \returns a JSON string.
     **/
    std::string ToJson() const;

    /**
     * Convert the state of this object into a ClimateTimerList which is compatible with the protobuf signals.
     * \returns the climate timers in the ClimateTimerList format
     **/
    remote_common::ClimateTimerList ToClimateList() const;

    /**
     * Log the current climate timers to DLT.
     * \param[in] prefix a string to preface the logs with.
     **/
    void LogTimers(const std::string& prefix) const;

    /**
     * Creates a ClimatizationTimerList object from a JSON string.
     * \param[in] data JSON string.
     * \returns a ClimatizationTimerList object.
     **/
    static ClimatizationTimerList FromJsonV1(const std::string& data);
    static ClimatizationTimerList FromJsonV2(const std::string& data);
    static ClimatizationTimerList FromJsonV3(const std::string& data);

    /**
     * Converts the state of this object into someip payload represented as a byte vector.
     * \returns a byte vector.
     **/
    std::vector<uint8_t> SerializeSomeip() const;

    /**
     * Sets the state of this object from someip payload represented as a byte vector.
     * \param[in] bytes byte vector.
     **/
    void DeserializeSomeip(const std::vector<uint8_t>& bytes);

    /**
     * Updates the climatization timers of this object with
     * climatization timers of other upon inequality.
     * \param[in] other the object to compare against.
     * \returns true if and only if any climatization timer of this object was updated.
     **/
    bool UpdateIfChanged(const ClimatizationTimerList& other);

    /**
     * Get a pointer to a specific timer by timer index.
     * \param[in] index The index for the timer in the list.
     * \return A pointer to the requested timer from the list.
     **/
    ClimatizationTimer* GetTimerPtr(const TimerIndex index);

    /**
     * Saves climate timers information to a persistency .dat file
     **/
    void SaveToPersistency();

    /**
     * Reads climate timers information from a persistency .dat file
     * \return current ClimatizationTimerList.
     **/
    static ClimatizationTimerList LoadFromPersistency();

    /**
     * Recalculates the time value in all timers from local time
     * to UTC time by using the timezone offset and adjusts all
     * active RunOnDays in all timers either one day forward or one day backward,
     * if the calculation results in 'time gliding'.
     **/
    void ToUTCTime();

    /**
     * Set which timer is the closest in time
     * \param[in] timer_index the timer index of the closes expiring timer
     **/
    void SetNextTimer(TimerIndex timer_index);

    /**
     * Set the timezone offset of the climate timers
     * \param[in] timezone_offset the timezone offset in minutes
     **/
    void SetTimezoneOffset(int32_t timezone_offset);

    /**
     * Trigger update of all timers to mark completed timers as done.
     **/
    void UpdateDayStateOnNonActiveTimers();

    /**
     * Mark an expired timer as done by changing the RunOnDay from `Yes` to `Done`
     * This will also mark any other timer that expires at the same time and on the same day as done.
     * \param[in] timer_index the timer that expired and should be marked as done
     * \param[in] weekday the day that should be marked as done
     **/
    void MarkTimerDoneOnDay(TimerIndex timer_index, int weekday);

    const TimerList& timers() const;
    TimerIndex next_timer() const;
    int32_t timezone_offset() const;

#ifndef UNIT_TESTS

 private:
#endif
    TimerIndex next_timer_;
    TimerList timers_;
    int32_t timezone_offset_;
};

}  // namespace vocconv
#endif  // INCLUDE_PARKING_CLIMATIZATION_CLIMATIZATION_TIMER_LIST_H_
/** \} */  // end of addtogroup
