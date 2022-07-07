#!/bin/bash
set -e

function start_envoy { 
    echo "Executing envoy entrypoint"
    exec /docker-entrypoint.sh envoy -c /etc/envoy/envoy.yaml; 
}

if [ -e "/run/secrets/envoy_ssl_chain" ]; then
    echo "Found envoy_ssl_chain secret"
    cp /run/secrets/envoy_ssl_chain /etc/ssl/chain.txt

    if [ -e "/run/secrets/envoy_ssl_private" ]; then
        echo "Found envoy_ssl_private secret"
        cp /run/secrets/envoy_ssl_private /etc/ssl/private.txt
    else
        echo "Couldn't find envoy_ssl_private secret"
        exit 64
    fi

    start_envoy
fi

if [ -e "/run/secrets/envoy_ssl_pkcs12" ]; then
    echo "Found envoy_ssl_pkcs12 secret"
    cp /run/secrets/envoy_ssl_pkcs12 /etc/ssl/pkcs12.pfx
    start_envoy
fi


if [ -n "$SSL_CHAIN" ]; then
    echo "Found SSL_CHAIN variable"
    echo "$SSL_CHAIN" > /etc/ssl/chain.txt

    if [ -n "$SSL_PRIVATE" ]; then
        echo "Found SSL_PRIVATE variable"
        echo "$SSL_PRIVATE" > /etc/ssl/private.txt
    else
        echo "Couldn't find SSL_PRIVATE"
        exit 64
    fi

    start_envoy
fi

if [ -n "$SSL_PKCS12" ]; then
    echo "Found SSL_PKCS12"
    echo "$SSL_PKCS12"  > /etc/ssl/pkcs12.pfx
    start_envoy
fi

echo "Could not found any certificate"
exit 65