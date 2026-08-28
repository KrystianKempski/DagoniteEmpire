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

#ifndef INCLUDE_DIAGNOSTICS_OTA_TIMER_EXIT_CAUSES_H_
#define INCLUDE_DIAGNOSTICS_OTA_TIMER_EXIT_CAUSES_H_

#include <cstdint>

namespace vocconv {

enum class OTATimerExitCause: uint8_t {
    kUndefined = 0x00,

    kBadTimerExpiredSignal,
    kExpiredTimerIsOld,

    kUnhandledSetOTATimerSignal,
    kScheduled,
    kCancelled,
    kInvalidOtaTimer,

    kOTAAssignInvalidTimerData,
    kFailedToIssueIPLMRequest,
    kOTAAssignTransactionTimeout,
    kInitiated,

    // Put any new values above this line
    // Remember to update OTATimerDiagData::kVersion if you add an extra value to this enum.
    kInvalid  // Used for input validation.
};

}  // namespace vocconv

#endif  // INCLUDE_DIAGNOSTICS_OTA_TIMER_EXIT_CAUSES_H_
/** \} */  // end of addtogroup
