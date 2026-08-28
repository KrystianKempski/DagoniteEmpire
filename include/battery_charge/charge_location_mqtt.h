/*
 * Copyright (C) 2026 - Volvo Car Corporation
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

#ifndef INCLUDE_BATTERY_CHARGE_CHARGE_LOCATION_MQTT_H_
#define INCLUDE_BATTERY_CHARGE_CHARGE_LOCATION_MQTT_H_

#include <app_framework/signal_sources/mqtt_signal_source.h>
#include <user_manager/usermanager_interface.h>

#include <memory>

#include "utilities/imqtt_wrapper.h"
#include "utilities/mqtt_wrapper.h"
#include "utilities/result.h"

namespace vocconv {
/**
 * \class ChargeLocationMqtt
 * \brief Wrapper to expose data needed to manage Mqtt Signals for charging location use cases
 */
class ChargeLocationMqtt : public remote_common::MqttWrapper {
 public:
    ChargeLocationMqtt();
    ~ChargeLocationMqtt() override = default;

    ChargeLocationMqtt(const ChargeLocationMqtt& other) = delete;
    ChargeLocationMqtt(ChargeLocationMqtt&& other) = delete;
    ChargeLocationMqtt& operator=(const ChargeLocationMqtt& other) = delete;
    ChargeLocationMqtt& operator=(ChargeLocationMqtt&& other) = delete;

#ifndef UNIT_TESTS
 protected:
#endif

    /**
     * \brief Get the profile configuration for a given payload
     * \param payload The payload for which to get the profile configuration
     * \return The profile configuration if available, otherwise an error
     */

    remote_common::Result<remote_common::SignalConfig> SignalProfileConfig(
            const std::shared_ptr<fsm::PayloadInterface>& payload) const override;
};

}  // namespace vocconv
#endif  // INCLUDE_BATTERY_CHARGE_CHARGE_LOCATION_MQTT_H_
