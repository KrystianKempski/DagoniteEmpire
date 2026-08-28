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

#ifndef INCLUDE_FEATURES_PARKING_CLIMATIZATION_H_
#define INCLUDE_FEATURES_PARKING_CLIMATIZATION_H_

#include <boost/chrono/chrono.hpp>

#include <memory>

#include "app_framework/features/feature.h"
#include "common/icar_config_util.h"
#include "feature_authorization_service_proxy/ifeature_authorization_service_proxy.h"
#include "utilities/itime_provider.h"

#include "common/requestor.h"
#include "local_config_reader/configs.h"
#include "parking_climatization/iclimatization_timers_state.h"
#include "parking_climatization/latest_climate_status.h"
#include "timers/itimer_manager.h"

namespace vocconv {

constexpr char const* kParkingClimatizationFeatureName = "ParkingClimatization";

#ifdef UNIT_TESTS
enum class HandleSignalResult : uint8_t {
    ReceivedSignalIsNull = 0,
    TransactionForParkClimaDirectStartStopCreated,
    TransactionForPrkgClimaStsSignalsCreated,
    TransactionForSetParkClimaTimerListCreated,
    TransactionForGetParkClimaTimerListCreated,
    TransactionForGetParkingClimatizationStatusCreated,
    TransactionForScheduleNextTimerCreated,
    TransactionForCarTimeOffsetUpdateCreated,
    BroadcastToTransactionsCarUsageMode,
    BroadcastToTransactionsGetPropulsionType,
    BroadcastToTransactionsVfcActivationAckResponse,
    NoTransactionCreated
};
#endif

class ParkingClimatization : public fsm::Feature {
 public:
    explicit ParkingClimatization(
            std::shared_ptr<IClimatizationTimersState> climatization_timers,
            std::shared_ptr<fas::IFeatureAuthorizationServiceProxy> feature_authorization_service,
            std::shared_ptr<ITimerManager> timer_manager,
            std::shared_ptr<remote_common::ICarConfigUtil> car_config,
            std::shared_ptr<remote_common::ITimeProvider> time_provider,
            const local_config::parking_climatization_tcam1::Config& parking_climatization_config);
    ~ParkingClimatization();

    ParkingClimatization(const ParkingClimatization& other) = delete;
    ParkingClimatization(ParkingClimatization&& other) = delete;
    ParkingClimatization& operator=(const ParkingClimatization& other) = delete;
    ParkingClimatization& operator=(ParkingClimatization&& other) = delete;

    /**
     * \brief Handle a signal.
     * \param[in] signal The signal to handle.
     * \return None.
     **/
    void HandleSignal(std::shared_ptr<fsm::Signal> signal) override;

    /**
     * \brief Setup parking climatization start/stop transaction timeout.
     * \param[in] timeout_duration timeout for transaction in seconds.
     * \return None.
     **/
    void SetStartStopTransactionTimeout(const boost::chrono::seconds& timeout_duration);

#ifdef ENABLE_SIGNAL_INJECTION
    void TestTeardownClimaTimerList();

    /**
     * \brief Override the timeout used for the timer-triggered SetClimatizationTimersTransaction.
     * \param[in] timeout_duration timeout for the transaction in seconds.
     * \return None.
     **/
    void SetClimatizationTimersTransactionTimeout(const boost::chrono::seconds& timeout_duration);

    /**
     * \brief Check whether an active SetClimatizationTimersTransaction exists.
     * \return True if an active SetClimatizationTimersTransaction exists, otherwise false.
     **/
    bool ActiveSetClimatizationTimersTransactionExists();
#endif

 private:
#ifdef UNIT_TESTS

 public:
    HandleSignalResult signal_recv_result_ = HandleSignalResult::NoTransactionCreated;
#endif

    /**
     * \brief Check if start or stop signal has been received from mobile application
     * and create proper transaction.
     * \param[in] signal direct start/stop request signal from mobile app.
     * \return None.
     **/
    void HandleParkClimaDirectStartStopRequestSignal(const std::shared_ptr<fsm::Signal>& signal);
    void HandleIPLMControllerSignal(const std::shared_ptr<fsm::Signal>& signal);
    /**
     * \brief Handle PrkgClimaInfoSts and PrkgClimaNotifSts signals received from CCM.
     * \param[in] signal received from CCM.
     * \return None.
     **/
    void HandleParkingClimateStatus(const std::shared_ptr<fsm::Signal>& signal);
    void HandleSetParkClimaTimerList(const std::shared_ptr<fsm::Signal>& signal, Requestor requestor);
    void HandleGetParkClimaTimerList(const std::shared_ptr<fsm::Signal>& signal, Requestor requestor);
    void HandleSendNextTimerToCCM(const std::shared_ptr<fsm::Signal>& signal, Requestor requestor);
    void HandleVfcActivationSignal(const std::shared_ptr<fsm::Signal>& signal);
    void HandleGetPropulsionTypeSignal(const std::shared_ptr<fsm::Signal>& signal);
    void HandleCarUsageModeSignal(const std::shared_ptr<fsm::Signal>& signal);
    void HandleGetParkingClimatizationInfo(const std::shared_ptr<fsm::Signal>& signal);
    void HandleGetPreClimatizationDataSignal(const std::shared_ptr<fsm::Signal>& signal);
    void HandleEventPreClimatizationData(const std::shared_ptr<fsm::Signal>& signal);
    void HandlePreClimatizationData(const vc::ResGetPreClimatizationData& data);
    void HandleCarTimeOffsetUpdate(const std::shared_ptr<fsm::Signal>& signal);
    void HandleUpdateAllSignal(const std::shared_ptr<fsm::Signal>& signal);
    vc::VFCType GetVfcTypeForClimate();

    template<typename T>
    bool ActiveTransactionExists();

    std::shared_ptr<IClimatizationTimersState> climatization_timers_;
    std::shared_ptr<ITimerManager> timer_manager_;
    std::shared_ptr<fas::IFeatureAuthorizationServiceProxy> feature_authorization_service_;
    std::shared_ptr<LatestClimateStatus> latest_climate_status_;
    std::shared_ptr<remote_common::ICarConfigUtil> car_config_;
    boost::chrono::seconds start_stop_transaction_timeout_;
    std::atomic<bool> status_transaction_created_;
    std::shared_ptr<remote_common::ITimeProvider> time_provider_;
    local_config::parking_climatization_tcam1::Config parking_climatization_config_;
};

}  // namespace vocconv
#endif  // INCLUDE_FEATURES_PARKING_CLIMATIZATION_H_
/** \} */  // end of addtogroup
