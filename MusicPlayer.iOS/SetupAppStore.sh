#!/bin/bash

echo 'Set Bundle ID to com.youriisolutions.gMusic2'
/usr/libexec/PlistBuddy -c "Set :CFBundleIdentifier com.youriisolutions.gMusic2" Info.plist
/usr/libexec/PlistBuddy -c "Set :CFBundleDisplayName gMusic" Info.plist