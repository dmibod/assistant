#!/bin/sh
docker-compose down && docker image rm dmibod/kanban-monolith && docker-compose up -d
