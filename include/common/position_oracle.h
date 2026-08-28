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

#ifndef INCLUDE_COMMON_POSITION_ORACLE_H_
#define INCLUDE_COMMON_POSITION_ORACLE_H_

#include <mutex>

#include "text_handler.hpp"

#include "common/iposition_oracle.h"
#include "charging_common/charging_location_types.h"

namespace vocconv {

class PositionOracle : public IPositionOracle {
 public:
    PositionOracle();
    ~PositionOracle() override;

    PositionOracle(const PositionOracle& other) = delete;
    PositionOracle(PositionOracle&& other) = delete;
    PositionOracle& operator=(const PositionOracle& other) = delete;
    PositionOracle& operator=(PositionOracle&& other) = delete;

    bool IsAt(const remote_common::battery_charge::Position& position, const double max_distance) const override;

    void SetPosition(const remote_common::battery_charge::Position& position) override;
    void ClearPosition() override;
    remote_common::battery_charge::Position Position() const override;

 private:
    void PersistPosition();
    void LoadPositionFromDisk();
    void FromJsonV1(const Json::Value& json);
    void ResetState();
    remote_common::battery_charge::Position position_{0, 0};
    bool position_is_set_{false};
    persistency::TextHandler text_handler_{};
    mutable std::mutex mutex_;
};

}  // namespace vocconv
#endif  // INCLUDE_COMMON_POSITION_ORACLE_H_
/** \} */  // end of addtogroup
