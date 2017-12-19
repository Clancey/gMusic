#!/bin/bash

echo 'Set Bundle ID to com.youriisolutions.gMusic2'
/usr/libexec/PlistBuddy -c "Set :CFBundleIdentifier com.youriisolutions.gMusic2" Info.plist
/usr/libexec/PlistBuddy -c "Set :CFBundleDisplayName gMusic" Info.plist
/usr/libexec/PlistBuddy -c "Delete :keychain-access-groups:0" Entitlements.plist
/usr/libexec/PlistBuddy -c "Add :keychain-access-groups:0 string group.com.iis.music" Entitlements.plist