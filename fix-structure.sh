#!/bin/bash

# Fix the nested project structure

# Navigate to the source directory
cd src

# Core project
echo "Fixing Core project structure..."
# Create project file if it doesn't exist
cp -n PolyBucket.Core/PolyBucket.Core/PolyBucket.Core.csproj PolyBucket.Core/
# Move all files from nested folder to parent
cp -r PolyBucket.Core/PolyBucket.Core/Interfaces/ PolyBucket.Core/
cp -r PolyBucket.Core/PolyBucket.Core/Models/ PolyBucket.Core/
cp -r PolyBucket.Core/PolyBucket.Core/Configuration/ PolyBucket.Core/

# Infrastructure project
echo "Fixing Infrastructure project structure..."
# Create project file if it doesn't exist
cp -n PolyBucket.Infrastructure/PolyBucket.Infrastructure/PolyBucket.Infrastructure.csproj PolyBucket.Infrastructure/
# Move all files from nested folder to parent
cp -r PolyBucket.Infrastructure/PolyBucket.Infrastructure/Services/ PolyBucket.Infrastructure/
cp -r PolyBucket.Infrastructure/PolyBucket.Infrastructure/Data/ PolyBucket.Infrastructure/

# Api project
echo "Fixing Api project structure..."
mkdir -p PolyBucket.Api.temp
cp -r PolyBucket.Api/PolyBucket.Api/* PolyBucket.Api.temp/
rm -rf PolyBucket.Api/PolyBucket.Api
cp -r PolyBucket.Api.temp/* PolyBucket.Api/
rm -rf PolyBucket.Api.temp

echo "Structure fixed. Please update namespaces and project references manually if needed." 