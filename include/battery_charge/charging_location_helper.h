/*
 * Copyright (C) 2024 - Volvo Car Corporation
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

#ifndef INCLUDE_BATTERY_CHARGE_CHARGING_LOCATION_HELPER_H_
#define INCLUDE_BATTERY_CHARGE_CHARGING_LOCATION_HELPER_H_

#include <pb.h>

#include <memory>
#include <vector>

#include "battery_charge/charging_location.h"
#include "charging_common/charging_location_types.h"
#include "signals/common_response_codes.h"
#include "signals/protobuf/messages/batterycharge/icup_charging.pb.h"

namespace vocconv {
namespace charging {

using remote_common::battery_charge::ActiveDays;
using remote_common::battery_charge::DepartureTime;
using remote_common::battery_charge::ChargingTimer;
using remote_common::battery_charge::Position;

/**
 * \brief Get CreateChargingLocation from IHU<->TCAM proto schema type to TCAM internal type
 *
 * \param create_proto Incoming CreateChargingLocation from IHU<->TCAM protobuf payload
 *
 * \return CreateChargingLocation as TCAM internal type
 */
ChargingLocation GetCreateChargingLocationFromIhu(const icup_charging_CreateChargingLocationRequest& create_proto);

/**
 * \brief Get ModifyChargingLocation from IHU<->TCAM proto schema type to TCAM internal type
 *
 * \param modify_proto Incoming ModifyChargingLocation from IHU<->TCAM protobuf payload
 *
 * \return ModifyChargingLocation as TCAM internal type
 */
ChargingLocation GetModifyChargingLocationFromIhu(const icup_charging_ModifyChargingLocationRequest& modify_proto);

/**
 * \brief Get Schedule from IHU<->TCAM proto schema type to TCAM internal type
 *
 * \param schedule_proto Incoming Schedule from IHU<->TCAM protobuf payload
 *
 * \return Schedule as TCAM internal type
 */
ChargingSchedule GetScheduleFromIhu(const icup_charging_Schedule& schedule_proto);

/**
 * \brief Get Position from IHU<->TCAM proto schema type to TCAM internal type
 *
 * \param position_proto Incoming Position from IHU<->TCAM protobuf payload
 *
 * \return Position as TCAM internal type
 */
Position GetPositionFromIhu(const icup_charging_Position& position_proto);

/**
 * \brief Convert ManualTimer from IHU<->TCAM proto schema type to TCAM internal type
 *
 * \param manual_timer_ihu Incoming ManualTimer from IHU<->TCAM protobuf payload
 *
 * \return ManualTimer as TCAM internal type
 */
ChargingTimer ConvertManualTimerIhuToInternal(const icup_charging_ManualSchedule& manual_timer_ihu);

/**
 * \brief Convert DepartureTime from IHU<->TCAM proto schema type to TCAM internal type
 *
 * \param departure_time_ihu Incoming DepartureTime from IHU<->TCAM protobuf payload
 *
 * \return DepartureTime as TCAM internal type
 */
DepartureTime ConvertDepartureTimeIhuToInternal(const icup_charging_DepartureTime& departure_time_ihu);

/**
 * \brief Convert ChargingTimer from TCAM internal type to IHU protobuf
 *
 * \param manual_schedule Incoming ChargingTimer as TCAM internal type
 *
 * \return ManualSchedule as IHU protobuf
 */
icup_charging_ManualSchedule ConvertManualScheduleInternalToIhu(const ChargingTimer& manual_schedule);

/**
 * \brief Convert DepartureTime from TCAM internal type to IHU protobuf
 *
 * \param departure_time Incoming DepartureTime as TCAM internal type
 *
 * \return DepartureTime as IHU protobuf
 */
icup_charging_DepartureTime ConvertDepartureTimeInternalToIhu(const DepartureTime& departure_time);

