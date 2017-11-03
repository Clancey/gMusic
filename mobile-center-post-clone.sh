#!/usr/bin/env bash
if [[ -z "${ApiConstantsUrl}" ]]; then
    echo 'No ApiConstantsUrl'
else
    echo 'Downloading ApiConstants.cs's
    curl -o ApiConstants.cs "${ApiConstantsUrl}"
fi
sh ./Setup.sh