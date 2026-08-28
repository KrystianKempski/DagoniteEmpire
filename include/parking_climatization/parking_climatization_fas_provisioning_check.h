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

#ifndef INCLUDE_PARKING_CLIMATIZATION_PARKING_CLIMATIZATION_FAS_PROVISIONING_CHECK_H_
#define INCLUDE_PARKING_CLIMATIZATION_PARKING_CLIMATIZATION_FAS_PROVISIONING_CHECK_H_

#include <memory>

#include "feature_authorization_service_proxy/ifeature_authorization_service_proxy.h"
#include "transactions/climatization_support.h"

namespace vocconv {
namespace parking_climate_fas_provisioning {

ClimatizationStatusCode IsParkingClimatizationDirectStartStopAllowed(
        const std::shared_ptr<fas::IFeatureAuthorizationServiceProxy>& feature_authorization_service);

ClimatizationStatusCode IsParkingClimatizationStatusUpdateAllowed(
        const std::shared_ptr<fas::IFeatureAuthorizationServiceProxy>& feature_authorization_service);

ClimatizationStatusCode IsClimatizationTimersOperationAllowed(
        const std::shared_ptr<fas::IFeatureAuthorizationServiceProxy>& feature_authorization_service);

ClimatizationStatusCode FasReturnCodeToClimatizationStatusCode(const fas::FasReturnCode fas_return_code);

}  // namespace parking_climate_fas_provisioning
}  // namespace vocconv
#endif     // INCLUDE_PARKING_CLIMATIZATION_PARKING_CLIMATIZATION_FAS_PROVISIONING_CHECK_H_
/** \} */  // end of addtogroup
