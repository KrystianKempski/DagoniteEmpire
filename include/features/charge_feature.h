/*
 * Copyright (C) 2024 - Volvo Car Corporation
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

#ifndef INCLUDE_FEATURES_CHARGE_FEATURE_H_
#define INCLUDE_FEATURES_CHARGE_FEATURE_H_

#include <boost/chrono/chrono.hpp>

#include <string>
#include <memory>

#include "app_framework/features/feature.h"
#include "app_framework/signals/signal.h"
#include "feature_authorization_service_proxy/ifeature_authorization_service_proxy.h"
#include "local_config_reader/config_loader.h"
#include "utilities/iasync_function_executor.h"
#include "utilities/itime_provider.h"
#include "vc_message_payloads.hpp"

#include "battery_charge/icharge_controller.h"
#include "utilities/imqtt_wrapper.h"
#include "battery_charge/icharge_feature_signal_consumer.h"
#include "battery_charge/icharge_support_sender.h"
#include "transactions/set_obc_charging_properties_transaction.h"

namespace vocconv {

#ifdef UNIT_TESTS
enum class CheckPoint : uint8_t {
    Idle = 0,
    ReceivedSignalIsNull,
    ChargingLocationTransactionCreated,
    SetSmartTimersRequestTransactionCreated,
    SetObcChargingPropertiesTransactionCreated,
    SignalIsBroadcastedToTransactions,
    ChargeStatusRequestedFromVgm,
    GetAllChargingLocationsTransactionCreated,
};
#endif

class ChargeFeature : public fsm::Feature {
 public:
    ChargeFeature(
            std::shared_ptr<remote_common::IAsyncFunctionExecutor> async_function_executor,
            std::shared_ptr<IChargeFeatureSignalConsumer> charge_feature_signal_consumer,
            std::shared_ptr<IChargeSupportSender> charge_support_sender,
            std::shared_ptr<IChargeController> charge_controller,
            std::shared_ptr<fas::IFeatureAuthorizationServiceProxy> fas_proxy,
            std::shared_ptr<remote_common::ITimeProvider> time_provider,
            std::shared_ptr<remote_common::IMqttWrapper> mqtt_wrapper,
            const local_config::charge_locations_tcam1::Config& config);
    ~ChargeFeature();

    ChargeFeature(const ChargeFeature& other) = delete;
    ChargeFeature(ChargeFeature&& other) = delete;
    ChargeFeature& operator=(const ChargeFeature& other) = delete;
    ChargeFeature& operator=(ChargeFeature&& other) = delete;

    /**
     * \brief Handles received signal
     * \param signal The signal to handle.
     */
    void HandleSignal(std::shared_ptr<fsm::Signal> signal) override;

#ifdef UNIT_TESTS
    CheckPoint test_checkpoint_;
#endif

 private:
    void EnqueueObcSettingsToSetObcTransaction(const std::shared_ptr<fsm::Signal>& signal);
    void CreateSetOptimizedChargingScheduleTransaction(const std::shared_ptr<fsm::Signal>& signal);
    void HandleBatteryChargeStatus(const std::shared_ptr<fsm::Signal>& signal);
    void HandleChargeNowRequestCcmSignal(const std::shared_ptr<fsm::Signal>& signal);
    void HandleGetChargeNowRequestSomeipSignal(const std::shared_ptr<fsm::Signal>& signal);
    void HandleSetChargeNowRequestSomeipSignal(const std::shared_ptr<fsm::Signal>& signal);
    void CreateChargingLocationTransaction(const std::shared_ptr<fsm::Signal>& signal);
    void CreateGetAllChargingLocationsTransaction(const std::shared_ptr<fsm::Signal>& signal);
    void EnqueueVgmAvailabilityFunction(const std::shared_ptr<fsm::Signal>& signal);
    void HandleUsageModeSignal(const std::shared_ptr<fsm::Signal>& signal);
    void ReceivedVsomeipAck(const std::shared_ptr<fsm::Signal>& signal) const;
    void HandleExpiredChargingTimer();
    void HandleOtaAndWorkshopModeInactiveSignal();
    void CarTimeOffsetUpdated();
    void HandleSetTargetSocRequestCcmSignal(const std::shared_ptr<fsm::Signal>& signal);
    void HandleSetTargetSocRequestSomeipSignal(const std::shared_ptr<fsm::Signal>& signal);
    void HandleGetTargetSocRequestSomeipSignal(const std::shared_ptr<fsm::Signal>& signal);
    void HandleUpdateAllSignal();

    /**
     * \brief Checks if OTA or Workshop mode is active
     * \returns true if OTA mode or Workshop mode is on, otherwise false.
     */
    bool IsOtaOrWorkshopModeOn();

    const std::string kFeatureName{"ChargeFeature"};

    std::shared_ptr<remote_common::IAsyncFunctionExecutor> async_function_executor_;
    std::shared_ptr<IChargeFeatureSignalConsumer> charge_feature_signal_consumer_;
    std::shared_ptr<IChargeSupportSender> charge_support_sender_;
    std::shared_ptr<IChargeController> charge_controller_;
    std::shared_ptr<fas::IFeatureAuthorizationServiceProxy> feature_authorization_service_;
    std::shared_ptr<SetObcChargingPropertiesTransaction> set_obc_charging_properties_transaction_;
    vc::CarUsageModeState usage_mode_{vc::CarUsageModeState::CAR_ABANDONED};
    std::shared_ptr<remote_common::ITimeProvider> time_provider_;
    std::shared_ptr<remote_common::IMqttWrapper> mqtt_wrapper_{};
};

}  // namespace vocconv
#endif     // INCLUDE_FEATURES_CHARGE_FEATURE_H_
/** \} */  // end of addtogroup
