# MinIO Integration Guide for PolyBucket

This guide explains how MinIO is integrated into the PolyBucket application and how to configure and use it effectively, especially when working across different machines.

## What is MinIO?

MinIO is a high-performance, S3-compatible object storage system. In PolyBucket, we use MinIO to store user-uploaded files such as 3D models, reference images, and other assets.

## Docker Setup for MinIO

Our `docker-compose.yml` file includes a MinIO service that is configured with:

- Default ports (9000 for API, 9001 for Console)
- Default credentials (minioadmin/minioadmin)
- A persistent volume for data storage
- Health checks to ensure stability
- A helper service to create the required bucket automatically

## Accessing MinIO

### Local Access (same machine)

To access MinIO locally:

- **MinIO Console**: http://localhost:9001 
- **API Endpoint**: http://localhost:9000
- **Credentials**: Username: `minioadmin`, Password: `minioadmin`

### Remote Access (different machine)

To access MinIO from a different machine:

1. Ensure that ports 9000 and 9001 are allowed in your firewall
2. Use the host machine's IP address instead of `localhost`:
   - **MinIO Console**: http://[HOST_IP]:9001
   - **API Endpoint**: http://[HOST_IP]:9000
   - **Credentials**: Same as local access

## Configuration in PolyBucket

The application is configured to use MinIO through settings in `appsettings.json`:

```json
"Storage": {
  "Endpoint": "localhost",
  "Port": 9000,
  "AccessKey": "minioadmin",
  "SecretKey": "minioadmin",
  "BucketName": "polybucket-uploads",
  "UseSSL": false
}
```

When using PolyBucket on a different machine from where MinIO is running, update the `Endpoint` to the IP address of the MinIO host machine.

## Storage Service Implementation

The project includes a `MinioStorageService` that provides:

- File uploads with unique file names
- File downloads with proper content-type handling
- File deletion
- Existence checking
- URL generation for accessing files directly

The service is implemented in `src/PolyBucket.Api/PolyBucket.Api/Services/MinioStorageService.cs` and provides a clean abstraction over the MinIO client library.

## API Endpoints

The `FileStorageController` provides these endpoints for file operations:

- `POST /api/FileStorage/upload` - Upload a new file
- `GET /api/FileStorage/{fileName}` - Download a file
- `DELETE /api/FileStorage/{fileName}` - Delete a file
- `GET /api/FileStorage/check/{fileName}` - Check if a file exists

## Frontend Integration

A React component `FileUploader.jsx` is provided that allows users to:

- Upload files with progress indication
- View uploaded files with size and date
- Download files directly
- Delete files from storage

## Testing MinIO Connectivity

The project includes test scripts in the `scripts` directory:

- `test-minio.sh` - For Linux/Mac users
- `test-minio.ps1` - For Windows users

These scripts test connectivity to MinIO, bucket existence, and file operations.

### Usage:

```bash
# Test connection to MinIO on the local machine
./scripts/test-minio.sh

# Test connection to MinIO on a remote machine
./scripts/test-minio.sh 192.168.1.100
```

## Advanced Configuration

### Sharing Data Between Machines

For development teams that need to share the same uploaded files:

1. **Network Storage**: Mount the MinIO data volume to a network file system
2. **Distributed Mode**: Configure MinIO in distributed mode for high availability
3. **Central MinIO**: Use a single dedicated MinIO server that all team members connect to

### Securing MinIO

For production deployments:

1. Change the default credentials
2. Enable TLS/SSL
3. Set up proper bucket policies
4. Configure backup and replication
5. Implement access control through IAM policies

## Troubleshooting

### Common Issues

1. **Cannot connect to MinIO**:
   - Check firewall settings
   - Verify the correct IP address is being used
   - Ensure Docker containers are running

2. **Permission denied when uploading/downloading**:
   - Check bucket policies
   - Verify credentials in `appsettings.json`

3. **Files not appearing in MinIO console**:
   - Verify the correct bucket name is being used
   - Check upload code for errors

### Logs

To view MinIO logs:

```bash
docker-compose logs minio
```

## Resources

- [MinIO Documentation](https://docs.min.io/)
- [S3 Compatible API Reference](https://docs.min.io/docs/minio-client-complete-guide.html)
- [MinIO Client (mc) Guide](https://docs.min.io/docs/minio-client-quickstart-guide.html) 