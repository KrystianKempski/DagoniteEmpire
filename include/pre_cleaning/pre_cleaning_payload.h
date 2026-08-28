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

#ifndef INCLUDE_PRE_CLEANING_PRE_CLEANING_PAYLOAD_H_
#define INCLUDE_PRE_CLEANING_PRE_CLEANING_PAYLOAD_H_

#include <memory>

#include "app_framework/signals/signal.h"
#include "parking_climatization/prkg_clima_info.h"
#include "pre_cleaning/air_qly_status.h"
#include "pre_cleaning/pre_clng_notif.h"
#include "vc_message_payloads.hpp"

namespace vocconv {
namespace pre_cleaning {

/**
 * Interface for extracting pre cleaning and parking climatization
 * payloads from status signals
 *
 * Hides the details. Transactions and Features should use this interface.
 **/
namespace payload {

bool ExtractParkingClimaInfo(const std::shared_ptr<fsm::Signal>& signal, remote_common::PrkgClimaInfo* status);
bool ExtractPreClngNotif(const std::shared_ptr<fsm::Signal>& signal, remote_common::PreClngNotif* notif);
bool ExtractAirQlyStatus(const std::shared_ptr<fsm::Signal>& signal, remote_common::AirQlyStatus* air_qly);
bool ExtractDoorLockStatus(const std::shared_ptr<fsm::Signal>& signal, vc::ResDoorLockUnlock& status);
bool ExtractWindowStatus(const std::shared_ptr<fsm::Signal>& signal, vc::ResGetWindowPosition& status);
bool ExtractUsageMode(const std::shared_ptr<fsm::Signal>& signal, vc::CarUsageModeState* usage_mode);

}  // namespace payload
}  // namespace pre_cleaning
}  // namespace vocconv
#endif  // INCLUDE_PRE_CLEANING_PRE_CLEANING_PAYLOAD_H_
/** \} */  // end of addtogroup
