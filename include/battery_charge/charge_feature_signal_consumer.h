/*
 * Copyright (C) 2025 - Volvo Car Corporation
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

#ifndef INCLUDE_BATTERY_CHARGE_CHARGE_FEATURE_SIGNAL_CONSUMER_H_
#define INCLUDE_BATTERY_CHARGE_CHARGE_FEATURE_SIGNAL_CONSUMER_H_

#include <memory>
#include <vector>

#include "app_framework/signals/ccm_signal.h"
#include "app_framework/signals/signal.h"
#include "app_framework/signals/vsomeip_signal.h"
#include "feature_authorization_service_proxy/ifeature_authorization_service_proxy.h"
#include "local_config_reader/configs.h"
#include "vc_message_payloads.hpp"

#include "battery_charge/icharge_controller.h"
#include "battery_charge/icharge_feature_signal_consumer.h"

namespace vocconv {

class ChargeFeatureSignalConsumer : public IChargeFeatureSignalConsumer {
 public:
    ChargeFeatureSignalConsumer(std::shared_ptr<IChargeController> charge_controller,
                                std::shared_ptr<fas::IFeatureAuthorizationServiceProxy> fas_proxy);

    void ConsumeChargeNowRequestCcmSignal(const std::shared_ptr<fsm::CcmSignal>& signal) const override;
    void ConsumeGetChargeNowRequestSomeipSignal(const std::shared_ptr<fsm::VsomeipSignal>& signal) const override;
    void ConsumeSetChargeNowRequestSomeipSignal(const std::shared_ptr<fsm::VsomeipSignal>& signal) const override;
    void ConsumeSetTargetSocRequestCcmSignal(const std::shared_ptr<fsm::CcmSignal>& signal) const override;
    void ConsumeSetTargetSocRequestSomeipSignal(const std::shared_ptr<fsm::VsomeipSignal>& signal) const override;
    void ConsumeGetTargetSocRequestSomeipSignal(const std::shared_ptr<fsm::VsomeipSignal>& signal) const override;
    void ConsumeChargeStatusAndLevelSignals(const std::shared_ptr<fsm::Signal>& signal) override;

 private:
    remote_common::CommonResponseCode HandleChargeNowRequestCcmSignal(
            const std::shared_ptr<fsm::CcmSignal>& ccm_signal) const;
    remote_common::CommonResponseCode HandleSetChargeNowRequestSomeipSignal(
            const std::shared_ptr<fsm::VsomeipSignal>& someip_signal) const;
    remote_common::CommonResponseCode HandleSetTargetSocRequestCcmSignal(
            const std::shared_ptr<fsm::CcmSignal>& ccm_signal) const;
    remote_common::CommonResponseCode HandleSetTargetSocRequestSomeipSignal(
            const std::shared_ptr<fsm::VsomeipSignal>& someip_signal) const;
    void HandleChargeLevelSignal(std::shared_ptr<fsm::Signal> signal) const;
    void HandleChargeStatusSignal(std::shared_ptr<fsm::Signal> signal);

    std::shared_ptr<IChargeController> charge_controller_;
    std::shared_ptr<fas::IFeatureAuthorizationServiceProxy> fas_proxy_;

    vc::BatteryOnboardCharger cable_connected_state_;

    const std::shared_ptr<std::vector<uint8_t>> kDummyBytePayload{std::make_shared<std::vector<uint8_t>>(1, 0x00)};
};

}  // namespace vocconv
#endif  // INCLUDE_BATTERY_CHARGE_CHARGE_FEATURE_SIGNAL_CONSUMER_H_
/** \} */  // end of addtogroup
