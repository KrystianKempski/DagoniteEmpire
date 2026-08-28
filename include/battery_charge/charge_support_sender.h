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

#ifndef INCLUDE_BATTERY_CHARGE_CHARGE_SUPPORT_SENDER_H_
#define INCLUDE_BATTERY_CHARGE_CHARGE_SUPPORT_SENDER_H_

#include <memory>
#include <mutex>

#include "feature_authorization_service_proxy/ifeature_authorization_service_proxy.h"
#include "utilities/iasync_function_executor.h"

#include "battery_charge/icharge_support_sender.h"
#include "utilities/imqtt_wrapper.h"

namespace vocconv {

class ChargeSupportSender : public IChargeSupportSender {
 public:
    ChargeSupportSender(
            std::shared_ptr<remote_common::IAsyncFunctionExecutor> async_function_executor,
            std::shared_ptr<fas::IFeatureAuthorizationServiceProxy> fas_proxy,
            std::shared_ptr<remote_common::IMqttWrapper> mqtt_wrapper);
    ~ChargeSupportSender() override = default;

    ChargeSupportSender(const ChargeSupportSender&) = delete;
    ChargeSupportSender& operator=(const ChargeSupportSender&) = delete;
    ChargeSupportSender(ChargeSupportSender&&) = delete;
    ChargeSupportSender& operator=(ChargeSupportSender&&) = delete;

    void SendChargeControlStatusUpdate(const ChargeControllerStatusUpdate& status) override;
    void SendChargeControlStatusUpdateToCloud(const ChargeControllerStatusUpdate& status) override;

    /**
     * @brief Send a charging location update to the cloud and IHU
     * @param location The charging location data to send
     * @param deleted True if the location was deleted, false if it was updated. Used to construct a differnt update
     * message for the IHU.
     */
    void SendChargingLocationUpdate(const charging::ChargingLocation& location, bool deleted) override;

    /**
     * @brief Send a charging location actual or default update to the cloud and IHU
     *
     * This is intended to send the Actual location update. If the current location is deleted, ActualUpdate passes
     * the default settings.
     *
     * @param location The charging location data to send
     * @param valid True if the location exists. False if not on a tracked location.
     */
    void SendChargingLocationActualUpdate(const charging::ChargingLocation& location, bool valid) override;

    /**
     * @brief Send the target SOC update to cloud and IHU
     *
     * @param target_soc The target SOC to send
     * @param setting The setting type of the target SOC to send
     */
    void SendTargetSocUpdate(const uint8_t target_soc, const TargetSocSetting setting) override;

    /**
     * @brief Send the target SOC update to cloud
     *
     * @param target_soc The target SOC to send
     * @param setting The setting type of the target SOC to send
     */
    void SendTargetSocUpdateToCloud(const uint8_t target_soc, const TargetSocSetting setting) override;

    /**
     * @brief Send the optimized charging schedule invalidated update to cloud and IHU
     *
     * @param reason The reason for the invalidation
     */
    void SendOptimizedScheduleInvalidatedUpdate(
            const charging::OptimizedChargingScheduleInvalidationReason reason) override;

#ifndef UNIT_TESTS

 private:
#endif
    void SendChargingLocationActualUpdateToCloud(const charging::ChargingLocation& location, bool valid);
    void SendChargingLocationActualUpdateToIhu(const charging::ChargingLocation& location) const;

    void SendTargetSocUpdateToIhu(const uint8_t target_soc, const TargetSocSetting setting) const;

    std::mutex mutex_;
    charging::ChargingLocation last_actual_location_sent_ccm_{};
    bool actual_location_sent_ccm_{false};
    ChargeControllerStatusUpdate last_sent_charge_controller_status_{};
    std::shared_ptr<remote_common::IAsyncFunctionExecutor> async_function_executor_;
    std::shared_ptr<fas::IFeatureAuthorizationServiceProxy> fas_proxy_;
    std::shared_ptr<remote_common::IMqttWrapper> mqtt_wrapper_;
};

}  // namespace vocconv
#endif     // INCLUDE_BATTERY_CHARGE_CHARGE_SUPPORT_SENDER_H_
/** \} */  // end of addtogroup
