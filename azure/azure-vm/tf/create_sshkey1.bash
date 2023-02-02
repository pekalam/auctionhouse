#!/bin/bash

rg='auctionhouse-keys'
name='sshk-vm-backend-dev'

scripts/create_ssh.bash "$rg" "$name" "$(pwd)/ssh"