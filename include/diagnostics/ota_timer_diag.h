/*
 * Copyright (C) 2023 - Volvo Car Corporation
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

#ifndef INCLUDE_DIAGNOSTICS_OTA_TIMER_DIAG_H_
#define INCLUDE_DIAGNOSTICS_OTA_TIMER_DIAG_H_

#include <vector>

#include "diagnostics/ota_timer_diag_data.h"
#include "diagnostics/ota_timer_exit_causes.h"
#include "common/log.h"

namespace vocconv {
/**
 * \brief Class for managing ota timer diagnostics.
 *
 * Provides an interface through which clients can register diagnostic
 * events related to ota timer.
 *
 * \tparam STORAGE class that implements a storage policy
 */
template <typename STORAGE>
class OTATimerDiag {
 public:
    OTATimerDiag() = default;
    ~OTATimerDiag() = default;

    OTATimerDiag(const OTATimerDiag& other) = delete;
    OTATimerDiag(OTATimerDiag&& other) = delete;
    OTATimerDiag& operator=(const OTATimerDiag& other) = delete;
    OTATimerDiag& operator=(OTATimerDiag&& other) = delete;

    void SetOTATimerSchedule(OTATimerDetailDiagData detail) {
        VLOG_ENTER(OTATIMER_LOG_CTX, DLT_LOG_DEBUG);
        STORAGE storage;
        OTATimerDiagData data;
        if (storage.Read(data)) {
            if ((detail.state == OTATimerExitCause::kScheduled) || (detail.state == OTATimerExitCause::kCancelled)) {
                data.SetOTAScheduleSuccess(detail);
            } else {
                data.SetOTAScheduleFailed(detail);
            }

            storage.Write(data);
        }
    }

    void SetOTAInitiateInstallation(OTATimerDetailDiagData detail) {
        VLOG_ENTER(OTATIMER_LOG_CTX, DLT_LOG_DEBUG);
        STORAGE storage;
        OTATimerDiagData data;
        if (storage.Read(data)) {
            data.SetOTATimerExpired();
            if (detail.state == OTATimerExitCause::kInitiated) {
                data.SetOTAInitiateInstallationSuccess(detail);
            } else {
                data.SetOTAInitiateInstallationFailed(detail);
            }

            storage.Write(data);
        }
    }

    void GetSerializedDiagData(std::vector<uint8_t>& outData) {  // NOLINT
        VLOG_ENTER(OTATIMER_LOG_CTX, DLT_LOG_DEBUG);
        STORAGE storage;
        OTATimerDiagData data;
        if (storage.Read(data)) {
            ToByteVector(data, outData);
        }
    }

    bool ClearStoredDiagnosticData() {
        VLOG_ENTER(OTATIMER_LOG_CTX, DLT_LOG_DEBUG);
        STORAGE storage;
        OTATimerDiagData data;
        data.Clear();
        return storage.Write(data);
    }
};

}  // namespace vocconv

#endif  // INCLUDE_DIAGNOSTICS_OTA_TIMER_DIAG_H_
/** \} */  // end of addtogroup
