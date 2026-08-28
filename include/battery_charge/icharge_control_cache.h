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

#ifndef INCLUDE_BATTERY_CHARGE_ICHARGE_CONTROL_CACHE_H_
#define INCLUDE_BATTERY_CHARGE_ICHARGE_CONTROL_CACHE_H_

#include <vector>

#include "app_framework/signals/vsomeip_signal.h"

#include "battery_charge/battery_charge_timer.h"

namespace vocconv {

class IChargeControlCache {
 public:
    virtual ~IChargeControlCache() = default;

    IChargeControlCache(const IChargeControlCache&) = delete;
    IChargeControlCache(IChargeControlCache&&) = delete;
    IChargeControlCache& operator=(const IChargeControlCache&) = delete;
    IChargeControlCache& operator=(IChargeControlCache&&) = delete;

    virtual bool UpdateChargeTimer(const std::vector<vsomeip::byte_t>& location_item) = 0;
    virtual bool UpdateAmperageLimit(const std::vector<vsomeip::byte_t>& location_item) = 0;
    virtual remote_common::BatteryChargeTimer GetChargeTimer() = 0;
    virtual uint32_t GetAmperageLimit() = 0;

 protected:
    IChargeControlCache() = default;
};

}  // namespace vocconv
#endif  // INCLUDE_BATTERY_CHARGE_ICHARGE_CONTROL_CACHE_H_
/** \} */  // end of addtogroup
