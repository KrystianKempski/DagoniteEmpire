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

#ifndef INCLUDE_PRE_CLEANING_PRE_CLEANING_STATUS_H_
#define INCLUDE_PRE_CLEANING_PRE_CLEANING_STATUS_H_

#include <cstdint>
#include <memory>

#include "parking_climatization/prkg_clima_info.h"
#include "pre_cleaning/air_qly_status.h"
#include "pre_cleaning/pre_clng_notif.h"
#include "vc_message_payloads.hpp"

#include "utilities/itime_provider.h"
#include "pre_cleaning/pre_cleaning_interior_state.h"
#include "local_config_reader/configs.h"

namespace vocconv {
namespace pre_cleaning {

class PreCleaningStatus {
 public:
    explicit PreCleaningStatus(
            std::shared_ptr<remote_common::ITimeProvider> time_provider,
            const local_config::pre_cleaning_tcam1::Config& pre_cleaning_config);
    ~PreCleaningStatus();

    PreCleaningStatus(const PreCleaningStatus&) = delete;
    PreCleaningStatus& operator=(const PreCleaningStatus&) = delete;
    PreCleaningStatus(PreCleaningStatus&&) noexcept = delete;
    PreCleaningStatus& operator=(PreCleaningStatus&&) noexcept = delete;

    void Update(const remote_common::PrkgClimaInfo& status);
    void Update(const remote_common::AirQlyStatus& status);
    bool Update(const remote_common::PreClngNotif& status);
    void Update(const vc::CarUsageModeState& status);
    void Update(const vc::ResDoorLockUnlock& status);
    void Update(const vc::ResGetWindowPosition& status);
    void Clear();
    void ClearAirQlyCache();
    remote_common::AirQlyStatus GetAirQlyStatus() const;
    remote_common::PrkgClimaInfo GetPrkgClimaInfoStatus() const;
    remote_common::PreClngNotif GetPreClngNotifStatus() const;
    int64_t GetLastCompleteClngTime() const;
    vc::CarUsageModeState GetUsageMode() const;
    vc::ResDoorLockUnlock GetDoorLockStatus() const;
    int64_t GetStartClngTime() const;
    void SetStartClngTime();
    bool IsDoorOpen() const;
    bool IsWindowOpen() const;
    bool InteriorClean() const;
    bool InteriorChangedToOpen() const;
    bool InteriorChangedToClosed() const;
    void SetInteriorClean();
    void TearDown();

    void SetInteriorOpen();
    bool SetInteriorClosed();

#ifdef ENABLE_SIGNAL_INJECTION
    void SetAllowedInteriorExposedTime(int allowed_time);
    void SetPreCleaningStartTime(int64_t timestamp);
#endif

#ifdef UNIT_TESTS
    bool InteriorOpen() const;
    void SetInteriorOpenTimeExpired();
    void ForcePersistency();
    fsm::TimeoutTransactionId GetInteriorReporterTransactionId() const;
#endif

    bool IsRunning() const;
    bool DriveJustEnded() const;
    bool IsPreClngDone() const;
    bool IsPrkgClimaRunngStsActive() const;
    void SetTimeStampValid(bool valid);
    bool RunningStatusHasChanged(const remote_common::PrkgClimaInfo& new_status) const;
    bool StatusHasChangedToRunning(const remote_common::PrkgClimaInfo& new_status) const;

 private:
    struct UsageModeCache {
        vc::CarUsageModeState current_mode;
        vc::CarUsageModeState prev_mode;
    };
    mutable std::mutex mutex_;
    remote_common::PrkgClimaInfo prkg_clima_cache;
    remote_common::AirQlyStatus air_qly_cache;
    remote_common::PreClngNotif clng_notif_cache;
    mutable status::PreCleaningInteriorState interior_state;
    UsageModeCache usage_mode_cache;
    vc::ResDoorLockUnlock door_status_cache;
    vc::ResGetWindowPosition window_status_cache;
    bool time_stamp_valid{true};
};
}  // namespace pre_cleaning
}  // namespace vocconv
#endif  // INCLUDE_PRE_CLEANING_PRE_CLEANING_STATUS_H_
/** \} */  // end of addtogroup
