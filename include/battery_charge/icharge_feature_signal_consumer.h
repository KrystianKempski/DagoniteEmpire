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

#ifndef INCLUDE_BATTERY_CHARGE_ICHARGE_FEATURE_SIGNAL_CONSUMER_H_
#define INCLUDE_BATTERY_CHARGE_ICHARGE_FEATURE_SIGNAL_CONSUMER_H_

#include <memory>

#include "app_framework/signals/ccm_signal.h"
#include "app_framework/signals/signal.h"
#include "app_framework/signals/vsomeip_signal.h"

namespace vocconv {

class IChargeFeatureSignalConsumer {
 public:
    virtual ~IChargeFeatureSignalConsumer() = default;

    IChargeFeatureSignalConsumer(const IChargeFeatureSignalConsumer&) = delete;
    IChargeFeatureSignalConsumer(IChargeFeatureSignalConsumer&&) = delete;
    IChargeFeatureSignalConsumer& operator=(const IChargeFeatureSignalConsumer&) = delete;
    IChargeFeatureSignalConsumer& operator=(IChargeFeatureSignalConsumer&&) = delete;

    /**
     * \brief Handles incoming ChargeNow request from cloud and sends a response.
     *
     * If the request is ok and allowed by FAS, this sets ChargeNow on/off in the ChargeController.
     */
    virtual void ConsumeChargeNowRequestCcmSignal(const std::shared_ptr<fsm::CcmSignal>& signal) const = 0;

    /**
     * \brief Handles incoming GetChargeNow request from IHU and sends a response.
     *
     * This get request has no specified payload and the someip payload is therefore ignored.
     * This means that the request will not fail even if the payload contains some bytes.
     *
     * No response is sent if the signal is not a someip signal, or if the someip message type is not request.
     */
    virtual void ConsumeGetChargeNowRequestSomeipSignal(const std::shared_ptr<fsm::VsomeipSignal>& signal) const = 0;

    /**
     * \brief Handles incoming SetChargeNow request from IHU and sends a response.
     *
     * If the request is ok, this sets ChargeNow on/off in the ChargeController.
     *
     * No response is sent if the signal is not a someip signal, or if the someip message type is not request.
     */
    virtual void ConsumeSetChargeNowRequestSomeipSignal(const std::shared_ptr<fsm::VsomeipSignal>& signal) const = 0;

    /**
     * \brief Handles incoming SetTargetSoC request from cloud and sends a response.
     *
     * If the request is ok and allowed by FAS, this sets TargetSoc in the ChargeController.
     */
    virtual void ConsumeSetTargetSocRequestCcmSignal(const std::shared_ptr<fsm::CcmSignal>& signal) const = 0;

    /**
     * \brief Handles incoming SetTargetSoC request from IHU and sends a response.
     *
     * If the request is ok, this sets TargetSoc in the ChargeController.
     */
    virtual void ConsumeSetTargetSocRequestSomeipSignal(const std::shared_ptr<fsm::VsomeipSignal>& signal) const = 0;

    /**
     * \brief Handles incoming GetTargetSoC request from IHU and sends a response.
     *
     * If the request is ok, this gets TargetSoc value from the ChargeController and sends it as a response.
     *
     * No response is sent if the signal is not a someip signal, or if the someip message type is not request.
     */
    virtual void ConsumeGetTargetSocRequestSomeipSignal(const std::shared_ptr<fsm::VsomeipSignal>& signal) const = 0;

    virtual void ConsumeChargeStatusAndLevelSignals(const std::shared_ptr<fsm::Signal>& signal) = 0;

 protected:
    IChargeFeatureSignalConsumer() = default;
};

}  // namespace vocconv
#endif  // INCLUDE_BATTERY_CHARGE_ICHARGE_FEATURE_SIGNAL_CONSUMER_H_
/** \} */  // end of addtogroup
