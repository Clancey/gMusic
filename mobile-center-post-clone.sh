#!/usr/bin/env bash
if [[ -z "${ApiConstantsUrl}" ]]; then
    echo 'No ApiConstantsUrl'
else
    echo 'Downloading ApiConstants.cs'
    curl -o ApiConstants.cs "${ApiConstantsUrl}"
fi

if [[ ${MOBILECENTER_BRANCH} = "AppStoreRelease" ]]
then
    echo 'App Store Build'
    cd MusicPlayer.iOS
    sh ./SetupAppStore.sh
    cd ..
else
    echo 'Normal Branch'
fi

sh ./Setup.sh
