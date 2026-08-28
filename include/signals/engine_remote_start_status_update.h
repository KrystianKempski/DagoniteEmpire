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

#ifndef INCLUDE_SIGNALS_ENGINE_REMOTE_START_STATUS_UPDATE_H_
#define INCLUDE_SIGNALS_ENGINE_REMOTE_START_STATUS_UPDATE_H_

#include "app_framework/signals/protobuf_signal.h"
#include "signals/protobuf/messages/engineremotestart/ERSStatusUpdate.pb.h"

namespace vocconv {

constexpr auto kEngineRemoteStartStatusUpdateOid = "1.3.6.1.4.1.37916.3.6.20.0.1.1";
constexpr char kEngineRemoteStartStatusUpdateName[] = "EngineRemoteStartStatusUpdate";

/**
* \brief Used when sending message from TCAM to Mapp. An update is sent to the Mapp
* whenever we receive either one of the signals PrkgClimaInfoSts or PrkgClimaNotifSts
* from VGM. The user should either set the notification type or the info whos data is
* received from the signals mentioned above.
*/
class EngineRemoteStartStatusUpdate : public fsm::ProtobufSignal<remote_control_ERSStatusUpdate> {
 public:
    EngineRemoteStartStatusUpdate();
    EngineRemoteStartStatusUpdate(const EngineRemoteStartStatusUpdate&) = delete;
    EngineRemoteStartStatusUpdate(EngineRemoteStartStatusUpdate&&) = delete;
    EngineRemoteStartStatusUpdate& operator=(const EngineRemoteStartStatusUpdate&) = delete;
    EngineRemoteStartStatusUpdate& operator=(EngineRemoteStartStatusUpdate&&) = delete;
    ~EngineRemoteStartStatusUpdate() = default;

    /**
     * \brief Set the engine remote start status
     * \param[in] status_type status_ErsStatusType,
     * result_type status_ErsStartResultType,
     */
    void SetStatus(status_ErsStatusType status_type,
                   status_ErsStartResultType result_type);

    /**
     * \brief Set the engine remote start status
     * \param[in] status status_ErsStatus
     */
    void SetStatus(const status_ErsStatus& status);
};

}  // namespace vocconv
#endif  // INCLUDE_SIGNALS_ENGINE_REMOTE_START_STATUS_UPDATE_H_
/** \} */  // end of addtogroup
