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

#ifndef INCLUDE_PARKING_CLIMATIZATION_IHU_PROTOBUF_CODEC_H_
#define INCLUDE_PARKING_CLIMATIZATION_IHU_PROTOBUF_CODEC_H_

#include <memory>
#include <vector>

#include "parking_climatization/climatization_timer_list.h"

namespace vocconv {
namespace protobuf {

/**
 * \brief Encodes the ClimatizationTimerList to a protobuf message.
 * \param[in] timer_list The timer list to encode.
 * \return The encoded protobuf message.
 */
std::shared_ptr<std::vector<uint8_t>> EncodeClimatizationTimerList(const ClimatizationTimerList& timer_list);

/**
 * \brief Decodes the ClimatizationTimerList from a protobuf message.
 * \param[in] payload The encoded protobuf message.
 * \return The decoded timer list.
 */
std::shared_ptr<vocconv::ClimatizationTimerList> DecodeTimerList(std::shared_ptr<std::vector<uint8_t>> payload);

/**
 * \brief Encodes a SetParkingClimatizationTimerListResponse to a protobuf message.
 * \param[in] status_code The status code to include in the response.
 * \return The encoded protobuf message.
 */
std::shared_ptr<std::vector<uint8_t>> EncodeTimerListResponse(uint32_t status_code);

}  // namespace protobuf

}  // namespace vocconv
#endif  // INCLUDE_PARKING_CLIMATIZATION_IHU_PROTOBUF_CODEC_H_
/** \} */  // end of addtogroup
