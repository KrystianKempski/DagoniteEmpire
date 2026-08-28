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

#ifndef INCLUDE_SIGNALS_ENGINE_REMOTE_START_STATUS_REQUEST_H_
#define INCLUDE_SIGNALS_ENGINE_REMOTE_START_STATUS_REQUEST_H_

#include "app_framework/signals/protobuf_signal.h"
#include "signals/protobuf/messages/engineremotestart/ERSStatusRequest.pb.h"

namespace vocconv {

class EngineRemoteStartStatusRequest :
        public fsm::ProtobufSignal<remote_control_ERSStatusRequest> {
 public:
    EngineRemoteStartStatusRequest();
    EngineRemoteStartStatusRequest(const EngineRemoteStartStatusRequest&) = delete;
    EngineRemoteStartStatusRequest(EngineRemoteStartStatusRequest&&) = delete;
    EngineRemoteStartStatusRequest& operator=(const EngineRemoteStartStatusRequest&) = delete;
    EngineRemoteStartStatusRequest& operator=(EngineRemoteStartStatusRequest&&) = delete;
    ~EngineRemoteStartStatusRequest() = default;

    static constexpr const char* kEngineRemoteStartStatusRequestOid = "1.3.6.1.4.1.37916.3.6.20.0.1.2";
};

}  // namespace vocconv
#endif  // INCLUDE_SIGNALS_ENGINE_REMOTE_START_STATUS_REQUEST_H_
/** \} */  // end of addtogroup
