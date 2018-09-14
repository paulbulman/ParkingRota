#!/bin/sh
if [ "$CODEBUILD_BUILD_SUCCEEDING" = "1" ]
then
  dotnet run --project ParkingRota.DatabaseUpgrader
  cd ParkingRota
  dotnet lambda deploy-serverless
fi