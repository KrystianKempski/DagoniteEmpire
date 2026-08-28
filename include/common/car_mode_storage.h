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

#ifndef INCLUDE_COMMON_CAR_MODE_STORAGE_H_
#define INCLUDE_COMMON_CAR_MODE_STORAGE_H_

#include <mutex>

#include "transactions/car_mode_support.h"

namespace vocconv {

class CarModeStorage {
 public:
    CarModeStorage();
    CarModeStorage(const CarModeStorage& other) = delete;
    CarModeStorage(CarModeStorage&& other) = delete;
    CarModeStorage& operator=(const CarModeStorage& other) = delete;
    CarModeStorage& operator=(CarModeStorage&& other) = delete;

    /**
     * \brief Stores the value of CarMode in storage.
     * \returns true if the stored value changed, otherwise false.
     */
    bool SetCarMode(car_mode_support::CarMode car_mode);
    car_mode_support::CarMode GetCarMode();
    bool IsCarModeSet();

 private:
    car_mode_support::CarMode car_mode_;
    bool car_mode_has_been_set_;
    std::mutex car_mode_mutex_;
};

}  // namespace vocconv
#endif  // INCLUDE_COMMON_CAR_MODE_STORAGE_H_
/** \} */  // end of addtogroup
