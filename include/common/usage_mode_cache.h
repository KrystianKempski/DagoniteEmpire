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

#ifndef INCLUDE_COMMON_USAGE_MODE_CACHE_H_
#define INCLUDE_COMMON_USAGE_MODE_CACHE_H_

#include <mutex>

#include "common/iusage_mode_cache.h"

namespace vocconv {

class UsageModeCache : public IUsageModeCache {
 public:
    UsageModeCache() = default;
    ~UsageModeCache() override = default;

    UsageModeCache(const UsageModeCache& other) = delete;
    UsageModeCache(UsageModeCache&& other) = delete;
    UsageModeCache& operator=(const UsageModeCache& other) = delete;
    UsageModeCache& operator=(UsageModeCache&& other) = delete;

    /**
     * \note: If you are interested in the transition between usage modes after setting it, you cannot share this cache
     * with another class that also sets usage mode in the cache, as the transition return codes only happen on change.
     */
    UsageModeTransition SetUsageMode(vc::CarUsageModeState usage_mode) override;
    boost::optional<vc::CarUsageModeState> GetUsageMode() const override;

 private:
    boost::optional<vc::CarUsageModeState> usage_mode_{};
    mutable std::mutex mutex_{};
};

}  // namespace vocconv
#endif  // INCLUDE_COMMON_USAGE_MODE_CACHE_H_
/** \} */  // end of addtogroup

