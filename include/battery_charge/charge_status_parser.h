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

#ifndef INCLUDE_BATTERY_CHARGE_CHARGE_STATUS_PARSER_H_
#define INCLUDE_BATTERY_CHARGE_CHARGE_STATUS_PARSER_H_

#include <cstdint>

#include "return_codes/feature_authorization_return_codes.h"
#include "signals/protobuf/entities/batterycharge/BatteryChargeStatus.pb.h"
#include "vc_message_payloads.hpp"

namespace vocconv {

namespace charge_status_parser {

/**
 * Codes used to communicate errors to the mobile app
 */
enum class ChargeStatusErrorCode : int32_t {
    kUnknown = 0,
    kAllDataNotAvailable,
    kProvisioning,
    kDataSharingOff,
    kFasProxySdbusTimeout,
    kFasProxySdbusGeneralError,
    kFasNotRunning,
};

status_ObcHandleStatus ParseOnboardChargerHandleStatus(vc::BatteryOnboardCharger status);
status_ChargerStatus ParseChargerStatus(vc::BatteryChargerState state);
status_ChargingType ParseChargingType(vc::BatteryChargingType type);
status_CancelHoldChargeRequest ParseCancelHoldChargeRequest(bool cancel_request);
status_HoldChargeRequest ParseHoldChargeRequest(vc::HldChargingReq hold_charge_request);
status_ChargeConnectorLockStatus ParseChargeConnectorLockStatus(vc::HmiChargeLockStatus lock_status);

/**
 * \brief Converts a FAS defined error to a Charge Status error.
 * \param fas_return_code The code returned by FAS
 * \return ChargeStatusErrorCode The FAS error as a ChargeStatus error
 */
ChargeStatusErrorCode GetChargeStatusErrorCode(fas::FasReturnCode fas_return_code);

}  // namespace charge_status_parser

}  // namespace vocconv

#endif  // INCLUDE_BATTERY_CHARGE_CHARGE_STATUS_PARSER_H_

/** \} */  // end of addtogroup
