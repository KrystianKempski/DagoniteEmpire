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

#ifndef INCLUDE_FEATURES_PRE_CLEANING_H_
#define INCLUDE_FEATURES_PRE_CLEANING_H_


#include <cstdint>
#include <memory>
#include <string>

#include "pre_cleaning/pre_cleaning_status.h"
#include "utilities/itime_provider.h"

#include "app_framework/features/feature.h"
#include "feature_authorization_service_proxy/ifeature_authorization_service_proxy.h"
#include "local_config_reader/configs.h"
#include "pre_cleaning/isession.h"

namespace vocconv {

#ifdef UNIT_TESTS
enum class CheckPoint : uint8_t {
    Idle = 0,
    ReceivedSignalIsNull,
    SignalIsBroadcastedToTransactions,
    PreCleaningTransactionCreated,
    PreCleaningStatusTransactionCreated,
    PreCleaningExteriorUpdateTransactionCreated,
    ClimaInfoSignalError,
    ClngNotifSignalError,
    AirQlySignalError
};
#endif

class PreCleaning : public fsm::Feature {
 public:
    explicit PreCleaning(
            std::shared_ptr<fas::IFeatureAuthorizationServiceProxy> fas,
            std::shared_ptr<remote_common::ITimeProvider> time_provider,
            const local_config::pre_cleaning_tcam1::Config& pre_cleaning_config);
    ~PreCleaning();

    PreCleaning(const PreCleaning& other) = delete;
    PreCleaning(PreCleaning&& other) = delete;
    PreCleaning& operator=(const PreCleaning& other) = delete;
    PreCleaning& operator=(PreCleaning&& other) = delete;

    void HandleSignal(std::shared_ptr<fsm::Signal> signal) override;
    void SetStartStopTransactionTimeout(const boost::chrono::seconds& timeout_duration);

#ifdef UNIT_TESTS
    CheckPoint test_checkpoint_;
    std::shared_ptr<pre_cleaning::PreCleaningStatus> GetStatusForTest() const { return status_; }
#endif
#ifdef ENABLE_SIGNAL_INJECTION
    void SetAllowedInteriorExposedTime(int allowed_time);
    void SetPreCleaningStartTime(int64_t timestamp);
#endif

 private:
    void HandlePreCleaningStartStopRequestSignal(const std::shared_ptr<fsm::Signal>& signal);
    void HandlePreClimaData(const std::shared_ptr<fsm::Signal>& signal);
    void HandlePreCleaningData(const std::shared_ptr<fsm::Signal>& signal);
    void HandlePreClngStatus(const std::shared_ptr<fsm::Signal>& signal);
    void HandleExteriorEvent(const std::shared_ptr<fsm::Signal>& signal);
    void HandleUsageModeSignal(const std::shared_ptr<fsm::Signal>& signal);
    void HandleUpdateAllSignal(const std::shared_ptr<fsm::Signal>& signal);
    void HandleOtaInactiveSignal();

    const std::string kFeatureName{"PreCleaning"};
    std::shared_ptr<fas::IFeatureAuthorizationServiceProxy> fas_;
    boost::chrono::seconds start_stop_transaction_timeout_;
    std::shared_ptr<remote_common::ITimeProvider> time_provider_;
    std::shared_ptr<pre_cleaning::PreCleaningStatus> status_;
    std::shared_ptr<pre_cleaning::ISession> session_;
    const local_config::pre_cleaning_tcam1::Config pre_cleaning_config_;
};

}  // namespace vocconv
#endif  // INCLUDE_FEATURES_PRE_CLEANING_H_
/** \} */  // end of addtogroup
