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

#ifndef INCLUDE_PRE_CLEANING_AIR_QLY_REPORTER_H_
#define INCLUDE_PRE_CLEANING_AIR_QLY_REPORTER_H_

#include <memory>

#include "app_framework/signal_sources/timeout_receiver.h"
#include "local_config_reader/configs.h"

namespace vocconv {
namespace pre_cleaning {
class PreCleaningStatus;
}  // namespace pre_cleaning

class AirQlyReporter : public fsm::TimeoutReceiver {
 public:
    AirQlyReporter(const local_config::pre_cleaning_tcam1::Config& pre_cleaning_config,
                   std::shared_ptr<const pre_cleaning::PreCleaningStatus> status);
    virtual ~AirQlyReporter();
    void Init();
#ifndef UNIT_TESTS
 private:
#endif
    void HandleTimeout(std::shared_ptr<fsm::TimeoutSignal> timeout_signal) override;
    unsigned minutes_left_;
    std::shared_ptr<const pre_cleaning::PreCleaningStatus> status_;
    const boost::chrono::seconds report_period_seconds_;
};

}  // namespace vocconv
#endif  // INCLUDE_PRE_CLEANING_AIR_QLY_REPORTER_H_
/** \} */  // end of addtogroup
