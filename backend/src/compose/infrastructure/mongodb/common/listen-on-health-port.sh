#!/bin/bash

echo "Starting to listen on healthy state port 32112"
while true; do
nc -l -s 0.0.0.0 -p 32112
done