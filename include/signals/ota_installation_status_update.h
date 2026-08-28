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

#ifndef INCLUDE_SIGNALS_OTA_INSTALLATION_STATUS_UPDATE_H_
#define INCLUDE_SIGNALS_OTA_INSTALLATION_STATUS_UPDATE_H_

#include "app_framework/signals/protobuf_signal.h"
#include "signals/protobuf/messages/ota/OTAInstallationStatusUpdate.pb.h"

namespace vocconv {

constexpr auto kOtaInstallationStatusUpdateOid = "1.3.6.1.4.1.37916.3.6.12.2.0.1";
constexpr char kOtaInstallationStatusUpdatename[] = "OtaInstallationStatusUpdate";

class OtaInstallationStatusUpdate : public fsm::ProtobufSignal<remote_control_OTAInstallationStatusUpdate> {
 public :
    OtaInstallationStatusUpdate();
    OtaInstallationStatusUpdate(const OtaInstallationStatusUpdate&) = delete;
    OtaInstallationStatusUpdate(OtaInstallationStatusUpdate&&) = delete;
    OtaInstallationStatusUpdate& operator = (const OtaInstallationStatusUpdate&) = delete;
    OtaInstallationStatusUpdate& operator = (const OtaInstallationStatusUpdate&&) = delete;
    ~OtaInstallationStatusUpdate() = default;

    void SetStatus(status_OTAInstallationStatusValue status);
};

}  // namespace vocconv
#endif  // INCLUDE_SIGNALS_OTA_INSTALLATION_STATUS_UPDATE_H_
/** \} */  // end of addtogroup
