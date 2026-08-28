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

#ifndef INCLUDE_BATTERY_CHARGE_BATTERY_CHARGE_STATUS_PERSISTENCY_H_
#define INCLUDE_BATTERY_CHARGE_BATTERY_CHARGE_STATUS_PERSISTENCY_H_

#include "json_handler.hpp"

#include "battery_charge/ibattery_charge_status_persistency.h"

namespace vocconv {

/**
 * \class BatteryChargeStatusPersistency
 */
class BatteryChargeStatusPersistency : public IBatteryChargeStatusPersistency {
 public:
    BatteryChargeStatusPersistency() = default;

    BatteryChargeStatusPersistency(const BatteryChargeStatusPersistency& other) = delete;
    BatteryChargeStatusPersistency(BatteryChargeStatusPersistency&& other) = delete;
    BatteryChargeStatusPersistency& operator=(const BatteryChargeStatusPersistency& other) = delete;
    BatteryChargeStatusPersistency& operator=(BatteryChargeStatusPersistency&& other) = delete;

    /**
     * \brief Store a ChargeStatusData struct persistently
     * \return If the action was successfull
     */
    bool Store(const ChargeStatusData& status_data) override;

    /**
     * \brief Read ChargeStatusData from persistency
     * If the new persistency file managed by JsonHandler does not exist then latest status is loaded using PCL.
     * \return If the action was successfull then the latest ChargeStatusData will be loaded, if something fails then
     * default values for ChargeStatusData will be returned.
     */
    ChargeStatusData Read() const override;

#ifdef UNIT_TESTS
    void SetNewPersistencyExists(bool exists);
#endif

 private:
    bool NewPersistencyExists() const;
    ChargeStatusData ReadNew() const;
    ChargeStatusData ReadV1(persistency::JsonHandler& json_handler) const;
    ChargeStatusData ReadV2(persistency::JsonHandler& json_handler) const;
    ChargeStatusData ReadLegacy() const;
};

}  // namespace vocconv

#endif  // INCLUDE_BATTERY_CHARGE_BATTERY_CHARGE_STATUS_PERSISTENCY_H_
/** \} */  // end of addtogroup
