# Model Management Documentation

This document describes the comprehensive model upload, edit, and delete functionality implemented in the PolyBucket 3D model sharing application.

## Overview

The application provides a complete workflow for authenticated users to manage their 3D models, including:

- **Upload Models**: Support for multiple file formats with drag-and-drop interface
- **Edit Models**: Update metadata and upload new versions
- **Delete Models**: Soft delete with confirmation dialogs
- **Version Management**: Create new versions with file uploads

## Backend API Endpoints

### Model Upload

**Endpoint**: `POST /api/models`

**Authentication**: Required (Bearer token)

**Permissions**: `MODEL_CREATE`

**Request Format**: `multipart/form-data`

**Parameters**:
- `name` (string, required): Model name
- `description` (string, optional): Model description
- `privacy` (string, optional): "public" or "private"
- `license` (string, optional): License type
- `categories` (string, optional): JSON array of categories
- `aiGenerated` (boolean, optional): Whether model is AI-generated
- `workInProgress` (boolean, optional): Whether model is work in progress
- `nsfw` (boolean, optional): Whether model is NSFW
- `remix` (boolean, optional): Whether model is a remix
- `thumbnailFileId` (string, optional): ID of thumbnail file
- `files` (file[], required): Model files

**Supported File Formats**:
- 3D Models: `.stl`, `.obj`, `.fbx`, `.gltf`, `.glb`
- Images: `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`, `.bmp`

**File Size Limits**:
- 3D Models: 500MB per file
- Images: 100MB per file
- Total upload: 1GB

**Response**: `201 Created` with model data

### Model Update

**Endpoint**: `PUT /api/models/{id}`

**Authentication**: Required (Bearer token)

**Permissions**: `MODEL_EDIT_OWN` or `MODEL_EDIT_ANY`

**Request Format**: `application/json`

**Parameters**:
- `name` (string, optional): Model name
- `description` (string, optional): Model description
- `license` (LicenseTypes, optional): License type
- `privacy` (PrivacySettings, optional): Privacy setting
- `aiGenerated` (boolean, optional): Whether model is AI-generated
- `wip` (boolean, optional): Whether model is work in progress
- `nsfw` (boolean, optional): Whether model is NSFW
- `isRemix` (boolean, optional): Whether model is a remix
- `remixUrl` (string, optional): URL to original model
- `isFeatured` (boolean, optional): Whether model is featured

**Response**: `200 OK` with updated model data

### Model Delete

**Endpoint**: `DELETE /api/models/{id}`

**Authentication**: Required (Bearer token)

**Permissions**: `MODEL_DELETE_OWN` or `MODEL_DELETE_ANY`

**Response**: `204 No Content`

**Note**: Implements soft delete - model is marked as deleted but not physically removed

### Model Version Creation

**Endpoint**: `POST /api/models/{id}/versions`

**Authentication**: Required (Bearer token)

**Permissions**: `MODEL_EDIT_OWN` or `MODEL_EDIT_ANY`

**Request Format**: `multipart/form-data`

**Parameters**:
- `name` (string, required): Version name
- `notes` (string, optional): Version notes
- `thumbnailFileId` (string, optional): ID of thumbnail file
- `files` (file[], required): Version files

**Response**: `201 Created` with version data

## Frontend Components

### ModelUpload.tsx

The main upload page component with the following features:

- **Drag-and-Drop Interface**: Users can drag files directly onto the upload area
- **File Selection**: Traditional file picker for selecting files
- **3D Preview**: Real-time 3D model preview using Three.js
- **Form Validation**: Client-side validation for required fields
- **Progress Tracking**: Upload progress indicator
- **Multiple File Support**: Upload multiple files in a single request
- **Thumbnail Selection**: Choose which file to use as thumbnail

**Key Features**:
- Supports all major 3D file formats
- Real-time file validation
- Automatic thumbnail generation
- Responsive design
- Error handling with user feedback

### ModelEditForm.tsx

Component for editing model metadata:

- **Pre-populated Fields**: Form loads with existing model data
- **Validation**: Client-side validation for required fields
- **Category Management**: Add/remove categories
- **Tag Management**: Add/remove tags
- **Privacy Controls**: Public/private visibility settings
- **License Selection**: Dropdown with common licenses
- **Advanced Options**: AI-generated, WIP, NSFW, remix flags

### EditModelModal.tsx

Modal component that combines editing and version management:

- **Tabbed Interface**: Separate tabs for editing and version management
- **Model Updates**: Update model metadata
- **Version Creation**: Create new model versions
- **File Upload**: Upload new files for versions
- **Progress Tracking**: Show progress during operations

### ModelVersionManager.tsx

Component for managing model versions:

- **Version Creation**: Create new versions with files
- **File Upload**: Support for multiple file uploads
- **Version Notes**: Add descriptive notes for each version
- **Thumbnail Selection**: Choose thumbnail for version
- **Validation**: Ensure at least one 3D model file

## Authentication and Permissions

### Permission System

The application uses a role-based permission system:

