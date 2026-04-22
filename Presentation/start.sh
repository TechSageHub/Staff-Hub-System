#!/bin/sh
set -eu

PORT_VALUE="${PORT:-8080}"
export ASPNETCORE_URLS="http://0.0.0.0:${PORT_VALUE}"
export ASPNETCORE_HTTP_PORTS="${PORT_VALUE}"

exec dotnet Presentation.dll
