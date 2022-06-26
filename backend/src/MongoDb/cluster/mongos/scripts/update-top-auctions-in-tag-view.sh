#!/bin/bash

mongosh 127.0.0.1/appDb --username "auctionhouse-user" --password "$(cat /run/secrets/mongouser-password)" /scripts/top_auctions_in_tag_view_update.js
