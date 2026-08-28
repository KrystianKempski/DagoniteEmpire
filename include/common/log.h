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

#ifndef INCLUDE_COMMON_LOG_H_
#define INCLUDE_COMMON_LOG_H_

#include "logging/import_context.h"
#include "logging/register_context.h"

VLOG_IMPORT_MODULE_CONTEXT(CHARGE_CONTROL_LOG_CTX);
VLOG_IMPORT_MODULE_CONTEXT(PARK_CLIM_LOG_CTX);
VLOG_IMPORT_MODULE_CONTEXT(PRECLEANING_LOG_CTX);
VLOG_IMPORT_MODULE_CONTEXT(SUBSYSTEM_LOG_CTX);
VLOG_IMPORT_MODULE_CONTEXT(OTATIMER_LOG_CTX);
VLOG_IMPORT_MODULE_CONTEXT(GENERAL_LOG_CTX);
VLOG_IMPORT_MODULE_CONTEXT(ENGINE_LOG_CTX);
VLOG_IMPORT_MODULE_CONTEXT(DIAG_LOG_CTX);

namespace vocconv {
constexpr char const* kAppName = "vocconv";
constexpr char const* kDltAppId = "VOCC";
constexpr char const* kDltDescription = "vocconv contains following features: "
        "parking climatization feature";

class RegisterContexts {
 public:
    RegisterContexts() {
        VLOG_REGISTER_MODULE_CONTEXT(CHARGE_CONTROL_LOG_CTX, "CHAR" , "charge control");
        VLOG_REGISTER_MODULE_CONTEXT(PARK_CLIM_LOG_CTX, "CLIM", "parking climatization");
        VLOG_REGISTER_MODULE_CONTEXT(ENGINE_LOG_CTX, "ERS-", "engine remote start");
        VLOG_REGISTER_MODULE_CONTEXT(PRECLEANING_LOG_CTX, "PREC", "precleaning");
        VLOG_REGISTER_MODULE_CONTEXT(SUBSYSTEM_LOG_CTX, "SUBS", "subsystem");
        VLOG_REGISTER_MODULE_CONTEXT(OTATIMER_LOG_CTX, "OTA-", "OTA");
        VLOG_REGISTER_MODULE_CONTEXT(GENERAL_LOG_CTX, "GEN-", "general");
        VLOG_REGISTER_MODULE_CONTEXT(DIAG_LOG_CTX, "DIAG", "diagnostic");
    }
    ~RegisterContexts() {
        VLOG_DEREGISTER_MODULE_CONTEXT(CHARGE_CONTROL_LOG_CTX);
        VLOG_DEREGISTER_MODULE_CONTEXT(PARK_CLIM_LOG_CTX);
        VLOG_DEREGISTER_MODULE_CONTEXT(ENGINE_LOG_CTX);
        VLOG_DEREGISTER_MODULE_CONTEXT(PRECLEANING_LOG_CTX);
        VLOG_DEREGISTER_MODULE_CONTEXT(SUBSYSTEM_LOG_CTX);
        VLOG_DEREGISTER_MODULE_CONTEXT(OTATIMER_LOG_CTX);
        VLOG_DEREGISTER_MODULE_CONTEXT(GENERAL_LOG_CTX);
        VLOG_DEREGISTER_MODULE_CONTEXT(DIAG_LOG_CTX);
    }

 private:
        remote_common::RegisterContexts remote_common_ctx_;
};
}  // namespace vocconv

#endif  // INCLUDE_COMMON_LOG_H_
/** \} */  // end of addtogroup
