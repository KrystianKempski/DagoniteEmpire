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

#ifndef INCLUDE_BATTERY_CHARGE_BATTERY_CHARGE_STATUS_STORAGE_H_
#define INCLUDE_BATTERY_CHARGE_BATTERY_CHARGE_STATUS_STORAGE_H_

#include <cstdint>
#include <cmath>
#include <memory>
#include <mutex>

#include "local_config_reader/configs.h"
#include "utilities/property.hpp"

#include "battery_charge/ibattery_charge_status_storage.h"
#include "battery_charge/ibattery_charge_status_persistency.h"

namespace vocconv {

/**
 * \class BatteryChargeStatusStorage
 * \brief This class holds Battery Charge Status data and is intended as an abstraction of data storage.
 */
class BatteryChargeStatusStorage : public IBatteryChargeStatusStorage {
 public:
    /**
     * \brief Constructor
     */
    BatteryChargeStatusStorage(
            std::shared_ptr<IBatteryChargeStatusPersistency> persistency,
            const local_config::charge_status_tcam1::Config& config);

    BatteryChargeStatusStorage(const BatteryChargeStatusStorage& other) = delete;
    BatteryChargeStatusStorage(BatteryChargeStatusStorage&& other) = delete;
    BatteryChargeStatusStorage& operator=(const BatteryChargeStatusStorage& other) = delete;
    BatteryChargeStatusStorage& operator=(BatteryChargeStatusStorage&& other) = delete;

    /**
     * \brief Verify that all ChargeStatus and ChargeInfo values are set
     */
    bool AreAllValuesSet() override;
    ChargeStatusUpdated SetBatteryChargeLevelData(const std::shared_ptr<vc::ResGetBatteryChargeLevel>& data) override;
    ChargeStatusUpdated SetBatteryChargeStatusData(const std::shared_ptr<vc::ResGetBatteryChargeStatus>& data) override;
    ChargeStatusUpdated SetChargingPower(const std::shared_ptr<vc::ResGetChargingPowerForHmi>& data) override;
    ChargeStatusUpdated SetChargingCurrent(const std::shared_ptr<vc::ResGetChargingCurrentForHmi>& data) override;
    ChargeStatusUpdated SetChargingVoltage(const std::shared_ptr<vc::ResGetChargingVoltageForHmi>& data) override;

    float GetBatteryChargeLevel() override;
    uint16_t GetDistanceToEmptyKm() override;
    float GetDistanceToEmptyMiles() override;
    float GetAverageEnergyConsumption() override;
    vc::BatteryOnboardCharger GetOnboardChargerHandleStatus() override;
    vc::HmiChargeLockStatus GetChargeConnectorLockStatus() override;
    vc::BatteryChargerState GetChargerState() override;
    vc::BatteryChargingType GetChargingType() override;
    uint16_t GetEstimatedChargingTime() override;
    bool GetCancelHoldChargeRequest() override;
    vc::HldChargingReq GetHoldChargeRequest() override;
    /**
     * \brief Returns the debounced charging power value in watts (accepted after N consecutive samples in the same
     * direction).
     */
    uint32_t GetChargingPower() override;
    /**
     * \brief Returns the most recent charging power sample in watts from CAN, regardless of debounce acceptance.
     */
    uint32_t GetLatestChargingPowerReceived() override;
    int16_t GetChargingCurrent() override;
    uint16_t GetChargingVoltage() override;
    void MarkAverageEnergyConsumptionClean() override;

    status_BatteryChargeStatus CreateChargeStatusData() override;
    std::shared_ptr<remote_common::BatteryChargeStatusUpdate> CreateChargeStatusUpdatePayload() override;
    std::shared_ptr<remote_common::BatteryChargeStatusResponse> CreateChargeStatusResponsePayload() override;

 private:
    enum class SignalDirection { kStable, kIncreasing, kDecreasing };

    bool StoreInPersistentStorage();

    bool IsChargeStatusDefaultValues() const;
    bool IsChargeLevelDefaultValues() const;

    bool charge_status_set_;
    bool charge_level_set_;

    std::mutex battery_charge_data_mutex_;

    std::shared_ptr<IBatteryChargeStatusPersistency> persistency_;

    const float charge_level_delta_;
    const int estimated_charge_time_delta_;

    float battery_charge_level_ = 0.0f;
    uint16_t estimated_distance_to_empty_km_ = 0;
    float estimated_distance_to_empty_miles_ = 0.0f;
    vc::BatteryOnboardCharger onboard_charger_handle_status_ =
            vc::BatteryOnboardCharger::BATTERY_ONBOARDCHARGER_DISCONNECTED;
    vc::HmiChargeLockStatus charge_connector_lock_status_ = vc::HmiChargeLockStatus::HMI_CHARGE_LOCK_STATUS_IDLE;
    vc::BatteryChargerState charger_state_ = vc::BatteryChargerState::BATTERY_CHARGERSTATE_IDLE_MODE;
    vc::BatteryChargingType charging_type_ = vc::BatteryChargingType::BATTERY_CHARGINGTYPE_DISCONNECTED;
    uint16_t estimated_charging_time_ = 0;
    bool cancel_hold_charge_request_ = false;
    vc::HldChargingReq hold_charge_request_ = vc::HldChargingReq::HLD_CHARGING_ALLOW;
    int16_t charging_current_ = 0;
    uint16_t charging_voltage_ = 0;
    remote_common::Property<float> average_energy_consumption_;
    uint32_t charging_power_watts_ = 0;
    uint32_t latest_charging_power_received_watts_ = 0;
    SignalDirection charging_power_direction_{SignalDirection::kStable};
    const uint32_t charging_power_delta_direction_changes_watts_;
    const uint32_t charging_power_delta_same_direction_watts_;
};

}  // namespace vocconv

#endif  // INCLUDE_BATTERY_CHARGE_BATTERY_CHARGE_STATUS_STORAGE_H_

/** \} */  // end of addtogroup
