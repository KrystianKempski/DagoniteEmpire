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

#ifndef INCLUDE_FEATURES_CHARGE_STATUS_FEATURE_H_
#define INCLUDE_FEATURES_CHARGE_STATUS_FEATURE_H_

#include <memory>

#include "feature_authorization_service_proxy/ifeature_authorization_service_proxy.h"
#include "app_framework/features/feature.h"
#include "utilities/iasync_function_executor.h"

#include "battery_charge/ibattery_charge_status_storage.h"
#include "local_config_reader/configs.h"
#include "transactions/charge_status_update_transaction.h"

namespace vocconv {

/**
 * \class ChargeStatusFeature
 * \brief Feature that handles Charge Status signals.
 */
class ChargeStatusFeature : public fsm::Feature {
 public:
    ChargeStatusFeature(
            std::shared_ptr<remote_common::IAsyncFunctionExecutor> async_function_executor,
            std::shared_ptr<IBatteryChargeStatusStorage> charge_status_storage,
            std::shared_ptr<fas::IFeatureAuthorizationServiceProxy> feature_authorization,
            const local_config::charge_status_tcam1::Config& config);
    ~ChargeStatusFeature();

    ChargeStatusFeature(const ChargeStatusFeature& other) = delete;
    ChargeStatusFeature(ChargeStatusFeature&& other) = delete;
    ChargeStatusFeature& operator=(const ChargeStatusFeature& other) = delete;
    ChargeStatusFeature& operator=(ChargeStatusFeature&& other) = delete;

    /**
     * \brief Handle a signal.
     * \param[in] signal The signal to handle.
     * \return None.
     */
    void HandleSignal(std::shared_ptr<fsm::Signal> signal);

 private:
    /**
     * \brief Creates a transaction that can handle a Battery Charge Status notification/event
     */
    void HandleChargeStatusUpdate(const std::shared_ptr<fsm::Signal>& signal);

    /**
     * \brief Creates a transaction which retrieves all charge statuses on startup
     */
    void CreateGetChargeStatusOnResumeTransaction(const std::shared_ptr<fsm::Signal>& signal);

    /**
     * \brief Handle a Charge Status request signal from the mobile app
     */
    void HandleChargeStatusRequestSignal(const std::shared_ptr<fsm::Signal>& signal);

    void HandleUpdateAllSignal();

    std::shared_ptr<remote_common::IAsyncFunctionExecutor> async_function_executor_;
    std::shared_ptr<IBatteryChargeStatusStorage> battery_charge_status_storage_;
    std::shared_ptr<fas::IFeatureAuthorizationServiceProxy> feature_authorization_service_;
    std::shared_ptr<ChargeStatusUpdateTransaction> charge_status_update_transaction_;
};

}  // namespace vocconv

#endif  // INCLUDE_FEATURES_CHARGE_STATUS_FEATURE_H_

/** \} */  // end of addtogroup
