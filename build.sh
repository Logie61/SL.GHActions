#!/usr/bin/env bash
set -euo pipefail
dotnet run --project ./DdcApply.Tools.Build/DdcApply.Tools.Build.csproj -- "$@"