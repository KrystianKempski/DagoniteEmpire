#!/bin/bash

setup_env () {
  tcam_src_path=$(cd `pwd`/../../../; pwd)
  export PATH=${PATH}:${tcam_src_path}/out/ccache:\
${tcam_src_path}/hardware/conti/release/fs/host/sbin:\
${tcam_src_path}/hardware/conti/release/fs/host/usr/bin:\
${tcam_src_path}/hardware/conti/release/fs/host/bin:\
${tcam_src_path}/hardware/conti/release/fs/devel/usr/bin:\
${tcam_src_path}/hardware/conti/release-toolchain/fs/devel/usr/bin:\
${tcam_src_path}/build/ninja:\
${tcam_src_path}/tools/bitbake/bin

  echo $PATH

  BUILD_TYPE="dev"
  export BUILD_TYPE
  export BB_ENV_EXTRAWHITE="$BB_ENV_EXTRAWHITE BUILD_TYPE"
  export BBPATH=${tcam_src_path}/build/bitshake/build/
}

setup_build () {
  sed -i 's/vocstatus//g' ${tcam_src_path}/build/bitshake/meta-vcc-tcam1/recipes-telematics/vocconv/vocconv.bb
  echo "export BUILD_TYPE" >> ${tcam_src_path}/build/bitshake/meta-vcc-tcam1/recipes-telematics/vocconv/vocconv.bb

  if [ "$1" = "clean" ]; then
    rm -rf ${tcam_src_path}/out/tcam1/build/vocconv
  fi
}

build () {
  ${tcam_src_path}/tools/bitbake/bin/bitbake vocconv -v
}

clean () {
  cd ${tcam_src_path}/build/bitshake/meta-vcc-tcam1/recipes-telematics/vocconv/
  git checkout -- .
  git clean -xfd
  cd -
}

setup_env
setup_build $1
build
clean
