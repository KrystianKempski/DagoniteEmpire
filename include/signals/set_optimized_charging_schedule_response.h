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

#ifndef INCLUDE_SIGNALS_SET_OPTIMIZED_CHARGING_SCHEDULE_RESPONSE_H_
#define INCLUDE_SIGNALS_SET_OPTIMIZED_CHARGING_SCHEDULE_RESPONSE_H_

#include <pb.h>

#include "app_framework/signals/protobuf_signal.h"

#include "signals/protobuf/messages/batterycharge/SetOptimizedChargingSchedule.pb.h"
#include "signals/vocconv_signal_types.h"

namespace vocconv {

class SetOptimizedChargingScheduleResponse :
        public fsm::ProtobufSignal<remote_control_SetOptimizedChargingScheduleResponse> {
 public:
    explicit SetOptimizedChargingScheduleResponse(common_ResponseStatus_Status response_status,
                                                 int32_t response_code);
    SetOptimizedChargingScheduleResponse();
    SetOptimizedChargingScheduleResponse(const SetOptimizedChargingScheduleResponse&) = delete;
    SetOptimizedChargingScheduleResponse(SetOptimizedChargingScheduleResponse&&) = delete;
    SetOptimizedChargingScheduleResponse& operator=(const SetOptimizedChargingScheduleResponse&) = delete;
    SetOptimizedChargingScheduleResponse& operator=(SetOptimizedChargingScheduleResponse&&) = delete;
    ~SetOptimizedChargingScheduleResponse() = default;

    static constexpr auto Oid() {
        return "1.3.6.1.4.1.37916.3.6.21.7.2.3";
    }

    static constexpr auto Name() {
        return "SetOptimizedChargingScheduleResponse";
    }

    static constexpr uint32_t Type() {
        return static_cast<uint32_t>(SignalTypes::kSetOptimizedChargingScheduleResponseSignal);
    }
};

}  // namespace vocconv
#endif  // INCLUDE_SIGNALS_SET_OPTIMIZED_CHARGING_SCHEDULE_RESPONSE_H_
/** \} */  // end of addtogroup
