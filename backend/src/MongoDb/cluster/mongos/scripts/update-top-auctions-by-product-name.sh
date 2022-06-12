#!/bin/bash

mongosh 127.0.0.1/appDb --username "auctionhouse-user" --password "$(cat /run/secrets/mongouser-password)" /scripts/top_auctions_by_product_name_update.js
