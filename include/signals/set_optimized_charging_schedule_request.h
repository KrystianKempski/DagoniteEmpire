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

#ifndef INCLUDE_SIGNALS_SET_OPTIMIZED_CHARGING_SCHEDULE_REQUEST_H_
#define INCLUDE_SIGNALS_SET_OPTIMIZED_CHARGING_SCHEDULE_REQUEST_H_

#include <pb.h>

#include <string>
#include <vector>

#include "app_framework/signals/protobuf_signal.h"

#include "battery_charge/charging_location.h"
#include "signals/protobuf/messages/batterycharge/SetOptimizedChargingSchedule.pb.h"
#include "signals/vocconv_signal_types.h"

namespace vocconv {

class SetOptimizedChargingScheduleRequest :
        public fsm::ProtobufSignal<remote_control_SetOptimizedChargingScheduleRequest> {
 public:
    explicit SetOptimizedChargingScheduleRequest(const std::string& uuid,
                                                const std::vector<charging::OptimizedChargingTimer>& timers);
    SetOptimizedChargingScheduleRequest();
    SetOptimizedChargingScheduleRequest(const SetOptimizedChargingScheduleRequest&) = delete;
    SetOptimizedChargingScheduleRequest(SetOptimizedChargingScheduleRequest&&) = delete;
    SetOptimizedChargingScheduleRequest& operator=(const SetOptimizedChargingScheduleRequest&) = delete;
    SetOptimizedChargingScheduleRequest& operator=(SetOptimizedChargingScheduleRequest&&) = delete;
    ~SetOptimizedChargingScheduleRequest() = default;

    static constexpr auto Oid() {
        return "1.3.6.1.4.1.37916.3.6.21.7.2.2";
    }

    static constexpr auto Name() {
        return "SetOptimizedChargingScheduleRequest";
    }

    static constexpr uint32_t Type() {
        return static_cast<uint32_t>(SignalTypes::kSetOptimizedChargingScheduleRequestSignal);
    }

    void SetDecodeCallbacks() override;

    std::string GetUuid() const {
        return uuid_;
    }

    std::vector<charging::OptimizedChargingTimer> GetTimers() const {
        return timers_;
    }

 private:
    std::string uuid_;
    std::vector<charging::OptimizedChargingTimer> timers_;
};

}  // namespace vocconv
#endif  // INCLUDE_SIGNALS_SET_OPTIMIZED_CHARGING_SCHEDULE_REQUEST_H_
/** \} */  // end of addtogroup
