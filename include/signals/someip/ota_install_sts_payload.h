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

#ifndef INCLUDE_SIGNALS_SOMEIP_OTA_INSTALL_STS_PAYLOAD_H_
#define INCLUDE_SIGNALS_SOMEIP_OTA_INSTALL_STS_PAYLOAD_H_

#include <cstdint>
#include <vector>

#include "vsomeip/primitive_types.hpp"

namespace vocconv {

constexpr int kOtaInstallStsPayloadSizeInBytes = 1;

class OtaInstallStsPayload {
 private:
    typedef union {
        uint8_t value[kOtaInstallStsPayloadSizeInBytes];
        struct {
            uint8_t otastatus;
        } __attribute__((packed, aligned(1))) bs;
    } OtaInstallSts;

 public:
    /**
     * Populate OtaInstallSts payload data in host order (little-endian)
     * with a byte vector in network order (big-endian).
     * \param payload raw payload for convertion to internal format
     **/
    explicit OtaInstallStsPayload(const std::vector<vsomeip::byte_t>& payload);

    /**
     * Construct an empty OtaInstallStsPayload. Value can then be set by: set_otastatus
     **/
    OtaInstallStsPayload();

    OtaInstallStsPayload(const OtaInstallStsPayload& other) = delete;
    OtaInstallStsPayload(OtaInstallStsPayload&& other) = delete;
    OtaInstallStsPayload& operator=(const OtaInstallStsPayload& other) = delete;
    OtaInstallStsPayload& operator=(OtaInstallStsPayload&& other) = delete;

    /**
     * Retrieve a byte vector in network order (big-endian) populated
     *        with a OtaInstallSts data member in host order (little-endian).
     **/
    std::vector<vsomeip::byte_t> GetData() const;

    /**
     * Retrieve ota installation status byte from payload, which can be set to:
     * 0 - NoNotify,
     * 1 - DownloadCompleted,
     * 2 - InstallationScheduled,
     * 3 - InstallationInitiated,
     * 4 - InstallationStarted,
     * 5 - InstallationCompleted
     **/
    uint8_t otastatus() const;

#if defined(UNIT_TESTS) || defined(ENABLE_SIGNAL_INJECTION)
    void set_otastatus(uint8_t value);
#endif

 private:
    /**
     * Copies payload data to OtaInstallStsPayload instance
     * \param payload payload received from network represented as bytes vector
     **/
    void SetData(const std::vector<vsomeip::byte_t>& payload);

    OtaInstallSts otainstallsts_;
};

}  // namespace vocconv
#endif  // INCLUDE_SIGNALS_SOMEIP_OTA_INSTALL_STS_PAYLOAD_H_
/** \} */  // end of addtogroup
