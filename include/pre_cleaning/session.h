/*
 * Copyright (C) 2021 - Volvo Car Corporation
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

#ifndef INCLUDE_PRE_CLEANING_SESSION_H_
#define INCLUDE_PRE_CLEANING_SESSION_H_

#include <memory>
#include <mutex>

#include "app_framework/iiplm_controller.h"
#include "app_framework/signal_sources/timeout_receiver.h"
#include "app_framework/signals/timeout_signal.h"
#include "app_framework/transactions/transaction_id.h"
#include "pre_cleaning/air_qly_reporter.h"
#include "pre_cleaning/isession.h"
#include "local_config_reader/configs.h"


namespace vocconv {
namespace pre_cleaning {

class PreCleaningStatus;

/**
 * PreCleaning Session
 *
 * Takes care of actions during running PreCleaning:
 *  - Starts the AirQly reporter that sends updates every minute
 *  - Makes a request for IPLM during the session
 *  - Ends the session on timeout if not stopped manually
 */
class Session : public ISession, public fsm::TimeoutReceiver {
 public:
    Session(std::shared_ptr<fsm::IIPLMController> iplm_controller, std::shared_ptr<PreCleaningStatus> status,
            const local_config::pre_cleaning_tcam1::Config& pre_cleaning_config);
    void Start() override;
    void Stop() override;

 private:
    void HandleTimeout(std::shared_ptr<fsm::TimeoutSignal>) override;

    /** Mutex for protecting internal data. An instance of this class
     * will be accessed from two threads: the owning thread and the thread
     * handling timeouts through a call to HandleTimeout. This mutex is needed
     * to synchronize the access to the internal data.
     */
    std::mutex shared_data_mutex_;
    std::shared_ptr<fsm::IIPLMController> iplm_controller_;
    std::shared_ptr<PreCleaningStatus> status_;
    std::shared_ptr<AirQlyReporter> reporter_;
    fsm::TimeoutTransactionId timeout_id_;
    bool session_running_;
    const local_config::pre_cleaning_tcam1::Config pre_cleaning_config_;
};

}  // namespace pre_cleaning
}  // namespace vocconv

#endif  // INCLUDE_PRE_CLEANING_SESSION_H_
/** \} */  // end of addtogroup
