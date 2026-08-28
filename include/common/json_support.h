/*
 * Copyright (C) 2024 - Volvo Car Corporation
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

#ifndef INCLUDE_COMMON_JSON_SUPPORT_H_
#define INCLUDE_COMMON_JSON_SUPPORT_H_

#include <string>

#include "json/value.h"

namespace vocconv {

/**
 * \brief Get the JSON value with the name of `key` from a JSON object.
 *
 * \param json_root The JSON object to search in.
 * \param key The name of the JSON object to look for.
 *
 * \throws std::runtime_error if the key does not exist.
 * \return const Json::Value& the JSON object with the name of the key.
 */
inline const Json::Value& GetJsonValue(const Json::Value& json_root, const char* key) {
    const Json::Value& value = json_root[key];
    if (value == Json::Value::nullRef) {
        std::string exception_message{"Key '"};
        exception_message += key;
        exception_message += "' does not exist";
        throw std::runtime_error(exception_message);
    }
    return value;
}

}  // namespace vocconv
#endif  // INCLUDE_COMMON_JSON_SUPPORT_H_
/** \} */  // end of addtogroup
