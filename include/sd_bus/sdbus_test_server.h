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

/** \addtogroup SdbusVocConv
 *  \{
 */

#ifndef INCLUDE_SD_BUS_SDBUS_TEST_SERVER_H_
#define INCLUDE_SD_BUS_SDBUS_TEST_SERVER_H_

#include <systemd/sd-bus.h>
#include <systemd/sd-event.h>
#include <memory>
#include <vector>

#include "app_framework/features/feature.h"
#include "common/car_config_util.h"
#include "updates/update_plugin_configuration.h"
#include "utilities/time_provider.h"
#include "utilities/car_time_offset_oracle.h"

#include "battery_charge/charge_controller.h"
#include "battery_charge/charge_controller_storage.h"
#include "battery_charge/location_storage.h"
#include "charging_common/charging_history.h"
#include "common/position_oracle.h"
#include "timers/timer_manager.h"
#include "timers/vuc_rtc.h"

namespace vocconv {
/**
 * Sd-bus module for handling injections from separate client application.
 */
namespace sdbus {

/**
 * \brief This type holds the instance data of the root object of the service.
 *
 * In a real world scenario, the type should have a proper name,
 * reflecting the logical meaning.
 * auto_* properties are automatically handled by sd-bus,
 * no explicit get/set-handlers needed.
 */
struct SdBusObject {
    sd_bus* bus;
};

class SdBusWrapper {
 public:
    SdBusWrapper(sd_event* event, const std::vector<fsm::Feature*>& feature_list,
                 std::shared_ptr<vocconv::TimerManager> timer_manager, std::shared_ptr<vocconv::VucRTC> vuc_rtc,
                 std::shared_ptr<vocconv::ChargeController> charge_controller,
                 std::shared_ptr<vocconv::ChargeControllerStorage> charge_controller_storage,
                 std::shared_ptr<vocconv::LocationStorage> location_storage,
                 std::shared_ptr<vocconv::PositionOracle> position_oracle,
                 std::shared_ptr<remote_common::TimeProvider> time_provider,
                 std::shared_ptr<remote_common::CarConfigUtil> car_config_util,
                 std::shared_ptr<remote_common::battery_charge::ChargingHistory> charging_history,
                 std::shared_ptr<remote_common::UpdatePluginConfiguration> charging_history_update_configuration,
                 std::shared_ptr<remote_common::CarTimeOffsetOracle> car_time_offset_oracle);
    ~SdBusWrapper();

    const std::vector<fsm::Feature*>* feature_list() const;
    std::shared_ptr<vocconv::TimerManager> timer_manager() const;
    std::shared_ptr<vocconv::VucRTC> vuc_rtc() const;
    std::shared_ptr<vocconv::ChargeController> charge_controller() const;
    std::shared_ptr<vocconv::LocationStorage> location_storage() const;
    std::shared_ptr<vocconv::PositionOracle> position_oracle() const;
    std::shared_ptr<remote_common::TimeProvider> time_provider() const;
    std::shared_ptr<vocconv::ChargeControllerStorage> charge_controller_storage() const;
    std::shared_ptr<remote_common::CarConfigUtil> car_config_util() const;
    std::shared_ptr<remote_common::battery_charge::ChargingHistory> charging_history() const;
    std::shared_ptr<remote_common::UpdatePluginConfiguration> charging_history_update_configuration() const;
    std::shared_ptr<remote_common::CarTimeOffsetOracle> car_time_offset_oracle() const;

 private:
    const std::vector<fsm::Feature*>* feature_list_;
    std::shared_ptr<vocconv::TimerManager> timer_manager_;
    std::shared_ptr<vocconv::VucRTC> vuc_rtc_;
    std::shared_ptr<vocconv::ChargeController> charge_controller_;
    std::shared_ptr<vocconv::ChargeControllerStorage> charge_controller_storage_;
    std::shared_ptr<vocconv::LocationStorage> location_storage_;
    std::shared_ptr<vocconv::PositionOracle> position_oracle_;
    std::shared_ptr<remote_common::TimeProvider> time_provider_;
    std::shared_ptr<remote_common::CarConfigUtil> car_config_util_;
    std::shared_ptr<remote_common::battery_charge::ChargingHistory> charging_history_;
    std::shared_ptr<remote_common::UpdatePluginConfiguration> charging_history_update_configuration_;
    std::shared_ptr<remote_common::CarTimeOffsetOracle> car_time_offset_oracle_;

    sd_bus_slot* slot_ = nullptr;
    sd_bus* bus_ = nullptr;
    vocconv::sdbus::SdBusObject* obj_;
};

}  // namespace sdbus
}  // namespace vocconv

#endif     // INCLUDE_SD_BUS_SDBUS_TEST_SERVER_H_
/** \} */  // end of addtogroup
