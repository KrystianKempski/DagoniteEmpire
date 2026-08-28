/*
 * Copyright (C) 2020 - Volvo Car Corporation
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

#ifndef INCLUDE_PARKING_CLIMATIZATION_CLIMA_TIMER_SUPPORT_H_
#define INCLUDE_PARKING_CLIMATIZATION_CLIMA_TIMER_SUPPORT_H_

#include <memory>

#include "parking_climatization/climatization_timer_list.h"
#include "common/vsomeip_support.h"
#include "signals/vocconv_signal_types.h"

namespace vocconv {

/**
 * Support functions and constants for ParkingClimatization timers feature
 */
namespace climatimersupport {

/**
 * Send notification to IHU based on the current timers list
 * \param timers_list List of all timers, that has been updated recently
**/
void NotifyIhuOnCurrentTimerList(SignalTypes notification,
        const std::shared_ptr<remote_common::VsomeIpSupport::VsomeIpData>& payload);

}  // namespace climatimersupport

}  // namespace vocconv
#endif  // INCLUDE_PARKING_CLIMATIZATION_CLIMA_TIMER_SUPPORT_H_
/** \} */  // end of addtogroup
