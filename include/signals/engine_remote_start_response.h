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

#ifndef INCLUDE_SIGNALS_ENGINE_REMOTE_START_RESPONSE_H_
#define INCLUDE_SIGNALS_ENGINE_REMOTE_START_RESPONSE_H_

#include <cstdint>

#include "app_framework/signals/protobuf_signal.h"
#include "signals/protobuf/messages/engineremotestart/ERSResponse.pb.h"

namespace vocconv {

class EngineRemoteStartResponse :
        public fsm::ProtobufSignal<remote_control_ERSResponse> {
 public:
    EngineRemoteStartResponse();
    EngineRemoteStartResponse(const EngineRemoteStartResponse&) = delete;
    EngineRemoteStartResponse(EngineRemoteStartResponse&&) = delete;
    EngineRemoteStartResponse& operator=(const EngineRemoteStartResponse&) = delete;
    EngineRemoteStartResponse& operator=(EngineRemoteStartResponse&&) = delete;
    ~EngineRemoteStartResponse() = default;

    static constexpr const char* kEngineRemoteStartResponseOid = "1.3.6.1.4.1.37916.3.6.20.0.0.3";

    void SetResponseStatus(bool status);

    void SetResponseCode(int32_t code);
};

}  // namespace vocconv
#endif  // INCLUDE_SIGNALS_ENGINE_REMOTE_START_RESPONSE_H_
/** \} */  // end of addtogroup