- **MODEL_CREATE**: Required to upload new models
- **MODEL_EDIT_OWN**: Required to edit own models
- **MODEL_EDIT_ANY**: Required to edit any model (admin)
- **MODEL_DELETE_OWN**: Required to delete own models
- **MODEL_DELETE_ANY**: Required to delete any model (admin)

### Ownership Validation

- Users can only edit/delete their own models
- Admin users with appropriate permissions can edit/delete any model
- Version creation is restricted to model owners or admins

## Testing

### Backend Integration Tests

Comprehensive test coverage for all endpoints:

- **ModelUploadTests.cs**: Tests for upload functionality
- **ModelEditTests.cs**: Tests for edit functionality  
- **ModelDeleteTests.cs**: Tests for delete functionality

**Test Coverage**:
- Valid data scenarios
- Authentication requirements
- Permission validation
- Error handling
- Edge cases

### Frontend E2E Tests

Cypress tests covering the complete user workflow:

- **model-management.cy.js**: Comprehensive E2E tests

**Test Scenarios**:
- Complete upload flow
- Edit model metadata
- Create new versions
- Delete models with confirmation
- Error handling
- Progress indicators
- Validation errors

## Error Handling

### Backend Error Responses

- **400 Bad Request**: Invalid input data
- **401 Unauthorized**: Missing or invalid authentication
- **403 Forbidden**: Insufficient permissions
- **404 Not Found**: Model not found
- **500 Internal Server Error**: Server-side errors

### Frontend Error Handling

- **Toast Notifications**: User-friendly error messages
- **Form Validation**: Client-side validation with immediate feedback
- **Progress Indicators**: Show upload/operation progress
- **Retry Mechanisms**: Allow users to retry failed operations

## File Storage

### Storage Service

The application uses a pluggable storage service:

- **Local Storage**: For development
- **AWS S3**: For production
- **Azure Blob Storage**: Alternative cloud storage

### File Organization

Files are organized in the following structure:

```
models/
├── {modelId}/
│   ├── {fileId}_{filename}  # Model files
│   └── versions/
│       └── {versionId}/
│           └── {fileId}_{filename}  # Version files
```

### File Cleanup

- Failed uploads are automatically cleaned up
- Deleted models retain files for potential recovery
- Storage cleanup jobs can be implemented for permanent deletion

## Security Considerations

### File Upload Security

- **File Type Validation**: Strict validation of file extensions
- **File Size Limits**: Prevent abuse through large uploads
- **Content Scanning**: Optional virus/malware scanning
- **Access Control**: Files are only accessible to authorized users

### Data Protection

- **Soft Delete**: Models are not permanently deleted
- **Audit Trail**: All operations are logged
- **User Isolation**: Users cannot access other users' private models
- **Input Sanitization**: All user input is validated and sanitized

## Performance Considerations

### Upload Optimization

- **Chunked Uploads**: Large files are uploaded in chunks
- **Progress Tracking**: Real-time upload progress
- **Background Processing**: File processing happens asynchronously
- **CDN Integration**: Files are served through CDN for better performance

### Database Optimization

- **Indexing**: Proper database indexes for queries
- **Pagination**: Large result sets are paginated
- **Caching**: Frequently accessed data is cached
- **Lazy Loading**: Related data is loaded on demand

## Future Enhancements

### Planned Features

- **Bulk Operations**: Upload/edit/delete multiple models
- **Advanced Search**: Full-text search with filters
- **Collaboration**: Shared model editing
- **Version Comparison**: Visual diff between versions
- **Export Options**: Export models in different formats
- **Analytics**: Usage statistics and insights

### Technical Improvements

- **Real-time Updates**: WebSocket integration for live updates
- **Offline Support**: PWA capabilities for offline access
- **Mobile Optimization**: Enhanced mobile experience
- **API Versioning**: Proper API versioning strategy
- **Rate Limiting**: Prevent API abuse
- **Monitoring**: Comprehensive application monitoring

## Deployment

### Environment Variables

Required environment variables:

```bash
# Database
DATABASE_CONNECTION_STRING=

# Storage
STORAGE_TYPE=Local|AwsS3|AzureBlob
AWS_ACCESS_KEY_ID=
AWS_SECRET_ACCESS_KEY=
AWS_BUCKET_NAME=
AZURE_CONNECTION_STRING=
AZURE_CONTAINER_NAME=

# Authentication
JWT_SECRET=
JWT_ISSUER=
JWT_AUDIENCE=

# API Configuration
API_URL=
CORS_ORIGINS=
```

### Docker Deployment

The application includes Docker configuration:

```bash
# Build and run
docker-compose up --build

# Production deployment
docker-compose -f docker-compose.prod.yml up -d
```

## Support

For issues and questions:

1. Check the test suite for expected behavior
2. Review the API documentation
3. Check server logs for error details
4. Verify authentication and permissions
5. Test with different file types and sizes

## Contributing

When contributing to model management features:

1. Follow the existing code patterns
2. Add comprehensive tests
3. Update documentation
4. Test with various file types
5. Verify error handling
6. Check performance impact 