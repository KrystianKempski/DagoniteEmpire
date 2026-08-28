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

#ifndef INCLUDE_ENGINE_REMOTE_START_IENGINE_REMOTE_START_STATE_H_
#define INCLUDE_ENGINE_REMOTE_START_IENGINE_REMOTE_START_STATE_H_

#include "signals/engine_remote_start_status_update.h"

namespace vocconv {

class IEngineRemoteStartState {
 public:
    virtual ~IEngineRemoteStartState() {}

    IEngineRemoteStartState(const IEngineRemoteStartState& other) = delete;
    IEngineRemoteStartState(IEngineRemoteStartState&& other) = delete;
    IEngineRemoteStartState& operator=(const IEngineRemoteStartState& other) = delete;
    IEngineRemoteStartState& operator=(IEngineRemoteStartState&& other) = delete;

    virtual void SetLatestErsStatus(const status_ErsStatusType status) = 0;
    virtual status_ErsStatusType GetLatestErsStatus() = 0;
 protected:
    IEngineRemoteStartState() = default;
};

}  // namespace vocconv
#endif  // INCLUDE_ENGINE_REMOTE_START_IENGINE_REMOTE_START_STATE_H_
/** \} */  // end of addtogroup
