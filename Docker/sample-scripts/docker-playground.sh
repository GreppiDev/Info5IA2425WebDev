#!/usr/bin/env bash

docker ps -a
docker run ubuntu command
docker run -d ubuntu sleep 200
docker exec  stoic_mayer cat /etc/hosts