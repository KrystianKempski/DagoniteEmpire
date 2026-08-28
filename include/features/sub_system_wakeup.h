/*
 * Copyright (C) 2022 - Volvo Car Corporation
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

#ifndef INCLUDE_FEATURES_SUB_SYSTEM_WAKEUP_H_
#define INCLUDE_FEATURES_SUB_SYSTEM_WAKEUP_H_

#include <boost/chrono/chrono.hpp>
#include <string>
#include <memory>

#include "utilities/itime_provider.h"

#include "app_framework/features/feature.h"
#include "app_framework/iiplm_controller.h"
#include "app_framework/signals/signal.h"
#include "feature_authorization_service_proxy/ifeature_authorization_service_proxy.h"

#include "timers/itimer_manager.h"
#include "local_config_reader/configs.h"

namespace vocconv {

#ifdef UNIT_TESTS
enum class CheckPoint : uint8_t {
    Idle = 0,
    ReceivedSignalIsNull,
    SignalIsBroadcastedToTransactions,
    WakeUpTransactionCreated,
    ScheduleTransactionCreated
};
#endif

class SubSystemWakeUp : public fsm::Feature {
 public:
    /**
     * @brief Creates SubSystemWakeUp feature
     * @param fas, pointer to FAS Proxy interface
     * @param timer_manager, pointer to timer manager interface used for
     * scheduling wake up transactions
     * @param time_provider, pointer to time provider interface
     * @param car_time_offset_config, reference to car time offset configuration
     */
    SubSystemWakeUp(std::shared_ptr<fas::IFeatureAuthorizationServiceProxy> fas,
                    std::shared_ptr<ITimerManager> timer_manager,
                    std::shared_ptr<remote_common::ITimeProvider> time_provider,
                    const local_config::vocconv_util_tcam1::Config& config);
    ~SubSystemWakeUp();

    SubSystemWakeUp(const SubSystemWakeUp& other) = delete;
    SubSystemWakeUp(SubSystemWakeUp&& other) = delete;
    SubSystemWakeUp& operator=(const SubSystemWakeUp& other) = delete;
    SubSystemWakeUp& operator=(SubSystemWakeUp&& other) = delete;

    /**
     * \brief Handles received signal
     * \param signal The signal to handle.
     */
    void HandleSignal(std::shared_ptr<fsm::Signal> signal) override;

    void SetIhuWakeUpTransactionTimeout(const boost::chrono::seconds& timeout_duration);
#ifdef UNIT_TESTS
    CheckPoint test_checkpoint_;
#endif

 private:
    void HandleWakeUpSignal(const std::shared_ptr<fsm::Signal>& signal);
    void HandleScheduleWakeUpRequest(const std::shared_ptr<fsm::Signal>& signal);
    void HandleIPLMControllerSignal(const std::shared_ptr<fsm::Signal>& signal);
    void CreateWakeUpTransaction(
            const std::shared_ptr<fsm::Signal>& signal,
            fsm::ResourceGroup resource_group = fsm::ResourceGroup::kIPLMResourceGroup_1);
    const std::string kFeatureName{"SubSystemWakeUp"};
    std::shared_ptr<fas::IFeatureAuthorizationServiceProxy> fas_;
    boost::chrono::seconds ihu_wakeup_transaction_timeout_;
    std::shared_ptr<ITimerManager> timer_manager_;
    std::shared_ptr<remote_common::ITimeProvider> time_provider_;
    const local_config::vocconv_util_tcam1::Config config_;
};

}  // namespace vocconv
#endif     // INCLUDE_FEATURES_SUB_SYSTEM_WAKEUP_H_
/** \} */  // end of addtogroup
