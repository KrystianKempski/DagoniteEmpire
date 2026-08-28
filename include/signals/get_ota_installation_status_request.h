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

#ifndef INCLUDE_SIGNALS_GET_OTA_INSTALLATION_STATUS_REQUEST_H_
#define INCLUDE_SIGNALS_GET_OTA_INSTALLATION_STATUS_REQUEST_H_

#include "app_framework/signals/protobuf_signal.h"

#include "signals/protobuf/messages/ota/GetOTAInstallationStatusRequest.pb.h"

namespace vocconv {

class GetOtaInstallationStatusRequest :
        public fsm::ProtobufSignal<remote_control_GetOTAInstallationStatusRequest> {
 public:
    GetOtaInstallationStatusRequest();
    GetOtaInstallationStatusRequest(const GetOtaInstallationStatusRequest&) = delete;
    GetOtaInstallationStatusRequest(GetOtaInstallationStatusRequest&&) = delete;
    GetOtaInstallationStatusRequest& operator=(const GetOtaInstallationStatusRequest&) = delete;
    GetOtaInstallationStatusRequest& operator=(GetOtaInstallationStatusRequest&&) = delete;
    ~GetOtaInstallationStatusRequest() = default;

    static constexpr const char* kGetOtaInstallationStatusRequestOid = "1.3.6.1.4.1.37916.3.6.12.2.1.2";
};

}  // namespace vocconv
#endif  // INCLUDE_SIGNALS_GET_OTA_INSTALLATION_STATUS_REQUEST_H_
/** \} */  // end of addtogroup
