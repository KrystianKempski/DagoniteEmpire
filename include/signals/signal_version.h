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

#ifndef INCLUDE_SIGNALS_SIGNAL_VERSION_H_
#define INCLUDE_SIGNALS_SIGNAL_VERSION_H_

#include <cstdint>

namespace vocconv {

constexpr std::uint32_t kSchemaVersion = 1;
constexpr std::uint32_t kPreliminaryVersion = 1;
constexpr std::uint32_t kSignalFlowVersion = 20;

}  // namespace vocconv
#endif  // INCLUDE_SIGNALS_SIGNAL_VERSION_H_
/** \} */  // end of addtogroup
