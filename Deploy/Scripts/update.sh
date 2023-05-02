#!/bin/sh
docker-compose down && docker image rm dmibod/assistant-tenant && docker image rm dmibod/assistant-market && docker-compose up -d
