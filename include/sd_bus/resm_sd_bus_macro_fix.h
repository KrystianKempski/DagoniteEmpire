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

/**
 * \file    resm_sd_bus_macro_fix.hpp
 * \brief   Resource Manager sd-bus macro vtable fix for C++ compilers.
 */


#ifndef INCLUDE_SD_BUS_RESM_SD_BUS_MACRO_FIX_H_
#define INCLUDE_SD_BUS_RESM_SD_BUS_MACRO_FIX_H_

extern "C" {

sd_bus_vtable sd_bus_vtable_start(int flags);
sd_bus_vtable sd_bus_method(const char * member, const char * signature, const char * result,
                            sd_bus_message_handler_t handler, int flags);
sd_bus_vtable sd_bus_signal(const char * member, const char * signature, int flags);
sd_bus_vtable sd_bus_property(const char * member, const char * signature, sd_bus_property_get_t get, int offset,
                              int flags);
sd_bus_vtable sd_bus_vtable_end();

}

#endif  // INCLUDE_SD_BUS_RESM_SD_BUS_MACRO_FIX_H_
