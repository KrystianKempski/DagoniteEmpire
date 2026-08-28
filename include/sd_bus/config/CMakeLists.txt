# Copyright (C) 2019 - Volvo Car Corporation
#
# All Rights Reserved
#
# LEGAL NOTICE:  All information (including intellectual and technical concepts) contained herein is,
# and remains, the property of Volvo Car Corporation.
# This information is protected by copyright and may be covered by patents or patent applications
# and include trade secrets.
# Dissemination of this information or reproduction of this material is strictly forbidden unless
# prior written permission is obtained from Volvo Car Corporation.

message("Entering ${CMAKE_CURRENT_SOURCE_DIR} dir.")

install(FILES com.volvocars.vocconv.service.conf DESTINATION /etc/dbus-1/system.d)
