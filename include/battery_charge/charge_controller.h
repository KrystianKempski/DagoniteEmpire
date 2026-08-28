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

#ifndef INCLUDE_BATTERY_CHARGE_CHARGE_CONTROLLER_H_
#define INCLUDE_BATTERY_CHARGE_CHARGE_CONTROLLER_H_

#include <limits>
#include <memory>
#include <mutex>
#include <string>
#include <vector>

#include "local_config_reader/configs.h"
#include "utilities/itime_provider.h"
#include "utilities/result.h"

#include "battery_charge/charge_control_status.h"
#include "battery_charge/icharge_controller.h"
#include "battery_charge/icharge_support_sender.h"
#include "battery_charge/icharge_controller_storage.h"
#include "battery_charge/ilocation_storage.h"
#include "battery_charge/obc_settings.h"
#include "battery_charge/target_soc_types.h"
#include "common/iposition_oracle.h"
#include "timers/icharge_timer_support.h"
#include "timers/itimer_manager.h"

namespace vocconv {

/**
 * \class ChargeController
 * \brief Facilitates all Charge Timer and OBC control related functionality in VocConv.
 *
 * This class holds all data related to charging locations, target soc and amp limit.
 * ChargeController will send commands and updates to OBC, Cloud and IHU when any state
 * changes. That can be as a result of user interaction, charge timer expiration, VGM
 * wakeup/reboot, minimum target soc reached etc.
 */
class ChargeController : public IChargeController {
 public:
    explicit ChargeController(std::shared_ptr<IChargeSupportSender> charge_support_sender,
                              std::shared_ptr<IChargeTimerSupport> timer_support,
                              std::shared_ptr<IChargeControllerStorage> charge_controller_storage,
                              std::shared_ptr<IPositionOracle> position_oracle,
                              std::shared_ptr<ILocationStorage> location_storage,
                              std::shared_ptr<ITimerManager> timer_manager,
                              std::shared_ptr<remote_common::ITimeProvider> time_provider,
                              const local_config::charge_locations_tcam1::Config& config);
    ~ChargeController();

    ChargeController(const ChargeController& other) = delete;
    ChargeController(ChargeController&& other) = delete;
    ChargeController& operator=(const ChargeController& other) = delete;
    ChargeController& operator=(ChargeController&& other) = delete;

    void PositionUpdated() override;
    remote_common::CommonResponseCode CreateLocation(charging::ChargingLocation& location) override;
    remote_common::CommonResponseCode ModifyLocation(
            const charging::ChargingLocation& location, const LocationModificationSettings& settings) override;
    remote_common::CommonResponseCode DeleteLocation(const std::string& uuid) override;
    remote_common::CommonResponseCode SetOptimizedChargingSchedule(
            const std::string& uuid, const std::vector<charging::OptimizedChargingTimer>& timers) override;
    void InvalidateOptimizedChargingSchedule(
        const charging::OptimizedChargingScheduleInvalidationReason reason) override;
    void SetVgmAvailability(bool available) override;
    void UpdateStateOfCharge(const float charge_level) override;

    bool GetChargeNow() const override;
    void SetChargeNow(const bool charge_now) override;

    remote_common::CommonResponseCode SetTargetSoc(const uint32_t custom_target_soc,
                                                   const TargetSocSetting setting) override;

    TargetSocBundle GetTargetSoc() const override;

    std::vector<charging::ChargingLocation> GetLocations() const;
    IChargeController::ChargeLocationsState GetLocationState() const override;

    ChargeControllerStatusUpdate GetChargeControlStatus() const override;

    void Recalculate() override;

#ifndef UNIT_TESTS

 private:
#endif
    remote_common::Result<charging::ChargingLocation> GetPresentLocation() const;

    void RecalculateSettings();
    bool TargetSocChanged(const uint32_t custom_target_soc, const TargetSocSetting setting) const;
    void TriggerControllerStatusUpdate(const ChargeControllerStatusUpdate& status);
    void TriggerObcSettingsTransaction(const ObcSettings& settings) const;
    remote_common::Result<ObcSettings> CalculateManualSchedule(const std::vector<ChargingTimer>& timers) const;
    void InvalidateOptimizedChargingScheduleInternal(
            const charging::OptimizedChargingScheduleInvalidationReason reason);

    mutable std::mutex mutex_{};
    ChargeControllerStatusUpdate status_{ChargeControlStatus::kUnspecified, charging::kDefaultAmpLimit};
    std::shared_ptr<IChargeSupportSender> charge_support_sender_;
    std::shared_ptr<IChargeTimerSupport> timer_support_;
    std::shared_ptr<IChargeControllerStorage> charge_controller_storage_;
    std::shared_ptr<IPositionOracle> position_oracle_;
    std::shared_ptr<ILocationStorage> location_storage_;
    std::shared_ptr<ITimerManager> timer_manager_;
    std::shared_ptr<remote_common::ITimeProvider> time_provider_;
    bool vgm_available_{false};
    bool charge_now_{false};
    uint32_t target_soc_{charging::kDefaultTargetSoC};
    TargetSocSetting target_soc_setting_{charging::kDefaultTargetSocSetting};
    float charge_level_{std::numeric_limits<float>::quiet_NaN()};
    std::string actual_location_uuid_{};

    // More about inclusion & exclusion areas:
    // swap://SystemWeaver:3000/x04000000029CD42F (CarWeaver)
    // https://confluence.volvocars.biz/display/ARTREM/Charge+Location
    const double location_inclusion_distance_meters_;
    const double location_exclusion_distance_meters_;

    const int daily_trip_target_soc_;
    const int long_trip_target_soc_;
};

}  // namespace vocconv
#endif     // INCLUDE_BATTERY_CHARGE_CHARGE_CONTROLLER_H_
/** \} */  // end of addtogroup
