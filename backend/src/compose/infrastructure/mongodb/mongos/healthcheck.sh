#!/bin/bash

set -e

mongosh 127.0.0.1/appDb --username "marek" --password "Test-1234" /scripts/healthcheck.js
