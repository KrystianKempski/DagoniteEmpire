/*
 * Copyright (C) 2021 - Volvo Car Corporation
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

#ifndef INCLUDE_PRE_CLEANING_PRE_CLEANING_INTERIOR_STATE_H_
#define INCLUDE_PRE_CLEANING_PRE_CLEANING_INTERIOR_STATE_H_

#include <functional>
#include <memory>
#include <mutex>

#include "fsm/fsm_persist_data_mgr.h"
#include "pre_cleaning/interior_reporter.h"
#include "utilities/itime_provider.h"
#include "local_config_reader/configs.h"

namespace vocconv {
namespace pre_cleaning {
namespace status {

class PreCleaningInteriorState {
 public:
    explicit PreCleaningInteriorState(
            std::shared_ptr<remote_common::ITimeProvider> time_provider,
            const local_config::pre_cleaning_tcam1::Config& pre_cleaning_config,
            bool is_clean = false);

    void SetSendUpdateCallback(std::function<void()> send_update_cb);
    int64_t GetPreCleaningLastCompletedTime();
    bool IsClean();
    bool InteriorExposed();
    bool IsCleanAfterExposed();
    void SetInteriorOpen();
    void SetInteriorClosed(bool& return_is_clean);
    void SetNewCompletedCleaning();
    void SetPreCleaningStartTime();
    int64_t GetPreCleaningStartTime();
    void StartInteriorReporter(int timeout_seconds);
    void StopInteriorReporter();
    bool IsInteriorReporterExpired();

    /**
     * \brief Destroys Interior Reporter
     */
    void ResetInteriorReporter() { interior_reporter_.reset(); }

#ifdef ENABLE_SIGNAL_INJECTION
    void SetAllowedInteriorExposedTime(int allowed_time) { kallowed_interior_exposed_time = allowed_time; }
    void SetPreCleaningStartTime(int64_t timestamp);
#endif

#ifdef UNIT_TESTS
    TimePoint& GetInteriorOpenTime() { return interior_open_time_; }
    void SetInteriorOpenTimeExpired() {
        interior_exposed_duration_ = kallowed_interior_exposed_time;
        is_clean_ = false;
    }
    void ForcePersistency() { tcam_restarted_ = true; }
    fsm::TimeoutTransactionId GetInteriorReporterTransactionId() const { return interior_reporter_->id_; }
#endif

 private:
    TimePoint Now() { return time_provider_->GetUtcNow(); }
    void ReadFromPersistency();
    void WriteToPersistency();

    int kallowed_interior_exposed_time;

    std::shared_ptr<remote_common::ITimeProvider> time_provider_;
    TimePoint last_complete_cleaning_;
    TimePoint interior_open_time_;
    TimePoint interior_closed_time_;
    TimePoint start_timestamp_;
    bool is_clean_;
    bool interior_exposed_;
    bool tcam_restarted_;
    int interior_exposed_duration_;
    int interior_reporter_duration_;
    std::shared_ptr<InteriorReporter> interior_reporter_;
    std::recursive_mutex interior_reporter_mutex_;
};

}  // namespace status
}  // namespace pre_cleaning
}  // namespace vocconv

#endif     // INCLUDE_PRE_CLEANING_PRE_CLEANING_INTERIOR_STATE_H_
/** \} */  // end of addtogroup
