#!/bin/bash

docker secret rm registry.{htpasswd,key,crt}
set -e
az login --identity
set +e

kv_name='auctionhouse-kv-prod'
cert_name='auctionhouse-cert'
htpasswd_name='registry-pass'

# temp file names for certficates and secrets
pfx_file=$(mktemp -u)
crt_file=$(mktemp -u)
key_file=$(mktemp -u)
htpasswd_file=$(mktemp -u)
bundle_file=$(mktemp -u)

set -e
prev_umask=$(umask)
umask 377
az keyvault secret download --vault-name $kv_name --name $cert_name --encoding base64 -f "$pfx_file"
az keyvault secret download --vault-name $kv_name --name $htpasswd_name -f "$htpasswd_file"
umask $prev_umask
set +e

echo -e "\ncreating registry.crt"
openssl pkcs12 -in "$pfx_file" -cacerts -nokeys -chain -out "$bundle_file" -passin pass:
openssl pkcs12 -in "$pfx_file" -clcerts -nokeys -out "$crt_file" -passin pass:

echo "creating registry.key"
openssl pkcs12 -in "$pfx_file" -nocerts -nodes -out "$key_file" -passin pass:

echo -e "\ncreating registry.crt secret"
cat "$crt_file" "$bundle_file" | sed -n '/-\+BEGIN CERTIFICATE-\+/,/-\+END CERTIFICATE-\+/p' | docker secret create registry.crt -

echo "creating registry.key secret"
sed -n '/-\+BEGIN PRIVATE KEY-\+/,/-\+END PRIVATE KEY-\+/p' "$key_file" | docker secret create registry.key -

echo "creating registry.htpasswd secret"
cat "$htpasswd_file" | docker secret create registry.htpasswd -

echo "cleanup"
rm -f "$pfx_file" "$crt_file" "$key_file" "$htpasswd_file" "$bundle_file"