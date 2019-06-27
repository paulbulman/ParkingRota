#!/bin/sh
if [ "$CODEBUILD_BUILD_SUCCEEDING" = "1" ]
then
  cd ParkingRota
  dotnet eb deploy-environment
fi