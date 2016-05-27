#!/bin/bash

/usr/libexec/PlistBuddy -c "Set :CFBundleIdentifier com.IIS.MusicPlayer.iOS" Info.plist
/usr/libexec/PlistBuddy -c "Set :CFBundleDisplayName gMusic beta" Info.plist