/*
 * Copyright (C) 2023 - Volvo Car Corporation
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

#ifndef INCLUDE_BATTERY_CHARGE_CHARGE_CONTROL_CACHE_H_
#define INCLUDE_BATTERY_CHARGE_CHARGE_CONTROL_CACHE_H_

#include <mutex>
#include <vector>

#include "battery_charge/battery_charge_timer.h"
#include "battery_charge/icharge_control_cache.h"
#include "app_framework/signals/vsomeip_signal.h"

namespace vocconv {

class ChargeControlCache : public IChargeControlCache {
 public:
    ChargeControlCache();

    ChargeControlCache(const ChargeControlCache& other) = delete;
    ChargeControlCache(ChargeControlCache&& other) = delete;
    ChargeControlCache& operator=(const ChargeControlCache& other) = delete;
    ChargeControlCache& operator=(ChargeControlCache&& other) = delete;

    /**
     * \brief Stores the latest received value of Charge Timer
     * \returns true if the stored value changed, otherwise false.
     */
    bool UpdateChargeTimer(const std::vector<vsomeip::byte_t>& location_item) override;

    /**
     * \brief Stores the latest received value of Amperage Limit
     * \returns true if the stored value changed, otherwise false.
     */
    bool UpdateAmperageLimit(const std::vector<vsomeip::byte_t>& location_item) override;

    /**
     * \brief Gets the latest stored value of Charge Timer
     * \returns The latest stored value of Charge Timer.
     */
    remote_common::BatteryChargeTimer GetChargeTimer() override;

    /**
     * \brief Gets the latest stored value of Amperage Limit
     * \returns The latest stored value of Amperage Limit.
     */
    uint32_t GetAmperageLimit() override;

 private:
    std::mutex access_mutex_;
    remote_common::BatteryChargeTimer battery_charge_timer_{};
    uint32_t battery_charge_amperage_limit_{0};
};

}  // namespace vocconv
#endif  // INCLUDE_BATTERY_CHARGE_CHARGE_CONTROL_CACHE_H_
/** \} */  // end of addtogroup
