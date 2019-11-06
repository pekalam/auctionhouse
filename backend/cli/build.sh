#!/bin/sh

if [ "$1" = "no-test" ] ; then
	TEST="no-test"
else
	TEST="test"
fi

if [ "$TEST" = "test" ] ; then
	echo "Running domain tests..."
	dotnet test ../src/Core.DomainModelTests || { echo "Domain tests failed"; exit 1; }
fi

echo "Building update-server docker image..."
pushd ../mongodb/views/docker
docker build -f Dockerfile -t update-server .
popd