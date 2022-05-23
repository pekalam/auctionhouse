#!/bin/bash

mongosh 127.0.0.1/appDb --username "auctionhouse-user" --password "Test-1234" /scripts/top_auctions_in_tag_view_update.js
