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

#ifndef INCLUDE_FEATURES_ENGINE_REMOTE_START_H_
#define INCLUDE_FEATURES_ENGINE_REMOTE_START_H_

#include <memory>

<<<<<<< PATCH SET (c10e4e106a4b6b86331fd90bfd6de448bbc5407c Improve VocConv robot test coverage)
#include <boost/chrono.hpp>

#include "feature_authorization_service_proxy/feature_authorization_service_proxy.h"
=======
#include "feature_authorization_service_proxy/ifeature_authorization_service_proxy.h"
>>>>>>> BASE      (cdbd4caabcfa7005f07be5210ab50bcf06c35bff Move slow ChargeController tests from smoke to regression)
#include "app_framework/features/feature.h"
#include "engine_remote_start/iengine_remote_start_state.h"
#include "local_config_reader/configs.h"

namespace vocconv {

constexpr char const* kEngineRemoteStartFeatureName = "EngineRemoteStart";

#ifdef UNIT_TESTS
enum class HandleSignalResult : uint8_t {
    ReceivedSignalIsNull = 0,
    ReceivedSignalIgnored,
    TransactionForEngineRemoteStartRequestCreated,
    TransactionForEngineRemoteStartStatusCreated,
    TransactionForEngineRemoteStartStatusRequestCreated,
    ReceivedSignalBroadcasted,
    NoResult,
};
#endif

class EngineRemoteStart : public fsm::Feature {
 public:
    EngineRemoteStart(
            std::shared_ptr<IEngineRemoteStartState> ers_state,
            std::shared_ptr<fas::IFeatureAuthorizationServiceProxy> feature_authorization_service,
            const local_config::engine_remote_start_tcam1::Config& ers_config);
    ~EngineRemoteStart();

    EngineRemoteStart(const EngineRemoteStart& other) = delete;
    EngineRemoteStart(EngineRemoteStart&& other) = delete;
    EngineRemoteStart& operator=(const EngineRemoteStart& other) = delete;
    EngineRemoteStart& operator=(EngineRemoteStart&& other) = delete;

    /**
     * \brief Handle a signal.
     * \param[in] signal The signal to handle.
     * \return None.
     **/
    void HandleSignal(std::shared_ptr<fsm::Signal> signal) override;

#ifdef ENABLE_SIGNAL_INJECTION
    /**
     * \brief Override the EngineRemoteStartRequestTransaction timeout (test hook).
     * \param[in] transaction_timeout Timeout applied to subsequently created transactions.
     **/
    void SetEngineRemoteStartTransactionTimeout(const boost::chrono::seconds& transaction_timeout);

    template <typename T>
    bool ActiveTransactionExists() const;

    bool ActiveEngineRemoteStartTransactionExists() const;
#endif

#ifdef UNIT_TESTS
    HandleSignalResult signal_recv_result_ = HandleSignalResult::NoResult;
#endif

 private:
    void HandleEngineRemoteStartRequestSignal(const std::shared_ptr<fsm::Signal>& signal);
    void CreateEngineRemoteStartRequestTransaction(const std::shared_ptr<fsm::Signal>& signal);
    void HandleVfcActivationSignal(const std::shared_ptr<fsm::Signal>& signal);
    void HandleEngineRemoteStartStsSignal(const std::shared_ptr<fsm::Signal>& signal);
    void HandleEngineRemoteStartStatusRequestSignal(const std::shared_ptr<fsm::Signal>& signal);
    void HandleUpdateAllSignal(const std::shared_ptr<fsm::Signal>& signal);

    std::shared_ptr<IEngineRemoteStartState> ers_state_;
    std::shared_ptr<fas::IFeatureAuthorizationServiceProxy> feature_authorization_service_;
    local_config::engine_remote_start_tcam1::Config engine_remote_start_config_;
};

}  // namespace vocconv
#endif  // INCLUDE_FEATURES_ENGINE_REMOTE_START_H_
/** \} */  // end of addtogroup
