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

#ifndef INCLUDE_PRE_CLEANING_PRE_CLEANING_SUPPORT_H_
#define INCLUDE_PRE_CLEANING_PRE_CLEANING_SUPPORT_H_

#include <cstdint>
#include <memory>

#include "app_framework/signals/ccm_signal.h"

namespace vocconv {
namespace pre_cleaning {

class PreCleaningStatus;

enum class PreClngStatusCode : uint32_t {
    kOk = 0,
    kProvisioning = 501,
    kDataSharingOff = 502,
    kWrongUsageMode = 503,
    kOtaModeOngoing = 504,
    kWorkshopMode = 505,
    kUnknownFasReturnCode = 506,
    kBusy = 507
};

/**
 * Send PreCleaningUpdate signal
 *
 * Sends the update signal with latest received PrkgClimaInfoSts
 * and PreClngNotifSts signals to connected devices and Digital Twin
 **/
void SendUpdate(const PreCleaningStatus& status);

/**
 * Send PreCleaningStatusResponse signal
 *
 * Sends a status response with latest received PrkgClimaInfoSts to
 * requester. Recipient, session-id and transaction-id are extracted from
 * the supplied request.
 *
 * @param request The received status request we are responding to
 * */
void SendStatus(const PreCleaningStatus& status, const std::shared_ptr<fsm::CcmSignal>& request);

void SendLockUpdate(const PreCleaningStatus& status, int64_t now);

void CreateAndSendPreCleaningResponseToRequester(const std::shared_ptr<fsm::CcmSignal>& request,
                                                 PreClngStatusCode code);

}  // namespace pre_cleaning
}  // namespace vocconv
#endif  // INCLUDE_PRE_CLEANING_PRE_CLEANING_SUPPORT_H_
/** \} */  // end of addtogroup
