#!/bin/bash

url=$1
jwt=$2
imgDir=$3
count=$4

files=($(ls "$imgDir"))
cd ../../CmdTools/CreateAuction
for (( i=0; i<$count; i++ ))
do
	rand=$(((RANDOM<<15|RANDOM) % ${#files[@]}))
	echo "Generating $i auction with img ${files[$rand]}..."
	dotnet run $url $jwt "$imgDir/${files[$rand]}" --no-build &
	echo "finished"
done