#!/bin/sh

echo "Running domain tests..."
dotnet test ../src/Core.DomainModelTests || { echo "Domain tests failed"; exit 1; }