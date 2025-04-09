# This script tests connectivity to a MinIO instance
# Usage: .\test-minio.ps1 [host_ip]

param(
    [string]$Host = "localhost"
)

$Port = 9000
$User = "minioadmin"
$Pass = "minioadmin"
$Bucket = "polybucket-uploads"
$TestFile = "test-file-$(Get-Date -Format 'yyyyMMddHHmmss').txt"

Write-Host "Testing MinIO connectivity to $Host`:$Port..."
Write-Host "Using credentials: $User / $Pass"

# Check if mc is installed
if (!(Get-Command mc -ErrorAction SilentlyContinue)) {
    Write-Host "MinIO client (mc) is not installed. Please install it first:"
    Write-Host "  Using Scoop: scoop install minio-client"
    Write-Host "  Manual: Download from https://dl.min.io/client/mc/release/windows-amd64/mc.exe"
    exit 1
}

# Configure mc
Write-Host "Configuring MinIO client..."
mc config host add test-host http://$Host`:$Port $User $Pass

# Check if bucket exists
Write-Host "Checking if bucket '$Bucket' exists..."
try {
    mc ls test-host/$Bucket | Out-Null
    Write-Host "Bucket '$Bucket' exists!"
}
catch {
    Write-Host "Warning: Bucket '$Bucket' does not exist. Creating it..."
    mc mb test-host/$Bucket
}

# Create test file
Write-Host "Creating test file..."
"This is a test file for MinIO connectivity. Created at $(Get-Date)" | Out-File -FilePath $TestFile

# Upload test file
Write-Host "Uploading test file to MinIO..."
mc cp $TestFile test-host/$Bucket

# Check if file exists
Write-Host "Checking if file was uploaded successfully..."
try {
    mc ls test-host/$Bucket/$TestFile | Out-Null
    Write-Host "Success! File was uploaded."
}
catch {
    Write-Host "Error: File was not uploaded."
    Remove-Item $TestFile
    exit 1
}

# Remove test file
Write-Host "Cleaning up..."
mc rm test-host/$Bucket/$TestFile
Remove-Item $TestFile

# Print URL for manual testing
Write-Host ""
Write-Host "MinIO Console URL: http://$Host`:9001"
Write-Host "Username: $User"
Write-Host "Password: $Pass"
Write-Host ""
Write-Host "All tests passed! MinIO is properly configured and accessible." 