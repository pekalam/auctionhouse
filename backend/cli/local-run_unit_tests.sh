#!/bin/sh

echo "Running unit tests..."
dotnet test ../src/UnitTests -v n || { echo "Domain tests failed"; exit 1; }