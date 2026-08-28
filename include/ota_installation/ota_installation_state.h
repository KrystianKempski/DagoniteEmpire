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

#ifndef INCLUDE_OTA_INSTALLATION_OTA_INSTALLATION_STATE_H_
#define INCLUDE_OTA_INSTALLATION_OTA_INSTALLATION_STATE_H_

#include <boost/thread/mutex.hpp>
#include <cstdint>
#include <memory>

#include "utilities/itime_provider.h"

#include "ota_installation/iota_installation_state.h"

namespace vocconv {

class OtaInstallationState : public IOtaInstallationState {
 public:
    /**
     * Creates instance of OtaInstallationState and populates
     * the status from persistency if exists.
     **/
    explicit OtaInstallationState(std::shared_ptr<remote_common::ITimeProvider> time_provider);

    OtaInstallationState(const OtaInstallationState& other) = delete;
    OtaInstallationState(OtaInstallationState&& other) = delete;
    OtaInstallationState& operator=(const OtaInstallationState& other) = delete;
    OtaInstallationState& operator=(OtaInstallationState&& other) = delete;

    /**
     * Sets OtaStatus and saves it to persistency.
     * \param status The status that will be set and saved.
     **/
    void set_status(OtaStatus status) override;

    /**
     * Gets currently set OtaStatus.
     * \return current ota status.
     **/
    OtaStatus status() const override;

    /**
     * Gets SchedulingReminderState, based on which VocConv decides if user
     * should be reminded about scheduled installation.
     * \return scheduling reminder state.
     **/
    SchedulingReminderState scheduling_reminder_state() const override;

    /**
     * Increments current scheduling reminder state, if last increment happened
     * more then 20h ago.
     **/
    void IncrementSchedulingReminderState() override;

#ifndef UNIT_TESTS

 private:
#endif
    void PopulateOtaInstallStatusFromPersistency();
    void PopulateSchedulingReminderStateFromPersistency();
    void SetSchedulingReminderState(SchedulingReminderState state);
    void WriteOtaInstallStatusToPersistency();

    OtaStatus status_;
    SchedulingReminderState scheduling_reminder_state_;
    boost::mutex persistence_mutex_;
    std::shared_ptr<remote_common::ITimeProvider> time_provider_;
};

}  // namespace vocconv
#endif  // INCLUDE_OTA_INSTALLATION_OTA_INSTALLATION_STATE_H_
/** \} */  // end of addtogroup
