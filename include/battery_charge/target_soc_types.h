/*
 * Copyright (C) 2025 - Volvo Car Corporation
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

#ifndef INCLUDE_BATTERY_CHARGE_TARGET_SOC_TYPES_H_
#define INCLUDE_BATTERY_CHARGE_TARGET_SOC_TYPES_H_

#include <string>

#include "utilities/result.h"

#include "signals/protobuf/entities/batterycharge/ChargeTargetLevelSettings.pb.h"
#include "signals/protobuf/messages/batterycharge/icup_charging.pb.h"

namespace vocconv {

/**
 * \brief Enum for Target SoC settings (Daily, Long-Trip, Custom)
 *
 * The values are defined to the same numbers as in the protos. This will make
 * it easier to read logs and the persistent storage json etc.
 */
enum class TargetSocSetting {
    kDaily = 1,     // Daily sets Target SoC to 90%
    kLongTrip = 2,  // LongTrip sets Target SoC to 100%
    kCustom = 3     // Custom sets Target SoC to a value chosen by the user
};

/**
 * \brief Bundles the Target Soc % value and the setting.
 */
struct TargetSocBundle {
    uint32_t target_soc;       // Target Soc value in %
    TargetSocSetting setting;  // Daily, Long-Trip, Custom
};

inline std::string TargetSocSettingToString(TargetSocSetting setting) {
    switch (setting) {
        case TargetSocSetting::kDaily:
            return "Daily";
        case TargetSocSetting::kLongTrip:
            return "LongTrip";
        case TargetSocSetting::kCustom:
            return "Custom";
        default:
            return "undefined";
    }
}

inline remote_common::Result<TargetSocSetting> ToTargetSocSetting(
        remote_control_ChargeTargetLevelSettingType setting_type_cloud) {
    switch (setting_type_cloud) {
        case remote_control_ChargeTargetLevelSettingType_CHARGE_TARGET_LEVEL_SETTINGS_TYPE_UNSPECIFIED:
            return remote_common::Result<TargetSocSetting>{remote_common::Status::kError};
        case remote_control_ChargeTargetLevelSettingType_CHARGE_TARGET_LEVEL_SETTINGS_TYPE_DAILY:
            return remote_common::Result<TargetSocSetting>{
                    TargetSocSetting::kDaily};
        case remote_control_ChargeTargetLevelSettingType_CHARGE_TARGET_LEVEL_SETTINGS_TYPE_LONG_TRIP:
            return remote_common::Result<TargetSocSetting>{
                    TargetSocSetting::kLongTrip};
        case remote_control_ChargeTargetLevelSettingType_CHARGE_TARGET_LEVEL_SETTINGS_TYPE_CUSTOM:
            return remote_common::Result<TargetSocSetting>{
                    TargetSocSetting::kCustom};
        default:
            return remote_common::Result<TargetSocSetting>{remote_common::Status::kError};
    }
}

inline remote_common::Result<TargetSocSetting> ToTargetSocSetting(
        icup_charging_TargetSocSetting setting_type_someip) {
    switch (setting_type_someip) {
        case icup_charging_TargetSocSetting_TARGET_SOC_SETTING_UNSPECIFIED:
            return remote_common::Result<TargetSocSetting>{remote_common::Status::kError};
        case icup_charging_TargetSocSetting_TARGET_SOC_SETTING_DAILY:
            return remote_common::Result<TargetSocSetting>{
                    TargetSocSetting::kDaily};
        case icup_charging_TargetSocSetting_TARGET_SOC_SETTING_LONG_TRIP:
            return remote_common::Result<TargetSocSetting>{
                    TargetSocSetting::kLongTrip};
        case icup_charging_TargetSocSetting_TARGET_SOC_SETTING_CUSTOM:
            return remote_common::Result<TargetSocSetting>{
                    TargetSocSetting::kCustom};
        default:
            return remote_common::Result<TargetSocSetting>{remote_common::Status::kError};
    }
}

}  // namespace vocconv

#endif  // INCLUDE_BATTERY_CHARGE_TARGET_SOC_TYPES_H_
