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

#ifndef INCLUDE_BATTERY_CHARGE_LOCATION_STORAGE_H_
#define INCLUDE_BATTERY_CHARGE_LOCATION_STORAGE_H_

#include <boost/uuid/uuid_generators.hpp>

#include <vector>
#include <mutex>
#include <string>
#include <unordered_map>

#include "local_config_reader/configs.h"

#include "battery_charge/ilocation_storage.h"
#include "battery_charge/charging_location.h"
#include "text_handler.hpp"

namespace vocconv {

class LocationStorage : public ILocationStorage {
 public:
    explicit LocationStorage(const local_config::charge_locations_tcam1::Config& config);

    LocationStorage(const LocationStorage&) = default;
    LocationStorage& operator=(const LocationStorage&) = default;
    LocationStorage(LocationStorage&&) = default;
    LocationStorage& operator=(LocationStorage&&) = default;

    ~LocationStorage() = default;

    LocationResult CreateLocation(charging::ChargingLocation& location) override;
    LocationResult ModifyLocation(
            charging::ChargingLocation location, const LocationModificationSettings& settings) override;
    LocationResult DeleteLocation(const std::string& uuid) override;

    remote_common::Result<charging::ChargingLocation> GetLocation(const std::string& uuid) const override;
    std::vector<charging::ChargingLocation> GetValidLocations() const override;

    remote_common::Result<OptimizedChargingSchedule> GetOptimizedChargingSchedule() const override;
    void SetOptimizedChargingSchedule(
            const std::string& uuid,
            const std::vector<charging::OptimizedChargingTimer>& timers) override;
    bool InvalidateOptimizedChargingSchedule() override;

#ifndef UNIT_TESTS

 private:
#endif
    void CreateFromPersistency();
    Json::Value OptimizedChargingScheduleToJson() const;
    void SaveToPersistency();
    void ResetCache();
    void FromJsonV1(const Json::Value& json);
    std::string CreateUuid();

    static constexpr uint32_t kMaxLocations = 20;
    std::unordered_map<std::string, charging::ChargingLocation> locations_{};
    remote_common::Result<OptimizedChargingSchedule> optimized_charging_schedule_{remote_common::Status::kError};
    mutable std::mutex mutex_;
    persistency::TextHandler text_handler_{};
    boost::uuids::random_generator generator_{};

    const double location_exclusion_radius_meters_;
};

}  // namespace vocconv
#endif  // INCLUDE_BATTERY_CHARGE_LOCATION_STORAGE_H_
/** \} */  // end of addtogroup
