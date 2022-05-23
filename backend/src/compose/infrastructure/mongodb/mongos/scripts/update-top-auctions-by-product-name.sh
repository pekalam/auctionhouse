#!/bin/bash

mongosh 127.0.0.1/appDb --username "auctionhouse-user" --password "Test-1234" /scripts/top_auctions_by_product_name_update.js
