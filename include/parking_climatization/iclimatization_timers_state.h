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

#ifndef INCLUDE_PARKING_CLIMATIZATION_ICLIMATIZATION_TIMERS_STATE_H_
#define INCLUDE_PARKING_CLIMATIZATION_ICLIMATIZATION_TIMERS_STATE_H_

#include "parking_climatization/climatization_timer_list.h"

namespace vocconv {

class IClimatizationTimersState {
 public:
    virtual ~IClimatizationTimersState() {}

    IClimatizationTimersState(const IClimatizationTimersState& other) = delete;
    IClimatizationTimersState(IClimatizationTimersState&& other) = delete;
    IClimatizationTimersState& operator=(const IClimatizationTimersState& other) = delete;
    IClimatizationTimersState& operator=(IClimatizationTimersState&& other) = delete;

    struct NextExecutionTime {
        bool valid;
        TimerIndex timer_index;
        int weekday;
        bool in_next_week;
        uint16_t minutes_since_midnight;
        int days_until_expiry;
        uint32_t seconds_until_expiry;

        NextExecutionTime();

        NextExecutionTime(const NextExecutionTime&) = default;
        NextExecutionTime& operator=(const NextExecutionTime&) = default;
        NextExecutionTime(NextExecutionTime&&) = default;
        NextExecutionTime& operator=(NextExecutionTime&&) = default;

        bool operator==(const NextExecutionTime& rhs) const;
        bool operator!=(const NextExecutionTime& rhs) const;
    };
    enum class UpdateStatus : uint8_t {
        kListUnchanged = 0,
        kListChangedButNextTimerUnchanged,
        kListChangedAndNextTimerChanged
    };

    virtual NextExecutionTime FindNextExecutionTime(int32_t car_time_offset) = 0;
    virtual UpdateStatus UpdateTimers(const ClimatizationTimerList& timers, int32_t car_time_offset) = 0;
    virtual ClimatizationTimerList timers() = 0;
    virtual void MarkTimerDoneOnCurrentDay(const TimerIndex index, int32_t car_time_offset) = 0;

 protected:
    IClimatizationTimersState() = default;
};

}  // namespace vocconv
#endif  // INCLUDE_PARKING_CLIMATIZATION_ICLIMATIZATION_TIMERS_STATE_H_
/** \} */  // end of addtogroup
