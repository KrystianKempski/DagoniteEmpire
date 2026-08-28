/*
 * Copyright (C) 2019 - Volvo Car Corporation
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

#ifndef INCLUDE_OTA_INSTALLATION_IOTA_INSTALLATION_STATE_H_
#define INCLUDE_OTA_INSTALLATION_IOTA_INSTALLATION_STATE_H_

#include <cstdint>

namespace vocconv {

enum class OtaStatus : uint8_t {
    kNoNotify = 0,
    kDownloadCompleted = 1,
    kInstallationScheduled = 2,
    kInstallationInitiated = 3,
    kInstallationStarted = 4,
    kInstallationCompleted = 5,
    kReserved1 = 6,
    kReserved2 = 7
};

struct SchedulingReminderState {
    uint32_t request_count;
    int64_t epoch;
};

class IOtaInstallationState {
 public:
    virtual ~IOtaInstallationState() {}

    IOtaInstallationState(const IOtaInstallationState& other) = delete;
    IOtaInstallationState(IOtaInstallationState&& other) = delete;
    IOtaInstallationState& operator=(const IOtaInstallationState& other) = delete;
    IOtaInstallationState& operator=(IOtaInstallationState&& other) = delete;

    virtual void set_status(OtaStatus) = 0;
    virtual OtaStatus status() const = 0;
    virtual SchedulingReminderState scheduling_reminder_state() const = 0;
    virtual void IncrementSchedulingReminderState() = 0;
 protected:
    IOtaInstallationState() = default;
};

}  // namespace vocconv
#endif  // INCLUDE_OTA_INSTALLATION_IOTA_INSTALLATION_STATE_H_
/** \} */  // end of addtogroup
