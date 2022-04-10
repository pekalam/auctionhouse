#!/bin/bash
set -e

start_nginx(){
    echo "Executing nginx entrypoint"
    exec /docker-entrypoint.sh "$@"
}

if [ -e "/run/secrets/front_ssl_chain" ]; then
    echo "Found front_ssl_chain secret"
    cp /run/secrets/front_ssl_chain /usr/share/nginx/ssl_certs/chain.txt

    if [ -e "/run/secrets/front_ssl_private" ]; then
        echo "Found front_ssl_private secret"
        cp /run/secrets/front_ssl_private /usr/share/nginx/ssl_certs/private.txt
    else
        echo "Couldn't find front_ssl_private"
        exit 64
    fi

    start_nginx "$@"
fi


if [ -n "$SSL_CHAIN" ]; then
    echo "Found SSL_CHAIN variable"
    echo "$SSL_CHAIN" > /usr/share/nginx/ssl_certs/chain.txt
else
    echo "SSL_CHAIN variable not found. Executing nginx entrypoint"
    start_nginx "$@"
fi
if [ -n "$SSL_PRIVATE" ]; then
    echo "Found SSL_PRIVATE variable"
    echo "$SSL_PRIVATE" > /usr/share/nginx/ssl_certs/private.txt
else
    exit 128;
fi

start_nginx "$@"