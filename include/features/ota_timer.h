/*
 * Copyright (C) 2019 - Volvo Car Corporation
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

#ifndef INCLUDE_FEATURES_OTA_TIMER_H_
#define INCLUDE_FEATURES_OTA_TIMER_H_

#include <boost/chrono/chrono.hpp>
#include <memory>
#include <vector>

#include "feature_authorization_service_proxy/ifeature_authorization_service_proxy.h"
#include "app_framework/features/feature.h"
#include "utilities/itime_provider.h"

#include "ota_installation/ota_installation_state.h"
#include "timers/itimer_manager.h"
#include "diagnostics/ota_timer_exit_causes.h"

namespace vocconv {

constexpr char const* kOtaTimerFeatureName = "OTATimer";
constexpr boost::chrono::seconds kOtaStsTimeoutDuration(300);
constexpr int64_t kOtaMaxExpirationTimeInSec(900);

#ifdef UNIT_TESTS
enum class HandleOtaTimerSignalResult : uint8_t {
    TransactionForSetOtaTimerCreated,
    TransactionForGetOtaTimerCreated,
    TransactionForOtaInstallStsCreated,
    TransactionForGetOtaInstallationStatusCreated,
    NoTransactionCreated,
    BroadcastToTransactionsIplmResourceGroupActive,
    BroadcastToTransactionsOtaInstallationInitiated
};
#endif

class OtaTimer : public fsm::Feature {
 public:
    /**
     * Create OtaTimer feature, which handles whole communication with IHU
     * related to scheduling OTA timer.
     * \param timer_manager pointer to TimerManager instance that is used by the
     * feature for scheduling OTA timer.
     * \param ota_installation pointer to OtaInstallationState instance, which
     * is used by the feature to keep track of OTA installation state.
     * \param time_provider Pointer to time provider instance.
    **/
    OtaTimer(std::shared_ptr<ITimerManager> timer_manager, std::shared_ptr<OtaInstallationState> ota_installation,
             std::shared_ptr<fas::IFeatureAuthorizationServiceProxy> fas_proxy, const bool ota_mode_enabled,
             std::shared_ptr<remote_common::ITimeProvider> time_provider);
    ~OtaTimer();

    OtaTimer(const OtaTimer& other) = delete;
    OtaTimer(OtaTimer&& other) = delete;
    OtaTimer& operator=(const OtaTimer& other) = delete;
    OtaTimer& operator=(OtaTimer&& other) = delete;

    /**
     * Handle signals received by this feature.
     * \param signal The signal to handle.
     **/
    void HandleSignal(std::shared_ptr<fsm::Signal> signal) override;

 private:
#ifdef UNIT_TESTS

 public:
    HandleOtaTimerSignalResult signal_recv_result_ = HandleOtaTimerSignalResult::NoTransactionCreated;
#endif

    /**
     * Executes when rtc timer has expired.
     * \param signal OtaAssignInstallationTimerExpiredSignal to handle.
     **/
    bool OtaAssignInstallationTimerExpired(const std::shared_ptr<fsm::Signal>& signal);

    void CreateSetOtaTimerTransaction(const std::shared_ptr<fsm::Signal>& signal);
    void CreateGetOtaTimerTransaction(const std::shared_ptr<fsm::Signal>& signal);

    /**
     * Executes when IPLM notification is received. Broadcasts the signal
     * to active transactions
     * \param signal The signal to handle.
     **/
    void BroadcastIPLMControllerRGActiveSignal(const std::shared_ptr<fsm::Signal>& signal);

    void CreateOtaInstallStsTransaction(const std::shared_ptr<fsm::Signal>& signal);
    void CreateGetOtaInstallationStatusTransaction(const std::shared_ptr<fsm::Signal>& signal);

    /**
     * Executes when OTA Install Status notification with value Installation Initiated is received.
     * Broadcasts the signal to active transactions.
     * \param signal The signal to handle.
     **/
    void BroadcastOtaInstallationInitiated(const std::shared_ptr<fsm::Signal>& signal);

    void SaveDiagnostics(uint32_t ota_timer_timestamp, uint32_t timer_expired_timestamp, vector<uint8_t> data,
                         OTATimerExitCause exit_cause);

    std::shared_ptr<ITimerManager> timer_manager_;
    std::shared_ptr<OtaInstallationState> ota_installation_;
    std::shared_ptr<fas::IFeatureAuthorizationServiceProxy> fas_proxy_;
    bool ota_mode_enabled_;
    std::shared_ptr<remote_common::ITimeProvider> time_provider_;
};

}  // namespace vocconv
#endif  // INCLUDE_FEATURES_OTA_TIMER_H_
/** \} */  // end of addtogroup
