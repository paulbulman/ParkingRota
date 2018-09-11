#!/bin/sh
if [ "$CODEBUILD_BUILD_SUCCEEDING" = "1" ]
then
  dotnet publish -c release
  cd ParkingRota
  dotnet eb deploy-environment
fi