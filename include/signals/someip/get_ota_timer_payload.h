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

#ifndef INCLUDE_SIGNALS_SOMEIP_GET_OTA_TIMER_PAYLOAD_H_
#define INCLUDE_SIGNALS_SOMEIP_GET_OTA_TIMER_PAYLOAD_H_

#include <uuid/uuid.h>
#include <cstdint>
#include <memory>
#include <vector>

#include "signals/car_signals.h"

namespace vocconv {

constexpr int kGetOtaTimerPayloadSize = kUUIDSize + 4;
constexpr int kGetOtaTimerRequestSize = kUUIDSize;

class GetOtaTimerPayload {
 private:
    typedef union {
        uint8_t value[kGetOtaTimerPayloadSize];
        struct {
            uuid_t otaid;
            uint8_t setby;
            int16_t otatimer;
            uint8_t otanotify;
        } __attribute__((packed, aligned(1))) bs;
    } GetOtaTimer;

 public:
    /**
     * Populate GetOtaTimer payload data with zeroes.
     **/
    GetOtaTimerPayload();

    /**
     * Populate GetOtaTimer data member in host order (little-endian)
     * \param payload raw payload for convertion to internal format
     *  with a byte vector in network order (big-endian).
     **/
    explicit GetOtaTimerPayload(const std::vector<vsomeip::byte_t>& payload);

    GetOtaTimerPayload(const GetOtaTimerPayload& other) = delete;
    GetOtaTimerPayload(GetOtaTimerPayload&& other) = delete;
    GetOtaTimerPayload& operator=(const GetOtaTimerPayload& other) = delete;
    GetOtaTimerPayload& operator=(GetOtaTimerPayload&& other) = delete;

    /**
     * Retrieve a byte vector in network order (big-endian) populated
     * with GetOtaTimer payload data in host order (little-endian)
     * with ota id set in uuid format.
     **/
    std::vector<vsomeip::byte_t> GetData() const;

    /**
     * Get ota id, represented as 16 bytes uuid, from this payload
     **/
    std::vector<uint8_t> otaid() const;

    /**
     * Get set by value from this payload, which can have two possible values:
     * OnBoard for payload received from IHU or OffBoard if payload was received
     * from remote device.
     **/
    uint8_t setby() const;

    /**
     * Get timer value in minutes from this payload, represented as 2 bytes value
     * (minimum value: 1, maximum value: 10100)
     **/
    int16_t otatimer() const;

    /**
     * Get ota notify value from this payload, which can have 2 possible vlaues:
     * true if IHU should notify the user about scheduled installation,
     * otherwise false
     **/
    uint8_t otanotify() const;

    /**
     * Set ota id in this payload, represented as 16 bytes uuid
     * \param value ota id represented as 16 bytes uuid
     **/
    void set_otaid(const std::vector<uint8_t>& value);

    /**
     * Set set by in this payload
     * \param value setby value, which is either 0 (OnBoard) or 1 (OffBoard)
     **/
    void set_setby(uint8_t value);

    /**
     * Set ota timer in this payload
     * \param value timer value in minutes, minimum value is 1, maximum
     * value is 10100
     **/
    void set_otatimer(int16_t value);

    /**
     * Set ota notify in this payload
     * \param value ota notify value, which is either 0 (do not notify user) or
     * 1 if IHU should notify user
     **/
    void set_otanotify(uint8_t value);

#ifndef UNIT_TESTS

 private:
#endif
    /**
     * Populate GetOtaTimer payload data in host (little-endian)
     * with a byte vector in network order (big-endian) with ota id
     * represented as uuid.
     * \param payload payload data in application internal format
     **/
    void SetData(const std::vector<vsomeip::byte_t>& payload);

    GetOtaTimer getotatimer_;
};

}  // namespace vocconv
#endif  // INCLUDE_SIGNALS_SOMEIP_GET_OTA_TIMER_PAYLOAD_H_
/** \} */  // end of addtogroup
