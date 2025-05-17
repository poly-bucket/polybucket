#!/bin/bash

# Fix the nested project structure

# Navigate to the source directory
cd src

echo "Creating corrected project structure..."

# Core project
mkdir -p PolyBucket.Core.new
cp -r PolyBucket.Core/PolyBucket.Core/* PolyBucket.Core.new/
cp -r PolyBucket.Core/Entities PolyBucket.Core.new/
cp -r PolyBucket.Core/Interfaces PolyBucket.Core.new/
cp -r PolyBucket.Core/PolyBucket.Core/Models PolyBucket.Core.new/ 2>/dev/null
cp -r PolyBucket.Core/PolyBucket.Core/Configuration PolyBucket.Core.new/ 2>/dev/null

# Infrastructure project
mkdir -p PolyBucket.Infrastructure.new
cp -r PolyBucket.Infrastructure/PolyBucket.Infrastructure/* PolyBucket.Infrastructure.new/
cp -r PolyBucket.Infrastructure/Data PolyBucket.Infrastructure.new/
cp -r PolyBucket.Infrastructure/Services PolyBucket.Infrastructure.new/
cp -r PolyBucket.Infrastructure/PolyBucket.Infrastructure/Repositories PolyBucket.Infrastructure.new/ 2>/dev/null
cp PolyBucket.Infrastructure/InfrastructureServiceRegistration.cs PolyBucket.Infrastructure.new/ 2>/dev/null

# API project
mkdir -p PolyBucket.Api.new
cp -r PolyBucket.Api/PolyBucket.Api/* PolyBucket.Api.new/
# Copy any root level files
cp -r PolyBucket.Api/Controllers PolyBucket.Api.new/ 2>/dev/null 
cp -r PolyBucket.Api/Data PolyBucket.Api.new/ 2>/dev/null
cp -r PolyBucket.Api/Services PolyBucket.Api.new/ 2>/dev/null
cp -r PolyBucket.Api/Extensions PolyBucket.Api.new/ 2>/dev/null

# Backup original directories
echo "Backing up original directories..."
mv PolyBucket.Core PolyBucket.Core.bak
mv PolyBucket.Infrastructure PolyBucket.Infrastructure.bak
mv PolyBucket.Api PolyBucket.Api.bak

# Move new directories to original names
echo "Moving new directories to original names..."
mv PolyBucket.Core.new PolyBucket.Core
mv PolyBucket.Infrastructure.new PolyBucket.Infrastructure
mv PolyBucket.Api.new PolyBucket.Api

# Update project references
echo "Updating project references..."
sed -i 's#\.\.\\\.\.\\PolyBucket\.Core\\PolyBucket\.Core\\PolyBucket\.Core\.csproj#\.\.\\PolyBucket\.Core\\PolyBucket\.Core\.csproj#g' PolyBucket.Api/PolyBucket.Api.csproj
sed -i 's#\.\.\\\.\.\\PolyBucket\.Infrastructure\\PolyBucket\.Infrastructure\\PolyBucket\.Infrastructure\.csproj#\.\.\\PolyBucket\.Infrastructure\\PolyBucket\.Infrastructure\.csproj#g' PolyBucket.Api/PolyBucket.Api.csproj

sed -i 's#\.\.\\\.\.\\PolyBucket\.Core\\PolyBucket\.Core\\PolyBucket\.Core\.csproj#\.\.\\PolyBucket\.Core\\PolyBucket\.Core\.csproj#g' PolyBucket.Infrastructure/PolyBucket.Infrastructure.csproj

echo "Structure fixed. Please rebuild the projects." 