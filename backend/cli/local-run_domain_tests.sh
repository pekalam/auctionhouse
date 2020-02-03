#!/bin/sh

echo "Running domain tests..."
dotnet test ../src/Test.DomainModelTests || { echo "Domain tests failed"; exit 1; }