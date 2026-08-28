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

#ifndef INCLUDE_DIAGNOSTICS_OTA_TIMER_DIAG_PERSISTENT_STORAGE_H_
#define INCLUDE_DIAGNOSTICS_OTA_TIMER_DIAG_PERSISTENT_STORAGE_H_

#include <mutex>  // NOLINT
#include <string>

#include "diagnostics/ota_timer_diag_data.h"
#include "text_handler.hpp"

namespace vocconv {

/**
 * \brief Stores OTATimerDiagData as a json string
 * in persistent memory.
 *
 * This class can be used as storage policy template parameter
 * when instantiating the OTATimerDiag class.
 *
 * \note A shared mutex is aquired during construction and
 * is released when object is destructed. So an object of this
 * class should typically be short lived.
 *
 */
class OTATimerDiagPersistentStorage {
 public:
    OTATimerDiagPersistentStorage();
    ~OTATimerDiagPersistentStorage() = default;
    bool Read(OTATimerDiagData& data);  // NOLINT
    bool Write(const OTATimerDiagData& data);

 private:
    static std::mutex storage_mutex;
    std::unique_lock<std::mutex> lock_;
    persistency::TextHandler text_handler_;
};

}  // namespace vocconv

#endif  // INCLUDE_DIAGNOSTICS_OTA_TIMER_DIAG_PERSISTENT_STORAGE_H_
/** \} */  // end of addtogroup
