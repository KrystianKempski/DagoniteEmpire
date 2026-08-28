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

#ifndef INCLUDE_DIAGNOSTICS_OTA_TIMER_DIAG_DATA_H_
#define INCLUDE_DIAGNOSTICS_OTA_TIMER_DIAG_DATA_H_

#include <uuid/uuid.h>
#include <algorithm>
#include <cstring>
#include <vector>

#include "diagnostics/ota_timer_exit_causes.h"
#include "signals/car_signals.h"

namespace vocconv {

constexpr char kDefaultOTAId[] = "00000000-0000-0000-0000-000000000000";

struct OTATimerDetailDiagData {
    char ota_id_str[kOtaIdSize + 1];
    uint8_t set_by;
    uint32_t ota_timer_timestamp;
    uint32_t ota_timer_expired_timestamp;
    uint32_t ota_state_timestamp;
    OTATimerExitCause state;

    void SetFromOTATimerByteVector(std::vector<uint8_t> data) {
        if (data.size() != 0) {
            // The first 16 bytes represents the ota_id (uuid).
            std::vector<uint8_t> uuid_bytes;
            std::copy_n(data.begin(), kUUIDSize, std::back_inserter(uuid_bytes));
            uuid_unparse(uuid_bytes.data(), ota_id_str);
            // 2 bytes skipping for the set_by value position.
            // The skipped 2 bytes represents ota_timer value (uint16) in minutes. We already convert and
            // set ota_timer_timestamp directly.
            set_by = data.at(kUUIDSize + 2);
        } else {
            std::strncpy(ota_id_str, kDefaultOTAId, kOtaIdSize + 1);
            set_by = -1;
        }
    }
};


/**
 * \brief Holds the diagnostic state for ota timer
 *
 * Typically the application creates an object of this class,
 * populates it from a storage, manipulates it, and writes it back
 * to the storage.
 *
 */
class OTATimerDiagData {
 public:
    OTATimerDiagData();
    ~OTATimerDiagData();
    OTATimerDiagData(const OTATimerDiagData& other) = delete;
    OTATimerDiagData(OTATimerDiagData&& other) = default;
    OTATimerDiagData& operator=(const OTATimerDiagData& other) = default;
    OTATimerDiagData& operator=(OTATimerDiagData&& other) = delete;

    void SetOTAScheduleSuccess(OTATimerDetailDiagData detail);
    void SetOTAScheduleFailed(OTATimerDetailDiagData detail);
    void SetOTATimerExpired();
    void SetOTAInitiateInstallationSuccess(OTATimerDetailDiagData detail);
    void SetOTAInitiateInstallationFailed(OTATimerDetailDiagData detail);

    uint8_t GetOTAScheduleSuccess() const;
    uint8_t GetOTAScheduleFailed() const;
    uint8_t GetOTATimerExpired() const;
    uint8_t GetOTAInitiateInstallationSuccess() const;
    uint8_t GetOTAInitiateInstallationFailed() const;
    OTATimerDetailDiagData* GetOTATimerDetailedDiagData();

    bool Clear();

    // Convenience functions processing OTATimerDiagData objects
    friend std::string ToJsonString(const OTATimerDiagData& data);
    friend bool FromJsonString(const std::string& json_string, OTATimerDiagData& data);     // NOLINT
    friend void ToByteVector(const OTATimerDiagData& data, std::vector<uint8_t>& outData);  // NOLINT
    friend bool operator==(const OTATimerDiagData& lhs, const OTATimerDiagData& rhs);

    static const int kMaxDiagEntries = 20;
    static const int kVersion = 1;

 private:
    void SaveEntry(OTATimerDetailDiagData data);
    uint8_t ota_schedule_success_ = 0;
    uint8_t ota_schedule_failed_ = 0;
    uint8_t ota_timer_expired_ = 0;
    uint8_t ota_initiate_installation_success_ = 0;
    uint8_t ota_initiate_installation_failed_ = 0;
    OTATimerDetailDiagData data_[kMaxDiagEntries];
    int next_entry_ = 0;
};

}  // namespace vocconv

#endif  // INCLUDE_DIAGNOSTICS_OTA_TIMER_DIAG_DATA_H_
/** \} */  // end of addtogroup
