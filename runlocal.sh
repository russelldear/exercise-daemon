#!/bin/bash

docker build -t "exercise-daemon" . 

docker run -p 5236:5236 --name exercise-daemon exercise-daemon