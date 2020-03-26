#!/bin/bash
$(aws ecr get-login --no-include-email --region us-east-1)
docker build -t exercise-daemon .
docker tag exercise-daemon:latest 540629508292.dkr.ecr.us-east-1.amazonaws.com/exercise-daemon:latest
docker push 540629508292.dkr.ecr.us-east-1.amazonaws.com/exercise-daemon:latest

add-apt-repository ppa:eugenesan/ppa
apt-get update
apt-get install jq -y

TASK=$(aws ecs describe-task-definition --task-definition exercise-daemon --output json | jq '.taskDefinition.containerDefinitions')
aws ecs register-task-definition --family exercise-daemon --container-definitions "$TASK" --task-role-arn arn:aws:iam::540629508292:role/ecsTaskExecutionRole --execution-role-arn arn:aws:iam::540629508292:role/ecsTaskExecutionRole

REVISION=$(aws ecs describe-task-definition --task-definition exercise-daemon --output json  | jq -r '.taskDefinition.revision')
aws ecs update-service --cluster default-ec2 --service exercise-daemon --task-definition exercise-daemon:$REVISION

#run chmod +x deploy.sh first if you have permissions issues

