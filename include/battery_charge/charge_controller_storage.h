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

#ifndef INCLUDE_BATTERY_CHARGE_CHARGE_CONTROLLER_STORAGE_H_
#define INCLUDE_BATTERY_CHARGE_CHARGE_CONTROLLER_STORAGE_H_

#include "battery_charge/charging_location_constants.h"
#include "battery_charge/icharge_controller_storage.h"

#include "text_handler.hpp"
#include "json/value.h"

namespace vocconv {

class ChargeControllerStorage : public IChargeControllerStorage {
 public:
    ChargeControllerStorage();

    ChargeControllerStorage(const ChargeControllerStorage&) = delete;
    ChargeControllerStorage& operator=(const ChargeControllerStorage&) = delete;
    ChargeControllerStorage(ChargeControllerStorage&&) = delete;
    ChargeControllerStorage& operator=(ChargeControllerStorage&&) = delete;

    ~ChargeControllerStorage() = default;

    void UpdateChargeNow(const bool charge_now) override;
    bool GetChargeNow() const override;
    void UpdateTargetSoc(const uint32_t target_soc, const TargetSocSetting setting) override;
    uint32_t GetTargetSoc() const override;
    TargetSocSetting GetTargetSocSetting() const override;
    void ResetCache() final;

 private:
    bool FromJsonV1(const Json::Value& json);
    bool FromJsonV2(const Json::Value& json);
    bool ReadFromPersistency();
    void SaveToPersistency();

    persistency::TextHandler text_handler_{};
    bool charge_now_{false};
    uint32_t target_soc_{charging::kDefaultTargetSoC};
    TargetSocSetting target_soc_setting_{charging::kDefaultTargetSocSetting};
};

}  // namespace vocconv

#endif  // INCLUDE_BATTERY_CHARGE_CHARGE_CONTROLLER_STORAGE_H_

/** \} */  // end of addtogroup
