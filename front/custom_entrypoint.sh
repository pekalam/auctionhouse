#!/bin/bash
set -e

if [ -n "$SSL_CHAIN" ]; then
    echo "Found SSL_CHAIN variable"
else
    echo "SSL_CHAIN variable not found. Executing nginx entrypoint"
    exec /docker-entrypoint.sh "$@"
fi
if [ -n "$SSL_PRIVATE" ]; then
    echo "Found SSL_PRIVATE variable"
else
    exit 128;
fi

echo "writing ssl certs"
echo "$SSL_CHAIN" > /usr/share/nginx/ssl_certs/chain.txt
echo "$SSL_PRIVATE" > /usr/share/nginx/ssl_certs/private.txt

echo "Executing nginx entrypoint"
exec /docker-entrypoint.sh "$@"