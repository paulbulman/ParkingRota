#!/bin/sh
if [ "$CODEBUILD_BUILD_SUCCEEDING" = "1" ]
then
  dotnet run --project ParkingRota.DatabaseUpgrader
  
  cd ParkingRota.Service
  dotnet lambda deploy-function
  
  cd ../ParkingRota
  dotnet eb deploy-environment
fi