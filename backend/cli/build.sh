#!/bin/sh

if [ "$1" = "no-test" ] ; then
	TEST="no-test"
else
	TEST="test"
fi

if [ "$TEST" = "test" ] ; then
	./local-run_domain_tests.sh
fi

echo "Building update-server docker image..."