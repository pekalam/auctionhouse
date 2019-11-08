#!/bin/sh

echo "Running unit tests..."
dotnet test ../src/UnitTests || { echo "Domain tests failed"; exit 1; }