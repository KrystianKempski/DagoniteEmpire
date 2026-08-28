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

#ifndef INCLUDE_SIGNALS_CHARGE_CONTROL_STATUS_UPDATE_H_
#define INCLUDE_SIGNALS_CHARGE_CONTROL_STATUS_UPDATE_H_

#include "app_framework/signals/protobuf_signal.h"
#include "battery_charge/charge_control_status.h"
#include "messages/batterycharge/ChargeControlStatusUpdate.pb.h"
#include "signals/vocconv_signal_types.h"

namespace vocconv {

class ChargeControlStatusUpdate : public fsm::ProtobufSignal<remote_control_ChargeControlStatusUpdate> {
 public:
    ChargeControlStatusUpdate();
    ChargeControlStatusUpdate(ChargeControlStatus status, uint32_t amp_limit);
    ChargeControlStatusUpdate(const ChargeControlStatusUpdate&) = delete;
    ChargeControlStatusUpdate(ChargeControlStatusUpdate&&) = delete;
    ChargeControlStatusUpdate& operator=(const ChargeControlStatusUpdate&) = delete;
    ChargeControlStatusUpdate& operator=(ChargeControlStatusUpdate&&) = delete;
    ~ChargeControlStatusUpdate() = default;

    static constexpr auto Oid() { return "1.3.6.1.4.1.37916.3.6.21.7.3.1"; }

    static constexpr auto Name() { return "ChargeControlStatusUpdate"; }

    static constexpr uint32_t Type() {
        return static_cast<uint32_t>(SignalTypes::kChargeControlStatusUpdateSignal);
    }
};

}  // namespace vocconv
#endif  // INCLUDE_SIGNALS_CHARGE_CONTROL_STATUS_UPDATE_H_
/** \} */  // end of addtogroup
