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

#ifndef INCLUDE_PLUGINS_CHARGING_HISTORY_UPDATE_PLUGIN_H_
#define INCLUDE_PLUGINS_CHARGING_HISTORY_UPDATE_PLUGIN_H_

#include <boost/chrono.hpp>
#include <memory>
#include <string>

#include "charging_common/charging_history_helper.h"
#include "charging_common/icharging_history.h"
#include "feature_authorization_service_proxy/ifeature_authorization_service_proxy.h"
#include "updates/ivehicle_comm_update_plugin.h"
#include "updates/iupdate_plugin.h"
#include "updates/update_plugin_configuration.h"
#include "vc_message_payloads.hpp"

#include "signals/battery_charge_status/charging_history_update.h"

namespace vocconv {

using remote_common::CloudUpdate;
using remote_common::battery_charge::ChargingHistorySessionStatus;

struct GridEnergySignal {
    static constexpr auto signal_type = fsm::Signal::BasicSignalTypes::kEventGridEnergyChargingAccumulation;
    using Payload = vc::EventGridEnergyChargingAccumulation;
};

struct HvBattEnergySignal {
    static constexpr auto signal_type = fsm::Signal::BasicSignalTypes::kEventHVBattEnergyChargingAccumulation;
    using Payload = vc::EventHvBattEnergyChargingAccumulation;
};

struct ChargingCableStatusSignal {
    static constexpr auto signal_type = fsm::Signal::BasicSignalTypes::kEventBatteryChargeStatus;
    using Payload = vc::EventBatteryChargeStatus;
};

struct ResGetChargingCableStatusSignal {
    static constexpr auto signal_type = fsm::Signal::BasicSignalTypes::kGetBatteryChargeStatus;
    using Payload = vc::EventBatteryChargeStatus;
};

struct ChargingPowerSignal {
    static constexpr auto signal_type = fsm::Signal::BasicSignalTypes::kEventBatteryChargePower;
    using Payload = vc::EventChargingPowerForHmi;
};

/**
 * \class ChargingHistoryUpdatePlugin
 * \brief ChargingHistoryUpdate to be attached to CarStatusCloudUpdater
 */
class ChargingHistoryUpdatePlugin : public remote_common::IVehicleCommUpdatePlugin<
                                            ChargingHistoryUpdatePlugin,
                                            GridEnergySignal,
                                            HvBattEnergySignal,
                                            ChargingCableStatusSignal,
                                            ResGetChargingCableStatusSignal,
                                            ChargingPowerSignal> {
    template <typename...>
    friend struct remote_common::DispatchHelper;

 public:
    ChargingHistoryUpdatePlugin(
            std::shared_ptr<fas::IFeatureAuthorizationServiceProxy> fas_proxy,
            std::shared_ptr<remote_common::battery_charge::IChargingHistory> charging_history,
            std::shared_ptr<remote_common::UpdatePluginConfiguration> charging_history_update_configuration,
            boost::chrono::duration<int32_t> transition_settle_delay);

    ~ChargingHistoryUpdatePlugin() override = default;

    const std::string& Name() const override { return name_; }

    void AfterSendAction() override;

    /**
     * \brief Perform any action needed when TCAM resumes.
     *
     * When TCAM has been in suspend we cannot be entirely sure that the cable hasn't been unplugged, so the session is
     * ended here.
     *
     * \return CloudUpdate containing potential update towards cloud>
     */
    CloudUpdate OnResume() override;

 protected:
    CloudUpdate ProcessVehicleCommPayload(const vc::EventGridEnergyChargingAccumulation& payload);
    CloudUpdate ProcessVehicleCommPayload(const vc::EventHvBattEnergyChargingAccumulation& payload);
    CloudUpdate ProcessVehicleCommPayload(const vc::EventBatteryChargeStatus& payload);
    CloudUpdate ProcessVehicleCommPayload(const vc::EventChargingPowerForHmi& payload);

 private:
    bool FasVerification();

    /**
     * @brief Generic path: validate charging history data, build protobuf payload, apply FAS check,
     * and return a CloudUpdate with the provided delay.
     *
     * @param delay Delay to apply to the returned CloudUpdate.
     * @return CloudUpdate containing the prepared charging history payload.
     */
    CloudUpdate PrepareDataToBeSent(boost::chrono::duration<int32_t> delay);

    /**
     * @brief Transition path: same as PrepareDataToBeSent, but guarded so only one transition-driven
     * delayed update can be pending at a time.
     *
     * Used for "charging done" / "connected but not charging" transitions that may arrive
     * repeatedly, but do not cause any new data to be added to the charging history.
     *
     * @param delay Delay to apply to the returned CloudUpdate.
     * @return CloudUpdate containing the prepared charging history payload.
     */
    CloudUpdate PrepareTransitionDataToBeSent(boost::chrono::duration<int32_t> delay);

    bool SessionIsActive() const;

    mutable std::mutex mutex_;
    std::string name_{remote_common::ChargingHistoryUpdate::Name()};
    std::shared_ptr<fas::IFeatureAuthorizationServiceProxy> fas_proxy_;
    std::shared_ptr<remote_common::battery_charge::IChargingHistory> charging_history_;
    std::shared_ptr<remote_common::UpdatePluginConfiguration> charging_history_update_configuration_;
    const boost::chrono::duration<int32_t> transition_settle_delay_;
    ChargingHistorySessionStatus session_status_;
    bool transition_send_pending_{false};
};

}  // namespace vocconv

#endif  // INCLUDE_PLUGINS_CHARGING_HISTORY_UPDATE_PLUGIN_H_

/** \} */  // end of addtogroup
