/*
 * Copyright (C) 2020 - Volvo Car Corporation
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

#ifndef INCLUDE_COMMON_REQUESTOR_H_
#define INCLUDE_COMMON_REQUESTOR_H_

namespace vocconv {

enum class Requestor {
    kHmi,     // IHU
    kRemote,  // MAPP
    kLocal    // TCAM self
    };

}  // namespace vocconv
#endif  // INCLUDE_COMMON_REQUESTOR_H_
/** \} */  // end of addtogroup
