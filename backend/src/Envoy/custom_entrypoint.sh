#!/bin/bash
set -e

if [ -n "$SSL_CHAIN" ]; then
    echo "Found SSL_CHAIN variable"
else
    echo "SSL_CHAIN variable not found. Executing enboy entrypoint"
    exec /docker-entrypoint.sh envoy -c /etc/envoy/envoy.yaml
fi
if [ -n "$SSL_PRIVATE" ]; then
    echo "Found SSL_PRIVATE variable"
else
    exit 128;
fi
echo "writing ssl certs"
echo "$SSL_CHAIN" > /etc/ssl/chain.txt
echo "$SSL_PRIVATE" > /etc/ssl/private.txt

echo "Executing enoy entrypoint"
exec /docker-entrypoint.sh envoy -c /etc/envoy/envoy.yaml