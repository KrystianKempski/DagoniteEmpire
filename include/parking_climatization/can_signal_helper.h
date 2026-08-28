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

#ifndef INCLUDE_PARKING_CLIMATIZATION_CAN_SIGNAL_HELPER_H_
#define INCLUDE_PARKING_CLIMATIZATION_CAN_SIGNAL_HELPER_H_

#include "app_framework/signals/vehicle_comm_signal.h"
#include "parking_climatization/prkg_clima_info.h"
#include "parking_climatization/prkg_clima_notif.h"

namespace vocconv {
namespace can_helper {
namespace climatization {

remote_common::PrkgClimaRunngSts ParseClimateState(const vc::ResGetPreClimatizationData& data);

remote_common::PrkgClimaReqTyp ParseClimateReqType(const vc::ResGetPreClimatizationData& data);

remote_common::PrkgClimaHeatgOrCoolgActn ParseClimateFunctionality(const vc::ResGetPreClimatizationData& data);

remote_common::UndefdTrueFalse1 ParseClimateTimer(const vc::ResGetPreClimatizationData& data);

remote_common::PrkgClimaNotifType ParseClimateNotification(const vc::ResGetPreClimatizationData& data);

remote_common::PrkgClimaNotifDest ParseClimateNotificationDest(const vc::ResGetPreClimatizationData& data);

remote_common::PrkgClimaInfo ParseClimatizationInfoData(const vc::ResGetPreClimatizationData& climate_data);

remote_common::PrkgClimaNotif ParseClimatizationNotifData(const vc::ResGetPreClimatizationData& climate_data);

}  // namespace climatization
}  // namespace can_helper
}  // namespace vocconv
#endif  // INCLUDE_PARKING_CLIMATIZATION_CAN_SIGNAL_HELPER_H_
/** \} */  // end of addtogroup
