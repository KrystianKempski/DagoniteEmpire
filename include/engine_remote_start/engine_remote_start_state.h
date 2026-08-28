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

#ifndef INCLUDE_ENGINE_REMOTE_START_ENGINE_REMOTE_START_STATE_H_
#define INCLUDE_ENGINE_REMOTE_START_ENGINE_REMOTE_START_STATE_H_

#include <mutex>

#include "engine_remote_start/iengine_remote_start_state.h"

namespace vocconv {

/**
 * \class EngineRemoteStartState
 * Support for temporary storage of Engine Remote Start state information.
 * This to be able to supply the information on request from the mobile application, Mapp.
 */
class EngineRemoteStartState : public IEngineRemoteStartState {
 public:
    EngineRemoteStartState();

    EngineRemoteStartState(const EngineRemoteStartState& other) = delete;
    EngineRemoteStartState(EngineRemoteStartState&& other) = delete;
    EngineRemoteStartState& operator=(const EngineRemoteStartState& other) = delete;
    EngineRemoteStartState& operator=(EngineRemoteStartState&& other) = delete;

    /**
     * Stores the Engine Remote Start state, (status and result).
     * \param state The state, (status and result), that will be stored.
     **/
    void SetLatestErsStatus(const status_ErsStatusType status) override;

    /**
     * Get the stored Engine Remote Start state (status and result).
     * \return Returns the stored state, (status and result).
     **/
    status_ErsStatusType GetLatestErsStatus() override;

 private:
    status_ErsStatusType status_;
    std::mutex ers_state_mutex_;
};

}  // namespace vocconv
#endif  // INCLUDE_ENGINE_REMOTE_START_ENGINE_REMOTE_START_STATE_H_
/** \} */  // end of addtogroup
