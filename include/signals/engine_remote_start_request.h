/*
 * Copyright (C) 2021 - Volvo Car Corporation
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

#ifndef INCLUDE_SIGNALS_ENGINE_REMOTE_START_REQUEST_H_
#define INCLUDE_SIGNALS_ENGINE_REMOTE_START_REQUEST_H_

#include "app_framework/signals/protobuf_signal.h"
#include "signals/protobuf/messages/engineremotestart/ERSRequest.pb.h"

namespace vocconv {

class EngineRemoteStartRequest :
        public fsm::ProtobufSignal<remote_control_ERSRequest> {
 public:
    EngineRemoteStartRequest();
    EngineRemoteStartRequest(const EngineRemoteStartRequest&) = delete;
    EngineRemoteStartRequest(EngineRemoteStartRequest&&) = delete;
    EngineRemoteStartRequest& operator=(const EngineRemoteStartRequest&) = delete;
    EngineRemoteStartRequest& operator=(EngineRemoteStartRequest&&) = delete;
    ~EngineRemoteStartRequest() = default;

    static constexpr const char* kEngineRemoteStartRequestOid = "1.3.6.1.4.1.37916.3.6.20.0.0.2";

    enum class SeatWheelHeatType {
        kOff = remote_control_ErsSeatWheelHeatType::remote_control_ErsSeatWheelHeatType_ERS_STATUS_OFF,
        kAuto = remote_control_ErsSeatWheelHeatType::remote_control_ErsSeatWheelHeatType_ERS_STATUS_AUTO
    };

    bool IsStartRequested();
    bool IsStopRequested();
    int32_t GetRunTime();
    SeatWheelHeatType GetLeftFrontSeatHeatSetting();
    SeatWheelHeatType GetRightFrontSeatHeatSetting();
    SeatWheelHeatType GetSteeringWheelHeatSetting();
};

}  // namespace vocconv
#endif  // INCLUDE_SIGNALS_ENGINE_REMOTE_START_REQUEST_H_
/** \} */  // end of addtogroup
