#!/bin/sh
if [ "$CODEBUILD_BUILD_SUCCEEDING" = "1" ]
then
  aws ec2 authorize-security-group-ingress --group-id $RdsSecurityGroupId --protocol tcp --port 1433 --cidr $CodeBuildCidr
  dotnet run --project ParkingRota.DatabaseUpgrader
  aws ec2 revoke-security-group-ingress --group-id $RdsSecurityGroupId --protocol tcp --port 1433 --cidr $CodeBuildCidr
  dotnet publish -c release
  cd ParkingRota
  dotnet lambda deploy-serverless
fi