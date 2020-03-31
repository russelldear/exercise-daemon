#!/bin/bash

docker build -t "exercise-daemon" . 

docker run -p 5236:5236 -e ASPNETCORE_ENVIRONMENT=Development --name exercise-daemon exercise-daemon