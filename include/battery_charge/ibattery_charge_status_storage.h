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

#ifndef INCLUDE_BATTERY_CHARGE_IBATTERY_CHARGE_STATUS_STORAGE_H_
#define INCLUDE_BATTERY_CHARGE_IBATTERY_CHARGE_STATUS_STORAGE_H_

#include <cstdint>
#include <memory>

#include "signals/battery_charge_status/battery_charge_status_update.h"
#include "signals/battery_charge_status/battery_charge_status_response.h"
#include "vc_message_payloads.hpp"

namespace vocconv {

enum class ChargeStatusUpdated {
    kUnchanged,
    kStateChanged,
    kValueChanged,
    kNoisyValueChanged,
};

class IBatteryChargeStatusStorage {
 public:
    virtual ~IBatteryChargeStatusStorage() = default;

    IBatteryChargeStatusStorage(const IBatteryChargeStatusStorage&) = delete;
    IBatteryChargeStatusStorage(IBatteryChargeStatusStorage&&) = delete;
    IBatteryChargeStatusStorage& operator=(const IBatteryChargeStatusStorage&) = delete;
    IBatteryChargeStatusStorage& operator=(IBatteryChargeStatusStorage&&) = delete;

    virtual bool AreAllValuesSet() = 0;
    virtual ChargeStatusUpdated SetBatteryChargeLevelData(
            const std::shared_ptr<vc::ResGetBatteryChargeLevel>& data) = 0;
    virtual ChargeStatusUpdated SetBatteryChargeStatusData(
            const std::shared_ptr<vc::ResGetBatteryChargeStatus>& data) = 0;
    virtual ChargeStatusUpdated SetChargingPower(const std::shared_ptr<vc::ResGetChargingPowerForHmi>& data) = 0;
    virtual ChargeStatusUpdated SetChargingCurrent(const std::shared_ptr<vc::ResGetChargingCurrentForHmi>& data) = 0;
    virtual ChargeStatusUpdated SetChargingVoltage(const std::shared_ptr<vc::ResGetChargingVoltageForHmi>& data) = 0;
    virtual float GetBatteryChargeLevel() = 0;
    virtual uint16_t GetDistanceToEmptyKm() = 0;
    virtual float GetDistanceToEmptyMiles() = 0;
    virtual float GetAverageEnergyConsumption() = 0;
    virtual vc::BatteryOnboardCharger GetOnboardChargerHandleStatus() = 0;
    virtual vc::HmiChargeLockStatus GetChargeConnectorLockStatus() = 0;
    virtual vc::BatteryChargerState GetChargerState() = 0;
    virtual vc::BatteryChargingType GetChargingType() = 0;
    virtual uint16_t GetEstimatedChargingTime() = 0;
    virtual bool GetCancelHoldChargeRequest() = 0;
    virtual vc::HldChargingReq GetHoldChargeRequest() = 0;
    virtual uint32_t GetChargingPower() = 0;
    virtual uint32_t GetLatestChargingPowerReceived() = 0;
    virtual int16_t GetChargingCurrent() = 0;
    virtual uint16_t GetChargingVoltage() = 0;
    virtual void MarkAverageEnergyConsumptionClean() = 0;
    virtual status_BatteryChargeStatus CreateChargeStatusData() = 0;
    virtual std::shared_ptr<remote_common::BatteryChargeStatusUpdate> CreateChargeStatusUpdatePayload() = 0;
    virtual std::shared_ptr<remote_common::BatteryChargeStatusResponse> CreateChargeStatusResponsePayload() = 0;

 protected:
    IBatteryChargeStatusStorage() = default;
};

}  // namespace vocconv

#endif  // INCLUDE_BATTERY_CHARGE_IBATTERY_CHARGE_STATUS_STORAGE_H_

/** \} */  // end of addtogroup
