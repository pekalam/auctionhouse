#!/bin/bash

CONFIG="Debug"

echo "Building categories data..."
pushd ../scripts/mongodb/
npm i 
npm run start
cp categories.js ../../mongodb/mongos/scripts/
popd

echo "Building backend..."
pushd ../src/Web
dotnet build -c "$CONFIG"
popd