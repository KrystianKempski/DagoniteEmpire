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

#ifndef INCLUDE_PRE_CLEANING_INTERIOR_REPORTER_H_
#define INCLUDE_PRE_CLEANING_INTERIOR_REPORTER_H_

#include <functional>
#include <memory>

#include "app_framework/signal_sources/timeout_receiver.h"

namespace vocconv {
namespace pre_cleaning {

class InteriorReporter : public fsm::TimeoutReceiver {
 public:
    explicit InteriorReporter(std::function<void()> send_update_cb = {});
    void SetSendUpdateCallback(std::function<void()> send_update_cb);
    void Start(int timeout_seconds);
    void Stop();

    /**
     * \brief Stores the timeout expired status.
     * \returns true if the reporter is started and then expired, otherwise false.
     */
    bool IsExpired();
#ifdef UNIT_TESTS
    fsm::TimeoutTransactionId id_;
#endif
 private:
    void HandleTimeout(std::shared_ptr<fsm::TimeoutSignal> timeout_signal) override;
    bool is_running_;
    bool is_expired_;
    std::mutex timeout_mutex_;
    std::function<void()> send_update_cb_;
};

}  // namespace pre_cleaning
}  // namespace vocconv
#endif  // INCLUDE_PRE_CLEANING_INTERIOR_REPORTER_H_
/** \} */  // end of addtogroup
