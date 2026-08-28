/*
 * Copyright (C) 2026 - Volvo Car Corporation
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

#ifndef INCLUDE_COMMON_IUSAGE_MODE_CACHE_H_
#define INCLUDE_COMMON_IUSAGE_MODE_CACHE_H_

#include <boost/optional/optional.hpp>

#include "vc_message_payloads.hpp"

namespace vocconv {

enum class UsageModeTransition : uint8_t {
    kUnchanged = 0,  // No change
    kInitial,        // No initial value, first time setting usage mode
    kFromDriving,    // Transition from driving to other usage mode
    kToDriving,      // Transition from any usage mode to driving
    kOther,          // All other transitions that aren't interesting
};

/**
 * \brief Read-only interface of the cache.
 *
 * Use this for classes that are only interested in knowing the usage mode, but do not need to change it.
 *
 * \note: This is important because some classes may be interested in the "transition" between usage mode,
 *        which is returned by SetUsageMode(...) and the transition will only happen on change.
 */
class IUsageModeCacheReader {
 public:
    IUsageModeCacheReader() = default;
    virtual ~IUsageModeCacheReader() = default;
    IUsageModeCacheReader(const IUsageModeCacheReader&) = delete;
    IUsageModeCacheReader(IUsageModeCacheReader&&) = delete;
    IUsageModeCacheReader& operator=(const IUsageModeCacheReader&) = delete;
    IUsageModeCacheReader& operator=(IUsageModeCacheReader&&) = delete;

    virtual boost::optional<vc::CarUsageModeState> GetUsageMode() const = 0;
};

/**
 * \brief Read-write version of the cache.
 */
class IUsageModeCache : public IUsageModeCacheReader {
 public:
    IUsageModeCache() = default;
    ~IUsageModeCache() override = default;
    IUsageModeCache(const IUsageModeCache&) = delete;
    IUsageModeCache(IUsageModeCache&&) = delete;
    IUsageModeCache& operator=(const IUsageModeCache&) = delete;
    IUsageModeCache& operator=(IUsageModeCache&&) = delete;

    virtual UsageModeTransition SetUsageMode(vc::CarUsageModeState usage_mode) = 0;
};

}  // namespace vocconv
#endif  // INCLUDE_COMMON_IUSAGE_MODE_CACHE_H_

/** \} */  // end of addtogroup
