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

#ifndef INCLUDE_BATTERY_CHARGE_ICHARGE_CONTROLLER_STORAGE_H_
#define INCLUDE_BATTERY_CHARGE_ICHARGE_CONTROLLER_STORAGE_H_

#include "battery_charge/target_soc_types.h"

namespace vocconv {

class IChargeControllerStorage {
 public:
    virtual ~IChargeControllerStorage() = default;

    IChargeControllerStorage(const IChargeControllerStorage&) = delete;
    IChargeControllerStorage(IChargeControllerStorage&&) = delete;
    IChargeControllerStorage& operator=(const IChargeControllerStorage&) = delete;
    IChargeControllerStorage& operator=(IChargeControllerStorage&&) = delete;

    /**
     * \brief Update the charge now setting in persistency
     * \param charge_now The new charge now setting
     */
    virtual void UpdateChargeNow(const bool charge_now) = 0;

    /**
     * \brief Read the charge now setting from persistency
     * \return The new charge now setting in persistency
     */
    virtual bool GetChargeNow() const = 0;

    /**
     * \brief Update the target soc setting in persistency
     * \param target_soc The new target soc setting
     */
    virtual void UpdateTargetSoc(const uint32_t target_soc, const TargetSocSetting setting) = 0;

    /**
     * \brief Read the target soc setting from persistency
     * \return The new target soc setting in persistency
     */
    virtual uint32_t GetTargetSoc() const = 0;
    virtual TargetSocSetting GetTargetSocSetting() const = 0;

    /**
     * \brief Reset the charge controller storage cache
     */
    virtual void ResetCache() = 0;

 protected:
    IChargeControllerStorage() = default;
};

}  // namespace vocconv

#endif  // INCLUDE_BATTERY_CHARGE_ICHARGE_CONTROLLER_STORAGE_H_

/** \} */  // end of addtogroup
