#!/bin/bash

rg="$1"
name="$2"
dest="$3"

echo "Creating key with name $name in rg $rg"

cmd_out=$(az sshkey create --name "$name" --resource-group "$rg" 2>&1)

echo "$cmd_out"

pub_key_path=$(echo "$cmd_out" | grep -Eo "Public key[^\"]+\"[^\"]+\"" | grep -o "\".*\"" | tr -d '\"')
priv_key_path=$(echo "$cmd_out" | grep -Eo "Private key[^\"]+\"[^\"]+\"" | grep -o "\".*\"" | tr -d '\"')

echo "priv: $priv_key_path"
echo "pub: $pub_key_path"

mkdir -p "$dest"

priv_dest="$dest/$name.key"
pub_dest="$dest/$name.pub"

echo "copying priv to $priv_dest"
cp -T "$priv_key_path" "$priv_dest"

echo "copying pub to $pub_dest"
cp -T "$pub_key_path" "$pub_dest"