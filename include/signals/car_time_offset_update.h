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

#ifndef INCLUDE_SIGNALS_CAR_TIME_OFFSET_UPDATE_H_
#define INCLUDE_SIGNALS_CAR_TIME_OFFSET_UPDATE_H_

#include <cstdint>

#include "app_framework/signals/protobuf_signal.h"
#include "signals/protobuf/messages/ihu_util/CarTimeOffsetUpdate.pb.h"

namespace vocconv {

constexpr auto kCarTimeOffsetUpdateOid = "1.3.6.1.4.1.37916.3.6.40.0.0.1";
constexpr char kCarTimeOffsetUpdateName[] = "CarTimeOffsetUpdate";

/**
* \brief Used to send the current car time offset to the cloud
*/
class CarTimeOffsetUpdate : public fsm::ProtobufSignal<remote_control_CarTimeOffsetUpdate> {
 public:
    CarTimeOffsetUpdate();
    CarTimeOffsetUpdate(const CarTimeOffsetUpdate&) = delete;
    CarTimeOffsetUpdate(CarTimeOffsetUpdate&&) = delete;
    CarTimeOffsetUpdate& operator = (const CarTimeOffsetUpdate&) = delete;
    CarTimeOffsetUpdate& operator = (const CarTimeOffsetUpdate&&) = delete;
    ~CarTimeOffsetUpdate() = default;

    void SetCarTimeOffset(int32_t offset);
    int32_t GetCarTimeOffset() const;
};

}  // namespace vocconv
#endif  // INCLUDE_SIGNALS_CAR_TIME_OFFSET_UPDATE_H_
/** \} */  // end of addtogroup
