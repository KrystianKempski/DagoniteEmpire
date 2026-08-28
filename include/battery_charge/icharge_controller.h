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

#ifndef INCLUDE_BATTERY_CHARGE_ICHARGE_CONTROLLER_H_
#define INCLUDE_BATTERY_CHARGE_ICHARGE_CONTROLLER_H_

#include <string>
#include <vector>

#include "battery_charge/charge_control_status.h"
#include "battery_charge/charging_location.h"
#include "battery_charge/ilocation_storage.h"
#include "battery_charge/target_soc_types.h"
#include "signals/common_response_codes.h"

namespace vocconv {

class IChargeController {
 public:
    struct ChargeLocationsState {
        charging::ChargingLocation default_location;
        charging::ChargingLocation actual_location;
        bool actual_location_exists;
        std::vector<charging::ChargingLocation> locations;
    };

    IChargeController() = default;
    virtual ~IChargeController() = default;

    IChargeController(const IChargeController&) = delete;
    IChargeController(IChargeController&&) = delete;
    IChargeController& operator=(const IChargeController&) = delete;
    IChargeController& operator=(IChargeController&&) = delete;

    virtual void PositionUpdated() = 0;
    /**
     * \brief Create a new location in the storage, if it does not already exist.
     * \param location The location to create.
     * If the UUID field is empty, a new UUID will be generated.
     * \return The result of the operation, kSuccess if successful otherwise an error code.
     */
    virtual remote_common::CommonResponseCode CreateLocation(charging::ChargingLocation& location) = 0;

    /**
     * \brief Modify an existing location in the storage, if it exists.
     * \param location The location to modify.
     * \param settings Defines if special modification rules should be followed. Like inheriting smart charging
     * supported.
     * \return The result of the operation, kSuccess if successful otherwise an error code.
     */
    virtual remote_common::CommonResponseCode ModifyLocation(
            const charging::ChargingLocation& location, const LocationModificationSettings& settings) = 0;

    /**
     * \brief Delete a location from the storage, if it exists.
     * \param uuid The UUID of the location to delete.
     * \return The result of the operation.
     */
    virtual remote_common::CommonResponseCode DeleteLocation(const std::string& uuid) = 0;

    /**
     * \brief Modify a location's smart schedule if allowed.
     * \param uuid The UUID of the location to update timers for.
     * \param timers Vector that contains the OptimizedChargingTimers.
     * \return The result of the operation, kSuccess if successful otherwise an error code.
     */
    virtual remote_common::CommonResponseCode SetOptimizedChargingSchedule(
            const std::string& uuid, const std::vector<charging::OptimizedChargingTimer>& timers) = 0;

    /**
     * \brief Invalidate the smart schedule.
     * \param reason The reason for the invalidation, this will be sent to cloud.
     */
    virtual void InvalidateOptimizedChargingSchedule(
        const charging::OptimizedChargingScheduleInvalidationReason reason) = 0;

    virtual void SetVgmAvailability(const bool available) = 0;
    virtual void UpdateStateOfCharge(const float charge_level) = 0;

    /**
     * \brief Returns the current status of ChargeNow.
     */
    virtual bool GetChargeNow() const = 0;

    /**
     * \brief Setting Charge Now overrides any schedule when the car is at a location.
     *
     * \note This will have no effect if the car is not at a charging location.
     */
    virtual void SetChargeNow(const bool charge_now) = 0;

    /**
     * \brief Set the charging level to which the High Voltage Battery should be charged, e.g. 90%.
     *
     * Setting Target Soc will trigger recalculation of settings in case the setting differs from the current one
     * and if Custom, the new custom target soc value is valid (0-100) and differs from the current one.
     *
     * \param custom_target_soc Target Soc in percentage. This value is ignored when the setting is not Custom.
     * \param setting Allows the user to select predefined values for Target Soc, or set it to the custom value.
     * \return CommonResponseCode: OutOfRange if the `custom_target_soc` value is greater than 100.
     *                             InvalidArgument if `setting` is not a valid enum value.
     */
    virtual remote_common::CommonResponseCode SetTargetSoc(const uint32_t custom_target_soc,
                                                           const TargetSocSetting setting) = 0;

    /**
     * \brief This returns the current Target Soc % value and the setting.
     */
    virtual TargetSocBundle GetTargetSoc() const = 0;

    /**
     * Recalculate OBC settings and initiate transaction to send those to OBC
     *
     * \brief This will take current time, position, user settings etc into account and derive the required OBC settings
     * needed to charge accordingly. With these settings it will then trigger a transaction to OBC to set these
     * settings.
     *
     * \note Recalculate is called when a charging schedule is expired, when ota and workshop mode is turned off and
     * when the time offset is updated, which occurs at every startup and resume if appropriate.
     */
    virtual void Recalculate() = 0;

    /**
     * Get the current charging location state
     *
     * \brief This returns all saved charging locations, the default location and the actual location where the car
     * currently is.
     */
    virtual ChargeLocationsState GetLocationState() const = 0;

    /**
     * \brief Get the current charge control status (type and amp limit).
     */
    virtual ChargeControllerStatusUpdate GetChargeControlStatus() const = 0;
};

}  // namespace vocconv
#endif  // INCLUDE_BATTERY_CHARGE_ICHARGE_CONTROLLER_H_
/** \} */  // end of addtogroup
