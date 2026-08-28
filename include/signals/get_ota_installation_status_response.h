/*
 * Copyright (C) 2022 - Volvo Car Corporation
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

#ifndef INCLUDE_SIGNALS_GET_OTA_INSTALLATION_STATUS_RESPONSE_H_
#define INCLUDE_SIGNALS_GET_OTA_INSTALLATION_STATUS_RESPONSE_H_

#include <cstdint>

#include "app_framework/signals/protobuf_signal.h"
#include "signals/protobuf/messages/ota/GetOTAInstallationStatusResponse.pb.h"

namespace vocconv {

class GetOtaInstallationStatusResponse :
        public fsm::ProtobufSignal<remote_control_GetOTAInstallationStatusResponse> {
 public:
    GetOtaInstallationStatusResponse();
    GetOtaInstallationStatusResponse(const GetOtaInstallationStatusResponse&) = delete;
    GetOtaInstallationStatusResponse(GetOtaInstallationStatusResponse&&) = delete;
    GetOtaInstallationStatusResponse& operator=(const GetOtaInstallationStatusResponse&) = delete;
    GetOtaInstallationStatusResponse& operator=(GetOtaInstallationStatusResponse&&) = delete;
    ~GetOtaInstallationStatusResponse() = default;

    static constexpr const char* kGetOtaInstallationStatusResponseOid = "1.3.6.1.4.1.37916.3.6.12.2.1.3";

    /**
     * \brief Set the OTA installation status
     * \param[in] status status_OTAInstallationStatusValue
     */
    void SetStatus(status_OTAInstallationStatusValue status);

    /**
     * \brief Set the Response Status of the payload.
     * \param success    Boolean defining if the payload should have response code
     *                   SUCCESS or ERROR.
     * \param error_code int32_t (optional) defining the error code. Will only be used if success equals false.
     */
    void SetResponseStatus(bool success, int32_t error_code = 0);
};

}  // namespace vocconv
#endif  // INCLUDE_SIGNALS_GET_OTA_INSTALLATION_STATUS_RESPONSE_H_
/** \} */  // end of addtogroup
