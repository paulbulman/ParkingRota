#!/bin/sh
if [ "$CODEBUILD_BUILD_SUCCEEDING" = "1" ]
then
  dotnet run --project ParkingRota.DatabaseUpgrader
  
  cd ParkingRota.Service
  mv -f aws-lambda-tools-defaults.develop.json aws-lambda-tools-defaults.json
  dotnet lambda deploy-serverless
  
  cd ../ParkingRota
  dotnet lambda deploy-serverless
fi