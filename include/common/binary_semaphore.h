/*
 * Copyright (C) 2019 - Volvo Car Corporation
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

#ifndef INCLUDE_COMMON_BINARY_SEMAPHORE_H_
#define INCLUDE_COMMON_BINARY_SEMAPHORE_H_

#include <boost/chrono.hpp>
#include <boost/thread/condition_variable.hpp>
#include <boost/thread/mutex.hpp>
#include <atomic>

namespace vocconv {

class BinarySemaphore {
 public:
    explicit BinarySemaphore(bool signaled = false);

    void Release();
    void Acquire(boost::chrono::seconds timeout_secs);

 private:
    std::atomic<bool> signaled_;
    boost::mutex mutex_;
    boost::condition_variable cond_var_;
};

}  // namespace vocconv
#endif  // INCLUDE_COMMON_BINARY_SEMAPHORE_H_
/** \} */  // end of addtogroup
