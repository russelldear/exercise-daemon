#!/bin/bash

docker build -t "exercise-daemon" . 

docker run -p 5235:5235 --name exercise-daemon exercise-daemon