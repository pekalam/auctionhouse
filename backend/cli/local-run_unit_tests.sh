#!/bin/sh

echo "Running unit tests..."
dotnet test ../src/Test.UnitTests -v n || { echo "Domain tests failed"; exit 1; }