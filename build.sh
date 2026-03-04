#!/usr/bin/env bash
set -euo pipefail
dotnet run --project ./DdcApply.Tools.Build.csproj -- "$@"