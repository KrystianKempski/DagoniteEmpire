/*
 * Copyright (C) 2026 - Volvo Car Corporation
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

#ifndef INCLUDE_SIGNALS_OPTIMIZED_CHARGING_SCHEDULE_INVALIDATED_UPDATE_H_
#define INCLUDE_SIGNALS_OPTIMIZED_CHARGING_SCHEDULE_INVALIDATED_UPDATE_H_

#include <pb.h>

#include "app_framework/signals/protobuf_signal.h"

#include "battery_charge/charging_location.h"
#include "signals/protobuf/messages/batterycharge/SetOptimizedChargingSchedule.pb.h"
#include "signals/vocconv_signal_types.h"

namespace vocconv {

class OptimizedChargingScheduleInvalidatedUpdate :
        public fsm::ProtobufSignal<remote_control_OptimizedChargingScheduleInvalidatedUpdate> {
 public:
    explicit OptimizedChargingScheduleInvalidatedUpdate(
            const charging::OptimizedChargingScheduleInvalidationReason reason);
    OptimizedChargingScheduleInvalidatedUpdate(const OptimizedChargingScheduleInvalidatedUpdate&) = delete;
    OptimizedChargingScheduleInvalidatedUpdate(OptimizedChargingScheduleInvalidatedUpdate&&) = delete;
    OptimizedChargingScheduleInvalidatedUpdate& operator=(const OptimizedChargingScheduleInvalidatedUpdate&) = delete;
    OptimizedChargingScheduleInvalidatedUpdate& operator=(OptimizedChargingScheduleInvalidatedUpdate&&) = delete;
    ~OptimizedChargingScheduleInvalidatedUpdate() = default;

    static constexpr auto Oid() {
        return "1.3.6.1.4.1.37916.3.6.21.7.4.1";
    }

    static constexpr auto Name() {
        return "OptimizedChargingScheduleInvalidatedUpdate";
    }

    static constexpr uint32_t Type() {
        return static_cast<uint32_t>(SignalTypes::kOptimizedChargingScheduleInvalidatedUpdateSignal);
    }
};
}  // namespace vocconv
#endif  // INCLUDE_SIGNALS_OPTIMIZED_CHARGING_SCHEDULE_INVALIDATED_UPDATE_H_
/** \} */  // end of addtogroup
