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

cmake_minimum_required(VERSION 3.5)

find_package(PkgConfig)

add_definitions(-D__ARM_PCS_VFP)
add_definitions(-DY2038_PROBLEM_WORKAROUND) # Workaround for Y2038 problem in TCAM1/TCAM1.1

# System
pkg_check_modules(MODULE_LIBSYSTEMD REQUIRED libsystemd)

# LocalConfig
pkg_check_modules(MODULE_LCFG REQUIRED local_config)

#uuid
pkg_check_modules(MODULE_UUID REQUIRED uuid)

# FSM
pkg_check_modules(MODULE_FSM_APP_FW REQUIRED libfsm_appframework)
pkg_check_modules(MODULE_FSM_CONFIG REQUIRED libfsm_fscfg)
pkg_check_modules(MODULE_MARBEN_RUNTIME REQUIRED marben_runtime)
pkg_check_modules(MODULE_MARBEN_PARSERS REQUIRED marben_parsers)
pkg_check_modules(MODULE_USER_MANAGER REQUIRED user_manager)

# VLog
pkg_check_modules(MODULE_VLOG REQUIRED vlog2)

#SSL
pkg_check_modules(OPENSSL_PKG REQUIRED openssl)

pkg_check_modules(JSONCPP_PKG REQUIRED jsoncpp)

pkg_check_modules(DATA_HANDLER_PKG REQUIRED datahandler)

# Feature Authorization Service (FAS)
pkg_check_modules(LIBFAS REQUIRED feature_authorization_service_proxy)

# VehicleComm - for VuC timers
pkg_check_modules(MODULE_VC_PROXY REQUIRED vc_proxy)

# tcam common
pkg_check_modules(TCAM_COMMON_TYPES_PKG REQUIRED tcam_common_types)
pkg_check_modules(TCAM_COMMON_INIT_PKG REQUIRED tcam_common_init)

project(vocconv)
set(CMAKE_VERBOSE_MAKEFILE OFF)
set(CMAKE_CXX_STANDARD 14)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_POSITION_INDEPENDENT_CODE ON)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)
set(ICUP_PLATFORM ON) # The macro is used in the code to conditionally compile code for ICUP platform in RemoteCommon.

# Set to ON to log GPS position. DISALLOWED in production builds.
set(LOG_GPS_POSITION OFF)

if (NOT "$ENV{BUILD_TYPE}" STREQUAL "prod")
    set(LOG_GPS_POSITION ON)
else()
    set(LOG_GPS_POSITION OFF)
endif()

if(LOG_GPS_POSITION)
    add_definitions(-DLOG_GPS_POSITION)
endif()

string(CONCAT EXTRA_CXX_FLAGS
  "-Werror "
  "-Wno-error=deprecated-declarations "
  "-Wall "
  "-Wextra "
  "-Wshadow "
  "-Wduplicated-cond "
  "-Wuseless-cast "
  "-Wfloat-conversion "
  "-Wsizeof-array-argument "
  "-Wlogical-op "
  "-Wno-packed-bitfield-compat "
  "-rdynamic "
)

set(CMAKE_CXX_FLAGS
  "${CMAKE_CXX_FLAGS} ${EXTRA_CXX_FLAGS}"
)

set(ROOT_PATH ${CMAKE_SOURCE_DIR}/../../..)
set(SYSTEMD_CONFIGURATIONS_FILES_DIR "/lib/systemd/system/")

