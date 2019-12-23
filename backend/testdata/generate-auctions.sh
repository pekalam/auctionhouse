#!/bin/bash

url=$1
jwt=$2
imgDir=$3
count=$4
categories=$5

files=($(ls "$imgDir"))
pushd ./CmdTools/CreateAuction
for (( i=0; i<$count; i++ ))
do
	rand=$(((RANDOM<<15|RANDOM) % ${#files[@]}))
	echo "Generating $i auction with img ${files[$rand]}..."
	dotnet run $url $jwt "$imgDir/${files[$rand]}" $categories --no-build &
	echo "finished"
done
popd