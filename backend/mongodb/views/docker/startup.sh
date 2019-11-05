#!/bin/bash


crond &

crontab /root/update-cronjob

./root/scripts/update-top-auctions-in-tag-view.sh
./root/scripts/update-common-tags-view.sh

exec bash
