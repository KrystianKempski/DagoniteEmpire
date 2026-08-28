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

#ifndef INCLUDE_SIGNALS_ENGINE_REMOTE_START_STATUS_RESPONSE_H_
#define INCLUDE_SIGNALS_ENGINE_REMOTE_START_STATUS_RESPONSE_H_

#include <cstdint>

#include "app_framework/signals/protobuf_signal.h"
#include "signals/protobuf/messages/engineremotestart/ERSStatusResponse.pb.h"

namespace vocconv {

class EngineRemoteStartStatusResponse : public fsm::ProtobufSignal<remote_control_ERSStatusResponse> {
 public:
    EngineRemoteStartStatusResponse();
    EngineRemoteStartStatusResponse(const EngineRemoteStartStatusResponse&) = delete;
    EngineRemoteStartStatusResponse(EngineRemoteStartStatusResponse&&) = delete;
    EngineRemoteStartStatusResponse& operator=(const EngineRemoteStartStatusResponse&) = delete;
    EngineRemoteStartStatusResponse& operator=(EngineRemoteStartStatusResponse&&) = delete;
    ~EngineRemoteStartStatusResponse() = default;

    static constexpr const char* kEngineRemoteStartStatusResponseOid = "1.3.6.1.4.1.37916.3.6.20.0.1.3";

    /**
     * \brief Set the engine remote start status
     * \param[in] status_type status_ErsStatusType,
     * result_type status_ErsStartResultType,
     */
    void SetStatus(status_ErsStatusType status_type,
                   status_ErsStartResultType result_type);

    /**
     * \brief Set the engine remote start status
     * \param[in] status status_ErsStatus
     */
    void SetStatus(const status_ErsStatus& status);

    /**
     * \brief Set the Response Status of the payload.
     * \param success    Boolean defining if the payload should have response code
     *                   SUCCESS or ERROR.
     * \param error_code int32_t (optional) defining the error code. Will only be used if success equals false.
     */
    void SetResponseStatus(bool success, int32_t error_code = 0);
};

}  // namespace vocconv
#endif  // INCLUDE_SIGNALS_ENGINE_REMOTE_START_STATUS_RESPONSE_H_
/** \} */  // end of addtogroup
