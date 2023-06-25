#!/bin/bash

# This ensures this script is run from folder containing it
SCRIPT=$(readlink -f "$0")
SCRIPTPATH=$(dirname "$SCRIPT")
cd "$SCRIPTPATH"

rm -rf outputbin/
mkdir -p outputbin/windows_cef/
mkdir -p outputbin/linux_puppeteer/
mkdir -p outputbin/linux_puppeteer_sc/
rm -rf TotallyNotCef/bin/

dotnet publish -c Release
mv TotallyNotCef/bin/Release/win-x64/publish outputbin/windows_cef/TotallyNotCef

dotnet publish -c Release -r linux-x64
mv TotallyNotCef/bin/Release/linux-x64/publish outputbin/linux_puppeteer/TotallyNotCef

dotnet publish -c Release -r linux-x64 -p:SelfContained=true
mv TotallyNotCef/bin/Release/linux-x64/publish outputbin/linux_puppeteer_sc/TotallyNotCef

cd outputbin/windows_cef/
7z a TotallyNotCef.zip TotallyNotCef/

cd ../linux_puppeteer/
7z a TotallyNotCef_linux.zip TotallyNotCef/

cd ../linux_puppeteer_sc/
7z a TotallyNotCef_linux_selfcontained.zip TotallyNotCef/
