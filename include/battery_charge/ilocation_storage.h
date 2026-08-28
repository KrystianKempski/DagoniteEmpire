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

#ifndef INCLUDE_BATTERY_CHARGE_ILOCATION_STORAGE_H_
#define INCLUDE_BATTERY_CHARGE_ILOCATION_STORAGE_H_

#include <boost/optional.hpp>
#include <vector>
#include <string>

#include "utilities/result.h"

#include "battery_charge/charging_location.h"
#include "signals/common_response_codes.h"

namespace vocconv {

using LocationResult = remote_common::Result<charging::ChargingLocation, remote_common::CommonResponseCode>;
struct OptimizedChargingSchedule {
    std::string uuid;
    std::vector<charging::OptimizedChargingTimer> timers;
};

struct LocationModificationSettings {
    bool inherit_smart_charging_supported = false;
    bool inherit_position = false;

    bool operator==(const LocationModificationSettings& other) const {
        return inherit_smart_charging_supported == other.inherit_smart_charging_supported &&
               inherit_position == other.inherit_position;
    }
};

class ILocationStorage {
 public:
    virtual ~ILocationStorage() = default;

    /**
     * \brief Create a new location in the storage, if it does not already exist.
     * \param location The location to create.
     * If the UUID field is empty, a new UUID will be generated.
     * \return The result of the operation, location if successful otherwise an error code.
     */
    virtual LocationResult CreateLocation(charging::ChargingLocation& location) = 0;
    /**
     * \brief Modify an existing location in the storage, if it exists.
     * \param location The location to modify.
     * \param settings Defines if special modification rules should be followed. Like inheriting smart charging
     * supported.
     * \return The result of the operation, location if successful otherwise an error code.
     */
    virtual LocationResult ModifyLocation(
            charging::ChargingLocation location, const LocationModificationSettings& settings) = 0;
    /**
     * \brief Delete a location from the storage, if it exists.
     * \param uuid The UUID of the location to delete.
     * \return The result of the operation, location if successful otherwise an error code.
     */
    virtual LocationResult DeleteLocation(const std::string& uuid) = 0;

    /**
     * \brief Get the location with the specified UUID.
     * \param uuid The UUID of the location to get.
     * \return The location with the specified UUID. No value (error) if no location with that uuid exists.
     */
    virtual remote_common::Result<charging::ChargingLocation> GetLocation(const std::string& uuid) const = 0;

    /**
     * \brief Get all valid locations.
     * \return A vector of all valid locations.
     */
    virtual std::vector<charging::ChargingLocation> GetValidLocations() const = 0;

    /**
     * \brief Get the stored Optimized Charging Schedule.
     *
     * @return remote_common::Result<OptimizedChargingSchedule> with an error if there is no schedule.
     */
    virtual remote_common::Result<OptimizedChargingSchedule> GetOptimizedChargingSchedule() const = 0;

    /**
     * \brief Set optimized charging schedule timers.
     *
     * These timers are calculated by cloud based on charging location settings, spot prices etc.
     *
     * \param timers Smart Schedule timers to send to OBC.
     */
    virtual void SetOptimizedChargingSchedule(const std::string& uuid,
                                              const std::vector<charging::OptimizedChargingTimer>& timers) = 0;
    /**
     * \brief Invalidates the Optimized Charging Schedule.
     *
     * This sets the internal Result<OptimizedChargingSchedule> to not have a value and
     * stores a schedule with empty uuid and empty timer list persistently.
     * \return if there was a schedule to invalidate or not. This can be used to determine if an update should be sent.
     */
    virtual bool InvalidateOptimizedChargingSchedule() = 0;

 protected:
    ILocationStorage() = default;
};

}  // namespace vocconv
#endif  // INCLUDE_BATTERY_CHARGE_ILOCATION_STORAGE_H_
/** \} */  // end of addtogroup
