#!/bin/bash

# This script tests connectivity to a MinIO instance
# Usage: ./test-minio.sh [host_ip]

HOST=${1:-localhost}
PORT=9000
USER=minioadmin
PASS=minioadmin
BUCKET=polybucket-uploads
TEST_FILE="test-file-$(date +%s).txt"

echo "Testing MinIO connectivity to $HOST:$PORT..."
echo "Using credentials: $USER / $PASS"

# Check if mc is installed
if ! command -v mc &> /dev/null; then
    echo "MinIO client (mc) is not installed. Please install it first:"
    echo "  Linux: wget https://dl.min.io/client/mc/release/linux-amd64/mc"
    echo "  macOS: brew install minio/stable/mc"
    echo "  Windows: scoop install minio-client"
    exit 1
fi

# Configure mc
echo "Configuring MinIO client..."
mc config host add test-host http://$HOST:$PORT $USER $PASS

# Check if bucket exists
echo "Checking if bucket '$BUCKET' exists..."
if mc ls test-host/$BUCKET &> /dev/null; then
    echo "Bucket '$BUCKET' exists!"
else
    echo "Warning: Bucket '$BUCKET' does not exist. Creating it..."
    mc mb test-host/$BUCKET
fi

# Create test file
echo "Creating test file..."
echo "This is a test file for MinIO connectivity. Created at $(date)" > $TEST_FILE

# Upload test file
echo "Uploading test file to MinIO..."
mc cp $TEST_FILE test-host/$BUCKET

# Check if file exists
echo "Checking if file was uploaded successfully..."
if mc ls test-host/$BUCKET/$TEST_FILE &> /dev/null; then
    echo "Success! File was uploaded."
else
    echo "Error: File was not uploaded."
    rm $TEST_FILE
    exit 1
fi

# Remove test file
echo "Cleaning up..."
mc rm test-host/$BUCKET/$TEST_FILE
rm $TEST_FILE

# Print URL for manual testing
echo ""
echo "MinIO Console URL: http://$HOST:9001"
echo "Username: $USER"
echo "Password: $PASS"
echo ""
echo "All tests passed! MinIO is properly configured and accessible." 