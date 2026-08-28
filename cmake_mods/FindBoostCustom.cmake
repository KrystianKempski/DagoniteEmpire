# Boost required by fsm vsomeip
# Need to explicitly give path to get right libraries
# Since it is not possible to use find_package, as out cmake version is
# incompatible with boost version
add_library(boost_system SHARED IMPORTED)
set_property(TARGET boost_system PROPERTY IMPORTED_LOCATION $ENV{SYSROOT}/usr/lib/boost/libboost_system.a)

add_library(boost_thread SHARED IMPORTED)
set_property(TARGET boost_thread PROPERTY IMPORTED_LOCATION $ENV{SYSROOT}/usr/lib/boost/libboost_thread.a)

add_library(boost_filesystem SHARED IMPORTED)
set_property(TARGET boost_filesystem PROPERTY IMPORTED_LOCATION $ENV{SYSROOT}/usr/lib/boost/libboost_filesystem.a)

add_library(boost_log SHARED IMPORTED)
set_property(TARGET boost_log PROPERTY IMPORTED_LOCATION $ENV{SYSROOT}/usr/lib/boost/libboost_log.a)

add_library(boost_date_time SHARED IMPORTED)
set_property(TARGET boost_date_time PROPERTY IMPORTED_LOCATION $ENV{SYSROOT}/usr/lib/boost/libboost_date_time.a)

add_library(boost_atomic SHARED IMPORTED)
set_property(TARGET boost_atomic PROPERTY IMPORTED_LOCATION $ENV{SYSROOT}/usr/lib/boost/libboost_atomic.a)

add_library(boost_chrono SHARED IMPORTED)
set_property(TARGET boost_chrono PROPERTY IMPORTED_LOCATION $ENV{SYSROOT}/usr/lib/boost/libboost_chrono.a)

set(BOOST_LIBS
    boost_log
    boost_filesystem
    boost_system
    boost_thread
    boost_date_time
    boost_atomic
    boost_chrono
)