/**
 * \brief Convert ChargingScheduleType from TCAM internal type to IHU protobuf
 *
 * \param position Incoming ChargingScheduleType as TCAM internal type
 *
 * \return ScheduleType as IHU protobuf
 */
icup_charging_ScheduleType ConvertChargingScheduleTypeInternalToIhu(const ChargingScheduleType& type);

/**
 * \brief Convert ActiveDays from TCAM internal type to IHU protobuf
 *
 * \param active_days Incoming ActiveDays as TCAM internal type
 *
 * \return ActiveDays as IHU protobuf
 */
icup_charging_ActiveDays ConvertActiveDaysInternalToIhu(const ActiveDays& active_days);

/**
 * \brief Convert ChargingTimer from TCAM internal type to IHU protobuf
 *
 * \param manual_schedule Incoming ChargingTimer as TCAM internal type
 *
 * \return ManualSchedule as IHU protobuf
 */
icup_charging_ManualSchedule ConvertManualScheduleInternalToIhu(const ChargingTimer& manual_schedule);

/**
 * \brief Convert DepartureTime from TCAM internal type to IHU protobuf
 *
 * \param departure_time Incoming DepartureTime as TCAM internal type
 *
 * \return DepartureTime as IHU protobuf
 */
icup_charging_DepartureTime ConvertDepartureTimeInternalToIhu(const DepartureTime& departure_time);

/**
 * \brief Convert ChargingScheduleType from TCAM internal type to IHU protobuf
 *
 * \param position Incoming ChargingScheduleType as TCAM internal type
 *
 * \return ScheduleType as IHU protobuf
 */
icup_charging_ScheduleType ConvertChargingScheduleTypeInternalToIhu(const ChargingScheduleType& type);

/**
 * \brief Decode ModifyChargingLocation from a vector of bytes to a proto schema type
 *
 * \param uuid Location uuid as a string
 * \param payload Incoming payload from IHU signal as a vector of bytes
 *
 * \return ModifyChargingLocation IHU<->TCAM proto schema type
 */
std::shared_ptr<icup_charging_ModifyChargingLocationRequest> DecodeModifyChargingLocation(
        const std::shared_ptr<std::vector<uint8_t>>& payload);

/**
 * \brief Decode DeleteChargingLocation from a vector of bytes to a proto schema type
 *
 * \param uuid Location uuid as a string
 * \param payload Incoming payload from IHU signal as a vector of bytes
 *
 * \return DeleteChargingLocation IHU<->TCAM proto schema type
 */
std::shared_ptr<icup_charging_DeleteChargingLocationRequest> DecodeDeleteChargingLocation(
        const std::shared_ptr<std::vector<uint8_t>>& payload);

/**
 * \brief Encode CreateChargingLocationResponse from a proto schema type to a vector of bytes
 *
 * \param uuid Location uuid as a string
 * \param response_code Response code as CommonResponseCode
 * \param response Data from proto schema type (CreateChargingLocationResponse)
 *
 * \return CreateChargingLocationResponse IHU<->TCAM vector of bytes
 */
std::shared_ptr<std::vector<uint8_t>> EncodeCreateChargingLocationResponse(
        const icup_charging_CreateChargingLocationResponse& response);

/**
 * \brief Validate that the OptimizedChargingTimers in a vector are correct.
 *
 * Checks that the vector isn't empty, doesn't contain too many timers, all timers have valid amp limit,
 * each timer has stop time later than start time, and that timers are in sequential order.
 *
 * \param timers Vector of OptimizedChargingTimer for a "smart charging schedule"
 * \return CommonResponseCode
 */
remote_common::CommonResponseCode ValidateOptimizedChargingTimers(
        const std::vector<charging::OptimizedChargingTimer>& timers);

}  // namespace charging
}  // namespace vocconv
#endif  // INCLUDE_BATTERY_CHARGE_CHARGING_LOCATION_HELPER_H_
/** \} */  // end of addtogroup
