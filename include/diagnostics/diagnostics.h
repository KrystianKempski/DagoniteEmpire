/*
 * Copyright (C) 2023 - Volvo Car Corporation
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

#ifndef INCLUDE_DIAGNOSTICS_DIAGNOSTICS_H_
#define INCLUDE_DIAGNOSTICS_DIAGNOSTICS_H_

#include <memory>
#include <vector>

#include "diag_plugin/diag_plugin.h"
#include "ota_installation/ota_installation_state.h"
#include "timers/itimer_manager.h"

#define VOCCONV_DBUS_DIAG_SERVICE         "com.volvocars.vocconv"
#define VOCCONV_DBUS_DIAG_PLUGIN          "/com/volvocars/vocconv/diag"

namespace vocconv {

constexpr uint16_t kDidResetOTA = 0x20C3;
constexpr uint16_t kDidDiagOTATimer = 0xA019;

/**
 * @brief Provides support for diagnostics DIDs in VocConv.
 *
 */
class Diagnostics : public tcam::DiagPluginCallbacks {
 public:
    Diagnostics(std::shared_ptr<ITimerManager> timer_manager,
                std::shared_ptr<OtaInstallationState> ota_installation_state);
    ~Diagnostics() {}

    Diagnostics(const Diagnostics& other) = delete;
    Diagnostics(Diagnostics&& other) = delete;
    Diagnostics& operator=(const Diagnostics& other) = delete;
    Diagnostics& operator=(Diagnostics&& other) = delete;

    /**
     * @brief   RegisterServices callback.
     * Event handler triggered when diag service is available.
     * @param registration  DiagPluginRegistration instance used for service registration
     */
    void RegisterServices(tcam::DiagPluginRegistration& registration) override;

    /**
     * @brief Callback from DiagPlugin for read DIDs.
     * @param id - DID id
     * @param data - Output data
     * @return UDSResponseCode - UDS response code
     */
    UDSResponseCode HandleReadDid(uint16_t id, std::vector<uint8_t>& data) override;

    /**
     * @brief Event handler triggered when a did write request arrives.
     * @param id DID id previously registered using DiagPluginRegistration::RegisterWriteDid
     * @param data Incoming data being sent to the TCAM. Not needed so far.
     */
    UDSResponseCode HandleWriteDid(uint16_t id, const std::vector<uint8_t>& data) override;

 private:
    std::unique_ptr<tcam::DiagPlugin> diag_plugin_;
    std::shared_ptr<ITimerManager> timer_manager_;
    std::shared_ptr<OtaInstallationState> installation_state_;
};

}  // namespace vocconv
#endif  // INCLUDE_DIAGNOSTICS_DIAGNOSTICS_H_
/** \} */  // end of addtogroup
