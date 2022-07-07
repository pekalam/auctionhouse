#!/bin/bash

mongosh 127.0.0.1/appDb --username "auctionhouse-user" --password "$(cat /run/secrets/mongouser-password)" /scripts/common_tags_view_update.js
