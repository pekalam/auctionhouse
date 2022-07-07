#!/bin/bash

mongosh 127.0.0.1/appDb --username "auctionhouse-user" --password "$(cat /run/secrets/mongouser-password)" /scripts/remove_unasigned_auction_images.js
