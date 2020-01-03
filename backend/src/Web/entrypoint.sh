#!/bin/bash

set -e

if [ -e "/run/secrets/auctionhouse-webapi_appsettings" ]; then
    ln -s "/run/secrets/auctionhouse-webapi_appsettings" /app/appsettings.Production.json
fi

categories_xml_data=/app/_Categories-xml-data exec dotnet Web.dll