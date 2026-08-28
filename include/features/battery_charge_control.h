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

#ifndef INCLUDE_FEATURES_BATTERY_CHARGE_CONTROL_H_
#define INCLUDE_FEATURES_BATTERY_CHARGE_CONTROL_H_

#include <boost/chrono/chrono.hpp>

#include <memory>
#include <mutex>

#include "feature_authorization_service_proxy/ifeature_authorization_service_proxy.h"
#include "app_framework/features/feature.h"
#include "utilities/itime_provider.h"

#include "battery_charge/charge_control_cache.h"
#include "battery_charge/charging_target_level_cache.h"
#include "common/car_mode_storage.h"
#include "timers/itimer_manager.h"

namespace vocconv {

constexpr char const* kBatteryChargeControlFeatureName = "BatteryChargeControl";

#ifdef UNIT_TESTS
enum class HandleBccSignalResult : uint8_t {
    InitialValue,
    TransactionForCancelChargingTimerCreated,
    TransactionForGetChargingLocationCreated,
    TransactionForGetChargingTargetLevelCreated,
    TransactionForGetChargingTimerCreated,
    TransactionForGetChargingAmperageLimitCreated,
    TransactionForSetChargingLocationCreated,
    TransactionForSetChargingTargetLevelCreated,
    TransactionForSetChargingTimerCreated,
    TransactionForSetChargingAmperageLimitCreated,
    TransactionForRescheduleChargingTimerCreated,
    TransactionForSetChargingTargetLevelUpdateCreated,
    TransactionForUpdateChargingLocationCreated,
    TransactionForPositionUpdateCreated,
    AckReceived,
    NoTransactionCreated
};
#endif

class BatteryChargeControl : public fsm::Feature {
 public:
    BatteryChargeControl(
            std::shared_ptr<ITimerManager> timer_manager,
            std::shared_ptr<fas::IFeatureAuthorizationServiceProxy> feature_authorization_service,
            std::shared_ptr<CarModeStorage> car_mode_storage,
            std::shared_ptr<remote_common::ITimeProvider> time_provider);
    ~BatteryChargeControl();

    BatteryChargeControl(const BatteryChargeControl& other) = delete;
    BatteryChargeControl(BatteryChargeControl&& other) = delete;
    BatteryChargeControl& operator=(const BatteryChargeControl& other) = delete;
    BatteryChargeControl& operator=(BatteryChargeControl&& other) = delete;

    /*
     * \brief Handle a signal.
     * \param[in] signal The signal to handle.
     * \return None.
     */
    void HandleSignal(std::shared_ptr<fsm::Signal> signal) override;

#ifdef ENABLE_SIGNAL_INJECTION
    void ResetBatteryChargeLocationItemPersistency();

    /**
     * \brief Setup UpdateChargingLocationTransaction timeout.
     * \param[in] transaction_timeout timeout for transaction in seconds.
     * \return None.
     **/
    void SetUpdateChargingLocationTransactionTimeout(const boost::chrono::seconds& transaction_timeout);

    template <typename T>
    bool ActiveTransactionExists() const;

    bool ActiveUpdateChargingLocationTransactionExists() const;
#endif
#ifdef UNIT_TESTS
    mutable HandleBccSignalResult signal_recv_result_;
#endif

 private:
    void CreateCancelChargingTimerTransaction(const std::shared_ptr<fsm::Signal>& signal);
    void CreateGetChargingLocationTransaction(const std::shared_ptr<fsm::Signal>& signal);
    void CreateGetChargingTargetLevelTransaction(const std::shared_ptr<fsm::Signal>& signal);
    void CreateGetChargingTimerTransaction(const std::shared_ptr<fsm::Signal>& signal);
    void CreateGetChargingAmperageLimitTransaction(const std::shared_ptr<fsm::Signal>& signal);
    void CreateSetChargingLocationTransaction(const std::shared_ptr<fsm::Signal>& signal);
    void CreateSetChargingTargetLevelTransaction(const std::shared_ptr<fsm::Signal>& signal);
    void CreateRescheduleChargingTimerTransaction(const std::shared_ptr<fsm::Signal>& signal);
    void CreateUpdateChargingTargetLevelInObcTransaction(const std::shared_ptr<fsm::Signal>& signal);
    void CreateSetChargingTimerTransaction(const std::shared_ptr<fsm::Signal>& signal);
    void CreateSetChargingAmperageLimitTransaction(const std::shared_ptr<fsm::Signal>& signal);
    void CreateUpdateChargingLocationTransaction(const std::shared_ptr<fsm::Signal>& signal);
    void HandleVehicleModeNotification(const std::shared_ptr<fsm::Signal>& signal);
    void HandleSomeIpServiceAvailabilitySignal(const std::shared_ptr<fsm::Signal>& signal);
    void HandleUpdateAll();

    /**
     * \brief Parses CarMode from signal's payload and stores it in storage.
     * \return True if the value was updated in storage. False if the value is unchanged, and on error.
     */
    bool UpdateCarModeStorage(const std::shared_ptr<fsm::Signal>& signal);
    void ReceivedVsomeipAck(const std::shared_ptr<fsm::Signal>& signal) const;

    void SendTargetSocToCloud();
    void SendAmperageLimitToCloud();
    void SendChargeTimerStatusToCloud();

    static uint8_t update_charging_location_iplm_id_;

    std::shared_ptr<ITimerManager> timer_manager_;
    std::shared_ptr<fas::IFeatureAuthorizationServiceProxy> feature_authorization_service_;
    std::mutex update_charging_location_mutex_;
    std::shared_ptr<CarModeStorage> car_mode_storage_;
    boost::chrono::seconds update_charging_location_transaction_timeout_{boost::chrono::seconds{15}};
    std::shared_ptr<ChargeControlCache> charge_control_cache_;
    std::shared_ptr<ChargingTargetLevelCache> charging_target_cache_;
    std::shared_ptr<remote_common::ITimeProvider> time_provider_;
};

}  // namespace vocconv
#endif     // INCLUDE_FEATURES_BATTERY_CHARGE_CONTROL_H_
/** \} */  // end of addtogroup
