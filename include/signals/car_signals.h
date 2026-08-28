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

/** \addtogroup VocConv
 *  \{
 */

#ifndef INCLUDE_SIGNALS_CAR_SIGNALS_H_
#define INCLUDE_SIGNALS_CAR_SIGNALS_H_

#include <vsomeip/vsomeip.hpp>

#include <cstdint>

namespace vocconv {

/*
 * All struct and enum member names are copied from
 * the signal database in Elektra.
 */
enum class OnOffNoReq : uint8_t {
    kNoReq = 0,
    kOn = 1,
    kOff = 2
};

// Ota id length, as a human readable string
constexpr int kOtaIdSize = 36;
// Fixed length zero padded UTF-8 string (incl null & BOM)
constexpr int kOtaIdSizeOnWire = kOtaIdSize * 4 + 4;
// UUID length
constexpr int kUUIDSize = 16;

enum class OtaNotifyType : uint8_t {
    kInitValue = 0,
    kTrue = 1,
    kFalse = 2
};

enum class SetByType : uint8_t {
    kNoNotify = 0,
    kOnboard = 1,
    kOffboard = 2
};

constexpr std::size_t kLocnItemPayloadMinSizeInBytes = 37;
constexpr std::size_t kLocnItemPayloadMaxSizeInBytes = 1061;
constexpr std::size_t kStoredLocnItemPayloadMinSizeInBytes = 36;
// LocnItem contains dynamic length strings.
// Charging current is on the 10th last byte of the LocnItem payload.
constexpr int kMaximumChargingCurrentReverseIndex = -10;
constexpr int kChargingTimerStartIndexFromEnd = 9;
constexpr int kChargingTimerStopIndexFromEnd = 5;
constexpr int kCarTiGlbDataSize = 4;
constexpr int kVehModMngtGlbSize = 10;
constexpr int kCarModeIndex = 1;

typedef union {
    uint8_t value;
    struct {
        uint8_t max_charging_current:7;
        uint8_t not_used:1;
    } __attribute__((packed, aligned(1))) bs;
} MaximumChargingCurrent;

constexpr std::size_t kBattLimPayloadSizeInBytes = 1;
constexpr int kChargingTargetPercentageIndex = 0;

}  // namespace vocconv
#endif  // INCLUDE_SIGNALS_CAR_SIGNALS_H_
/** \} */  // end of addtogroup
