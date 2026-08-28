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

#ifndef INCLUDE_PARKING_CLIMATIZATION_LATEST_CLIMATE_STATUS_H_
#define INCLUDE_PARKING_CLIMATIZATION_LATEST_CLIMATE_STATUS_H_

#include <mutex>

#include "parking_climatization/prkg_clima_info.h"
#include "parking_climatization/prkg_clima_notif.h"

namespace vocconv {

class LatestClimateStatus {
 public:
    /**
     * Creates instance of LatestClimateStatus
     **/
    LatestClimateStatus();

    LatestClimateStatus(const LatestClimateStatus& other) = delete;
    LatestClimateStatus(LatestClimateStatus&& other) = delete;
    LatestClimateStatus& operator=(const LatestClimateStatus& other) = delete;
    LatestClimateStatus& operator=(LatestClimateStatus&& other) = delete;

    /**
     * Update stored PrkgClimaInfo if it differs from the currently stored one.
     * \param new_clima_info The climate status that shall be stored.
     * \return true if the stored PrkgClimaInfo was updated
     **/
    bool StoreInfo(const remote_common::PrkgClimaInfo& new_clima_info);

    /**
     * Get latest known climate status
     * \return latest known climate status
     **/
    remote_common::PrkgClimaInfo GetClimateInfo();

    /**
     * Update stored PrkgClimaNotif if it differs from the currently stored one.
     * \param new_clima_notif The climate notif status that shall be stored.
     * \return true if the stored PrkgClimaNotif was updated
     **/
    bool StoreNotif(const remote_common::PrkgClimaNotif& new_clima_notif);

    /**
     * Get latest known climate notif status
     * \return latest known climate notif status
     **/
    remote_common::PrkgClimaNotif GetClimateNotif();


#ifndef UNIT_TESTS

 private:
#endif

    remote_common::PrkgClimaInfo climate_status_;
    remote_common::PrkgClimaNotif climate_notif_;
    std::mutex access_mutex_;
};

}  // namespace vocconv
#endif  // INCLUDE_PARKING_CLIMATIZATION_LATEST_CLIMATE_STATUS_H_
/** \} */  // end of addtogroup
