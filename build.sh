#!/usr/bin/env bash
set -euo pipefail
dotnet run --project ./SL.GHActions.Build/SL.GHActions.Build.csproj -- "$@"