file(GLOB_RECURSE ALL_SOURCE_FILES
  ${CMAKE_SOURCE_DIR}/src/*.cc
  ${CMAKE_SOURCE_DIR}/include/*.h
  ${CMAKE_SOURCE_DIR}/test/sd_bus/*.h
  ${CMAKE_SOURCE_DIR}/test/sd_bus/*.cc
)

add_custom_command(
  OUTPUT ${CMAKE_CURRENT_BINARY_DIR}/cpplint_output
  DEPENDS ${ALL_SOURCE_FILES}
  COMMAND ${ROOT_PATH}/tools/cpplint/cpplint.py --extensions=c,cc,cpp ${ALL_SOURCE_FILES}
  COMMAND ${CMAKE_COMMAND} -E touch ${CMAKE_CURRENT_BINARY_DIR}/cpplint_output
)
add_custom_target(
  vocconv_cpplint
  DEPENDS ${CMAKE_CURRENT_BINARY_DIR}/cpplint_output
)

add_custom_command(
  OUTPUT ${CMAKE_CURRENT_BINARY_DIR}/cppcheck_output
  DEPENDS ${ALL_SOURCE_FILES}
  COMMAND
    ${ROOT_PATH}/tools/cppcheck/cppcheck
      --language=c++
      --std=c++14
      --verbose
      --enable=all
      --suppress=missingInclude
      --output-file=${CMAKE_BINARY_DIR}/cppcheck_output.log
      -I ${CMAKE_SOURCE_DIR}/include/
      ${ALL_SOURCE_FILES}
  COMMAND ${CMAKE_COMMAND} -E touch ${CMAKE_CURRENT_BINARY_DIR}/cppcheck_output
)
add_custom_target(
  vocconv_cppcheck
  DEPENDS ${CMAKE_CURRENT_BINARY_DIR}/cppcheck_output
)

set(CLANG_TIDY_PY_SCRIPT "${ROOT_PATH}/tools/clang-tools-extra/clang-tidy/tool/run-clang-tidy.py")

# The syntax is "*,-dont_want_check_rule.*"
set(CLANG_TIDY_CHECK_STR "-*,google-*,modernize-*,cert-*,cppcoreguidelines-*")
set(CLANG_TIDY_CHECK_STR_ALL "-*,performance-*,google-*")

# This directive was needed when we run within Clang env.
# The built-in library within Clang don't have software implemented FP stub.
set(CLANG_TIDY_EXTRA_ARG "-D__ARM_PCS_VFP")

add_custom_target(
  clangtidy
  COMMAND ${CLANG_TIDY_PY_SCRIPT} -extra-arg=${CLANG_TIDY_EXTRA_ARG} -checks=${CLANG_TIDY_CHECK_STR_ALL} -p ${CMAKE_BINARY_DIR} | tee ${CMAKE_CURRENT_BINARY_DIR}/clangtidy_output
  COMMENT "Running clang-tidy"
)

# Codechecker we do same checks as above except with this you get nice html output
set(CODECHK_DIR "${ROOT_PATH}/tools/codechecker/")
add_custom_target(
  codechecker COMMAND /bin/sh ${CMAKE_SOURCE_DIR}/lintconfig/codechecker_pre.sh ${CODECHK_DIR} ${CMAKE_BINARY_DIR}
  COMMENT "Running codecheker"
)

link_directories(
  ${MODULE_TP_LIBRARY_DIRS}
)

set(ROOT_PATH ${CMAKE_CURRENT_SOURCE_DIR}/../../..)
set(APPS_PATH ${ROOT_PATH}/telematics/apps)
set(VOC_CONV_PATH ${APPS_PATH}/VocConv)
set(REMOTE_COMMON_PATH ${APPS_PATH}/RemoteCommon)

# Build RemoteCommon as a subdirectory to create the static library
add_subdirectory(${REMOTE_COMMON_PATH} ${CMAKE_BINARY_DIR}/remote_common)

list(APPEND CMAKE_MODULE_PATH "${CMAKE_SOURCE_DIR}/cmake_mods/")
include(GetGitRevisionDescription)
get_git_head_revision(GIT_REFSPEC GIT_SHA1)
git_local_changes(GIT_STATUS)

string(TIMESTAMP BUILD_TIME "%Y-%m-%d %H:%M:%S")

message(INFO "BUILD_TIME = \"${BUILD_TIME}\"")
message(INFO "GIT_REFSPEC = \"${GIT_REFSPEC}\", GIT_SHA1 = \"${GIT_SHA1}\", GIT_STATUS = \"${GIT_STATUS}\"")

set(PROJECT_INCLUDE_DIR ${CMAKE_SOURCE_DIR}/include)

configure_file (
  "${PROJECT_INCLUDE_DIR}/common/version.h.in"
  "${CMAKE_CURRENT_BINARY_DIR}/common/version.h"
)

add_subdirectory(local_config/lib)

add_subdirectory(src)

install(FILES config/com.volvocars.vocconv.conf DESTINATION /etc/dbus-1/system.d)

if (NOT "$ENV{BUILD_TYPE}" STREQUAL "prod")
  add_subdirectory(test/sd_bus)
endif()
