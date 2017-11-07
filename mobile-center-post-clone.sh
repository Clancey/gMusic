#!/usr/bin/env bash
if [[ -z "${ApiConstantsUrl}" ]]; then
    echo 'No ApiConstantsUrl'
else
    echo 'Downloading ApiConstants.cs'
    curl -o ApiConstants.cs "${ApiConstantsUrl}"
fi

cd MusicPlayer.iOS
if [[ ${MOBILECENTER_BRANCH} = "AppStoreRelease" ]]
then
    echo 'App Store Build'
    cd MusicPlayer.iOS
    sh ./SetupAppStore.sh
else
    echo 'Normal Branch'
    sh ./SetupDev.sh
fi
cd ..

sh ./Setup.sh
