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

#ifndef INCLUDE_COMMON_IPOSITION_ORACLE_H_
#define INCLUDE_COMMON_IPOSITION_ORACLE_H_

#include "charging_common/charging_location_types.h"

namespace vocconv {

class IPositionOracle {
 public:
    IPositionOracle() = default;
    virtual ~IPositionOracle() = default;

    IPositionOracle(const IPositionOracle&) = delete;
    IPositionOracle(IPositionOracle&&) = delete;
    IPositionOracle& operator=(const IPositionOracle&) = delete;
    IPositionOracle& operator=(IPositionOracle&&) = delete;

    virtual bool IsAt(const remote_common::battery_charge::Position& position, const double max_distance) const = 0;
    virtual void SetPosition(const remote_common::battery_charge::Position& position) = 0;
    virtual void ClearPosition() = 0;
    virtual remote_common::battery_charge::Position Position() const = 0;
};

}  // namespace vocconv
#endif  // INCLUDE_COMMON_IPOSITION_ORACLE_H_

/** \} */  // end of addtogroup
