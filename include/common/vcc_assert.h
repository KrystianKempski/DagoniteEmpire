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

#ifndef INCLUDE_COMMON_VCC_ASSERT_H_
#define INCLUDE_COMMON_VCC_ASSERT_H_

#include <cassert>

#if defined(NDEBUG) || defined(UNIT_TESTS)
#define vcc_assert(...)
#else
#define vcc_assert(x) assert(x)
#endif

#endif  // INCLUDE_COMMON_VCC_ASSERT_H_
/** \} */  // end of addtogroup
