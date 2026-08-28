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

#ifndef INCLUDE_PRE_CLEANING_ISESSION_H_
#define INCLUDE_PRE_CLEANING_ISESSION_H_

#include <memory>

#include "app_framework/iiplm_controller.h"
#include "local_config_reader/configs.h"

namespace vocconv {
namespace pre_cleaning {

class PreCleaningStatus;

/**
 * Interface for controlling a PreCleaning Session
 *
 * The intention is that the feature receives pre cleaning status
 * information from VGM and then uses this to Start or Stop the session.
 * The session is responsible for the actions to be performed during a running
 * PreCleaning Session.
 */
class ISession {
 public:
    virtual void Start() = 0;
    virtual void Stop() = 0;
    virtual ~ISession() = default;
};

/**
 * Factory method to create a PreCleaningSession instance.
 *
 * @param iplm_controller pointer to IPLM controller interface
 * @param status pre-cleaning status cache to report from
 */
std::shared_ptr<ISession> CreatePreCleaningSession(std::shared_ptr<fsm::IIPLMController> iplm_controller,
                                                   std::shared_ptr<PreCleaningStatus> status,
                                                   const local_config::pre_cleaning_tcam1::Config& pre_cleaning_config);

}  // namespace pre_cleaning
}  // namespace vocconv
#endif  // INCLUDE_PRE_CLEANING_ISESSION_H_
/** \} */  // end of addtogroup
