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

#ifndef INCLUDE_PARKING_CLIMATIZATION_CLIMATIZATION_TIMERS_STATE_H_
#define INCLUDE_PARKING_CLIMATIZATION_CLIMATIZATION_TIMERS_STATE_H_

#include <cstdint>
#include <memory>
#include <mutex>

#include "utilities/itime_provider.h"

#include "parking_climatization/climatization_timer.h"
#include "parking_climatization/climatization_timer_list.h"
#include "parking_climatization/iclimatization_timers_state.h"

namespace vocconv {

/**
 * Class managing a parking climatization timer list (calendar).
 **/
class ClimatizationTimersState : public IClimatizationTimersState {
 public:
    explicit ClimatizationTimersState(std::shared_ptr<remote_common::ITimeProvider> time_provider);
    /**
     * Creates instance of ClimatizationTimersState and populates
     * the timers from persistency, if persisted timers exist.
     **/
    ClimatizationTimersState(std::shared_ptr<remote_common::ITimeProvider> time_provider,
                             const ClimatizationTimerList& timer_list);

    ClimatizationTimersState(const ClimatizationTimersState& other) = delete;
    ClimatizationTimersState(ClimatizationTimersState&& other) = delete;
    ClimatizationTimersState& operator=(const ClimatizationTimersState& other) = delete;
    ClimatizationTimersState& operator=(ClimatizationTimersState&& other) = delete;

    /**
     * Updates ClimatizationTimerList, if changed, and, if so, saves it to persistency.
     * \param[in] timers the timers that will be checked for changes and saved.
     * \return status whether the list changed and whether the next timer changed
     **/
    UpdateStatus UpdateTimers(const ClimatizationTimerList& timers, int32_t car_time_offset = 0) override;

    /**
     * Gets currently set ClimatizationTimerList.
     * \return current ClimatizationTimerList.
     **/
    ClimatizationTimerList timers() override;

    /**
     * Searches all climatization timers for the one with the nearest execution time.
     * Might also clear and/or re-enable the RunOnDay configuration for timer days.
     * \return nearest climatization timer.
     **/
    NextExecutionTime FindNextExecutionTime(int32_t car_time_offset = 0) override;

    /**
     * Change run on day for the current day to `done` if it is currently `yes`.
     * \param[in] index the index of the timer that should be set to done.
     **/
    void MarkTimerDoneOnCurrentDay(TimerIndex index, int32_t car_time_offset = 0) override;

 private:
    /**
     *  Implementation of FindNextExecutionTime without grabbing the lock.
     **/
    NextExecutionTime UnlockedFindNextExecutionTime(int32_t car_time_offset = 0);

    /**
     * Searches one climatization timer for the day with the nearest execution time.
     * Might also re-enable the RunOnDay for timer days, if timer is repeated.
     * \param[in] timer_index actual timer index
     * \param[in] weekday current weekday
     * \param[in] minutes_since_midnight minutes sice midnight
     * \param[in/out] timer actual timer
     * \param[out] next_time nearest climatization timer
     **/
    void HandleOneTimer(TimerIndex timer_index, int weekday, uint16_t current_minutes_since_midnight,
                        const ClimatizationTimer& timer, NextExecutionTime* next_time);

    /**
     * Checks whether current day has an active timer, if so,
     * checks whether it has expired or not and should be cleared or
     * saved as the nearest timer.
     * \param[in] timer_index actual timer index
     * \param[in] weekday current weekday
     * \param[in] minutes_since_midnight minutes sice midnight
     * \param[in/out] timer actual timer
     * \param[out] next_time nearest climatization timer
     **/
    bool HandleCurrentDay(TimerIndex timer_index, int weekday, uint16_t current_minutes_since_midnight,
                          const ClimatizationTimer& timer, NextExecutionTime* next_time);

    /**
     * Checks whether a candidate time should be saved as the nearest timer.
     * \param[in] timer_index actual timer index
     * \param[in] weekday actual weekday
     * \param[in] in_next_week true, if weekday resides in next week, and false otherwise
     * \param[in] minutes_since_midnight minutes sice midnight
     * \param[out] next_time nearest climatization timer
     **/
    bool PossiblyUpdateNextExecutionTime(TimerIndex timer_index, int weekday, bool in_next_week,
                                         uint16_t minutes_since_midnight, NextExecutionTime* next_time,
                                         int days_until_expiry) const;

    /**
     * Updates the search result with a nearer timer.
     * \param[in] timer_index actual timer index
     * \param[in] weekday actual weekday
     * \param[in] in_next_week true, if weekday resides in next week, and false otherwise
     * \param[in] minutes_since_midnight minutes sice midnight
     * \param[out] next_time nearest climatization timer
     **/
    void SetNextExecutionTime(TimerIndex timer_index, int weekday, bool in_next_week, uint16_t minutes_since_midnight,
                              NextExecutionTime* next_time, int days_until_expiry) const;

    /**
     * Checks whether the remaining days of the week has active timers, if so,
     * checks whether they should be saved as the nearest timer.
     * \param[in] timer_index actual timer index
     * \param[in] weekday actual weekday
     * \param[in] timer actual timer
     * \param[out] next_time nearest climatization timer
     **/
    bool HandleRemainingDaysOfWeek(TimerIndex timer_index, int weekday, const ClimatizationTimer& timer,
                                   NextExecutionTime* next_time) const;

    /**
     * Checks whether the days in the next week has active timers, if so,
     * checks whether they should be saved as the nearest timer.
     * \param[in] timer_index actual timer index
     * \param[in] weekday current weekday
     * \param[in] timer actual timer
     * \param[out] next_time nearest climatization timer
     **/
    bool HandleDaysInWeekBeforeCurrentDay(TimerIndex timer_index, int weekday, const ClimatizationTimer& timer,
                                          NextExecutionTime* next_time) const;

    /**
     * Checks wheter timer should run on the current day and if it has already passed, if so, check wheter it should be
     *saved as the nearest timer next week.
     * \param[in] timer_index actual timer index
     * \param[in] weekday current weekday
     * \param[in] minutes_since_midnight minutes sice midnight
     * \param[in/out] timer actual timer
     * \param[out] next_time nearest climatization timer
     **/
    bool HandleCurrentDayInNextWeek(TimerIndex timer_index, int weekday, uint16_t current_minutes_since_midnight,
                                    const ClimatizationTimer& timer, NextExecutionTime* next_time) const;

    /**
     * Applies car_time_offset (minutes) to convert current UTC-derived time-of-day into car local time.
     * Updates weekday and minutes since midnight; seconds are optionally updated if provided.
     */
    void ApplyCarTimeOffset(uint8_t* weekday, uint16_t* minutes_since_midnight, uint32_t* seconds_since_midnight,
                            int32_t car_time_offset) const;

    ClimatizationTimerList timers_;
    std::mutex timers_state_api_mutex_;
    std::shared_ptr<remote_common::ITimeProvider> time_provider_;
};

}  // namespace vocconv
#endif  // INCLUDE_PARKING_CLIMATIZATION_CLIMATIZATION_TIMERS_STATE_H_
/** \} */  // end of addtogroup
