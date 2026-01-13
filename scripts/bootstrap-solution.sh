#!/usr/bin/env bash
# Regenerates ResourceBooking.sln and wires up project references.
# Not committed as a built artifact so each contributor's local .sln
# doesn't churn in diffs - run this once after cloning.
set -euo pipefail

dotnet new sln -n ResourceBooking --force

for proj in src/*/*.csproj tests/*/*.csproj; do
  dotnet sln add "$proj"
done

echo "Solution regenerated: ResourceBooking.sln"
