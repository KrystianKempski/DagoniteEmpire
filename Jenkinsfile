#!/usr/bin/env groovy
library "Jenkins_TCAM@$GERRIT_BRANCH"

config = [
    git: 'telematics/apps/VocConv',
    testCommands: ['./unittest_vocconv']
]

run_job(config)
