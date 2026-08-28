/*
 * Copyright (C) 2024 - Volvo Car Corporation
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

#ifndef INCLUDE_FEATURES_UTIL_FEATURE_H_
#define INCLUDE_FEATURES_UTIL_FEATURE_H_

#include <boost/chrono/chrono.hpp>

#include <string>
#include <memory>

#include "app_framework/features/feature.h"
#include "app_framework/signals/signal.h"

#include "battery_charge/icharge_controller.h"
#include "common/iposition_oracle.h"
#include "common/iusage_mode_cache.h"
#include "local_config_reader/configs.h"
#include "transactions/update_position_transaction.h"
#include "utilities/icar_time_offset_oracle.h"
#include "utilities/itime_provider.h"

namespace vocconv {

#ifdef UNIT_TESTS
enum class CheckPoint : uint8_t {
    Idle = 0,
    ReceivedSignalIsNull,
    EndOfTripSignalIsPassedToPositionTransaction,
    PositionIsPassedToPositionTransaction,
    SetCarTimeOffsetRequestHandled,
    EnteredUsageModeDrivingIsPassedToPositionTransaction,
    LeftUsageModeDrivingIsPassedToPositionTransaction,
    LocationProviderCallbackExecuted,
};
#endif

class UtilFeature : public fsm::Feature {
 public:
    explicit UtilFeature(std::shared_ptr<IPositionOracle> position_oracle,
                         std::shared_ptr<IChargeController> charge_controller,
                         std::shared_ptr<remote_common::ICarTimeOffsetOracle> car_time_offset_oracle,
                         std::shared_ptr<remote_common::ITimeProvider> time_provider,
                         std::shared_ptr<IUsageModeCache> usage_mode_cache,
                         const local_config::vocconv_util_tcam1::Config& config);
    ~UtilFeature();

    UtilFeature(const UtilFeature& other) = delete;
    UtilFeature(UtilFeature&& other) = delete;
    UtilFeature& operator=(const UtilFeature& other) = delete;
    UtilFeature& operator=(UtilFeature&& other) = delete;

    /**
     * \brief Handles received signal
     * \param signal The signal to handle.
     */
    void HandleSignal(std::shared_ptr<fsm::Signal> signal) override;

#ifdef UNIT_TESTS
    CheckPoint test_checkpoint_;
#else

 private:
#endif
    /**
     * This class cannot share location provider with others, so it cannot be injected via the constructor. This method
     * is needed to inject location provider mock in unit tests.
     */
    void CreateUpdatePositionTransaction(
            std::shared_ptr<remote_common::location_service::ILocationProvider> location_provider,
            const local_config::vocconv_util_tcam1::Config& config);

    void CreateSetCarTimeOffsetTransaction(const std::shared_ptr<fsm::Signal>& signal);
    void HandleUsageModeSignal(const std::shared_ptr<fsm::Signal>& signal);
    void LocationProviderCallback(const remote_common::location_service::ILocationProvider::LocationData& location);
    void HandleUpdateAllSignal(const std::shared_ptr<fsm::Signal>& signal);

    const std::string kFeatureName{"UtilFeature"};

    std::shared_ptr<IPositionOracle> position_oracle_;
    std::shared_ptr<IChargeController> charge_controller_;
    std::shared_ptr<remote_common::ICarTimeOffsetOracle> car_time_offset_oracle_;
    std::shared_ptr<remote_common::ITimeProvider> time_provider_;
    std::shared_ptr<IUsageModeCache> usage_mode_cache_;
    std::shared_ptr<UpdatePositionTransaction> update_position_transaction_{};
    const local_config::vocconv_util_tcam1::Config config_;
};

}  // namespace vocconv
#endif  // INCLUDE_FEATURES_UTIL_FEATURE_H_
/** \} */  // end of addtogroup
