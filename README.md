# PolyBucket - Self-Hosted 3D File Sharing Platform

PolyBucket is a self-hosted alternative to platforms like Thingiverse, Makerworld, Printables, and Cults3D. It allows users to share, discover, and download 3D files, including models for 3D printing, CNC machines, laser cutters, and more.

## Features

- **Support for Multiple File Types**: STL, 3MF, OBJ, GCODE, and more
- **User Authentication**: Local authentication with optional OAuth support
- **Model Management**: Upload, version, tag, and categorize models
- **Social Features**: Like, comment, follow users, and create collections
- **3D Viewer**: Integrated Three.js viewer for previewing models in the browser
- **Object Storage**: Uses MinIO by default, with support for AWS S3 and other providers
- **Extensible Architecture**: Core features implemented as plugins
- **Federation**: Connect with other instances to share repositories
- **Modern UI**: Responsive design built with React and TypeScript

## Tech Stack

- **Backend**: C# with ASP.NET Core
- **Frontend**: React + TypeScript with Redux
- **3D Rendering**: Three.js
- **Database**: PostgreSQL
- **Storage**: MinIO (default), AWS S3, and others
- **Deployment**: Docker

## Quick Start with Docker

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/polybucket.git
   cd polybucket
   ```

2. Start the services:
   ```bash
   docker-compose up -d
   ```

3. Access the application:
   - Frontend: http://localhost:3000
   - API: http://localhost:5000
   - MinIO Console: http://localhost:9001 (minioadmin/minioadmin)

## Local Development

### Backend (API)

1. Navigate to the API project:
   ```bash
   cd src/PolyBucket.Api/PolyBucket.Api
   ```

2. Update the database:
   ```bash
   dotnet ef database update
   ```

3. Run the API:
   ```bash
   dotnet run
   ```

### Frontend

1. Navigate to the frontend project:
   ```bash
   cd frontend
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Run the development server:
   ```bash
   npm start
   ```

### Using MinIO Across Different Machines

PolyBucket uses MinIO for object storage, which allows you to work across different machines with minimal configuration:

1. **Exposing MinIO to other machines**:
   - By default, MinIO runs on ports 9000 (API) and 9001 (Console)
   - Make sure these ports are allowed in your firewall

2. **Accessing MinIO from another machine**:
   - If your host machine IP is `192.168.1.100`, you can access:
     - MinIO Console: `http://192.168.1.100:9001` (username: minioadmin, password: minioadmin)
     - MinIO API: `http://192.168.1.100:9000`

3. **Using MinIO for development on another machine**:
   Update your appsettings.json to point to the remote MinIO instance:
   ```json
   "Storage": {
     "Endpoint": "192.168.1.100",
     "Port": 9000,
     "AccessKey": "minioadmin",
     "SecretKey": "minioadmin",
     "BucketName": "polybucket-uploads",
     "UseSSL": false
   }
   ```

4. **Sharing data between machines**:
   - Since MinIO stores data in a Docker volume, the data persists across restarts
   - To share the same storage between machines, consider using a network file system or MinIO's distributed mode

## Development

### Backend

The backend is organized following Clean Architecture principles with these projects:

- **PolyBucket.Api**: ASP.NET Core Web API
- **PolyBucket.Core**: Domain models and interfaces
- **PolyBucket.Infrastructure**: Implementation of interfaces, data access, etc.
- **PolyBucket.Plugins**: Extensibility points

To run tests:
```
dotnet test tests/PolyBucket.Tests
```

### Frontend

The frontend is built with React, TypeScript, and Redux:

```
cd frontend/client
npm install
npm run dev
```

To run tests:
```
npm run test
```

### Database Migrations

To create a new migration:
```
cd src/PolyBucket.Api/PolyBucket.Api
dotnet ef migrations add MigrationName
```

To apply migrations:
```
dotnet ef database update
```

## Documentation

- [MinIO Integration Guide](docs/minio-guide.md) - Detailed guide on MinIO configuration and usage across different machines

## License

This project is licensed under the terms of the MIT license. 