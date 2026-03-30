#!/bin/bash
dotnet publish ./FIAE2GO.Desktop -c Release
dotnet publish ./FIAE2GO.Desktop -c Release -r win-x64
dotnet publish ./FIAE2GO.Android -c Release