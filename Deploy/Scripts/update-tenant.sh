#!/bin/sh
docker-compose down && docker image rm dmibod/assistant-tenant && docker-compose up -d
