# Thumbnail Generation System

This document describes the comprehensive thumbnail generation system implemented in PolyBucket, which provides both automatic and custom thumbnail generation for 3D models.

## Overview

The thumbnail generation system consists of two main components:

1. **Automatic Thumbnail Generation**: Automatically generates thumbnails when users upload 3D models without providing their own thumbnail images
2. **Custom Thumbnail Generation**: Allows users to create custom thumbnails in the browser with full control over rendering settings

## Architecture

### Backend Components

#### 1. Model Preview Generation Service
- **Location**: `backend/PolyBucket.Api/Features/Models/GenerateModelPreview/Services/ModelPreviewGenerationService.cs`
- **Purpose**: Core service that generates thumbnails using headless browser rendering
- **Technology**: Uses PuppeteerSharp for headless Chrome rendering and SixLabors.ImageSharp for image processing

#### 2. Background Job Service
- **Location**: `backend/PolyBucket.Api/Features/Models/GenerateModelPreview/Services/ModelPreviewBackgroundJobService.cs`
- **Purpose**: Handles thumbnail generation as background jobs using Hangfire
- **Features**: Automatic retry, status tracking, error handling

#### 3. Model Upload Service
- **Location**: `backend/PolyBucket.Api/Features/Models/CreateModel/Services/ModelUploadService.cs`
- **Purpose**: Automatically triggers thumbnail generation when models are uploaded without thumbnails

#### 4. API Controllers
- **GenerateModelPreviewController**: Handles automatic thumbnail generation requests
- **GenerateCustomThumbnailController**: Handles custom thumbnail generation with user-defined settings

### Frontend Components

#### 1. Thumbnail Generator Component
- **Location**: `frontend/src/components/models/ThumbnailGenerator.tsx`
- **Purpose**: Browser-based thumbnail creation tool with real-time 3D preview
- **Features**: 
  - Real-time 3D model rendering using Three.js
  - Customizable render settings (colors, materials, lighting, camera)
  - Canvas-to-image conversion for thumbnail generation

#### 2. Thumbnail Service
- **Location**: `frontend/src/services/thumbnailService.ts`
- **Purpose**: API client for thumbnail generation endpoints
- **Features**: Custom and automatic thumbnail generation, status checking

## Features

### Automatic Thumbnail Generation

When a user uploads a 3D model without providing a thumbnail image, the system automatically:

1. **Detects 3D Model Files**: Identifies supported 3D file formats (.stl, .obj, .fbx, .gltf, .glb, .ply)
2. **Generates Default Thumbnail**: Creates a thumbnail using default rendering settings
3. **Background Processing**: Uses Hangfire to process thumbnails asynchronously
4. **Status Tracking**: Provides real-time status updates on generation progress

**Default Settings**:
- Background: Dark gray (#1a1a1a)
- Model Color: Medium gray (#888888)
- Material: Standard (metalness: 0.5, roughness: 0.5)
- Lighting: Studio lighting setup
- Camera: Optimal distance for model visibility

### Custom Thumbnail Generation

Users can create custom thumbnails with full control over:

#### Render Settings
- **Background Color**: Custom background color picker
- **Model Color**: Custom model material color
- **Material Properties**: Metalness and roughness sliders
- **View Mode**: Solid, wireframe, or points rendering
- **Camera Distance**: Adjustable camera positioning
- **Lighting**: Intensity and color controls
- **Auto-rotation**: Optional model rotation

#### User Interface
- **Real-time Preview**: Live 3D preview with immediate feedback
- **Interactive Controls**: Sliders, color pickers, and toggles
- **Canvas Export**: Direct canvas-to-PNG conversion
- **Modal Interface**: Full-screen modal for focused thumbnail creation

## Supported File Formats

### 3D Model Formats
- **STL**: Stereolithography files (most common for 3D printing)
- **OBJ**: Wavefront Object files
- **FBX**: Autodesk FBX files
- **GLTF/GLB**: Khronos Group glTF files
- **PLY**: Stanford Polygon Library files

### Image Output
- **Format**: PNG with transparency support
- **Quality**: High-quality rendering with anti-aliasing
- **Sizes**: Thumbnail (300px), Medium (600px), Large (1200px)

## Technical Implementation

### Backend Rendering Pipeline

1. **File Processing**: Downloads model file from storage
2. **HTML Generation**: Creates Three.js-based HTML for rendering
3. **Headless Rendering**: Uses PuppeteerSharp to render in headless Chrome
4. **Screenshot Capture**: Takes high-quality screenshot of rendered scene
5. **Image Processing**: Resizes and optimizes image using ImageSharp
6. **Storage Upload**: Saves thumbnail to object storage
7. **URL Generation**: Creates presigned URLs for access

### Frontend Rendering Pipeline

1. **File Loading**: Loads 3D model file in browser
2. **Three.js Rendering**: Renders model using Three.js with user settings
3. **Real-time Updates**: Updates preview as settings change
4. **Canvas Export**: Converts Three.js canvas to PNG blob
5. **File Creation**: Creates downloadable thumbnail file

## API Endpoints

### Automatic Thumbnail Generation
```
POST /api/models/{modelId}/previews
```

**Parameters**:
- `size`: Thumbnail size (thumbnail, medium, large)
- `forceRegenerate`: Force regeneration of existing thumbnails

### Custom Thumbnail Generation
```
POST /api/models/{modelId}/custom-thumbnail
```

**Request Body**:
```json
{
  "modelFileUrl": "string",
  "fileType": "string",
  "size": "string",
  "settings": {
    "backgroundColor": "#1a1a1a",
    "modelColor": "#888888",
    "metalness": 0.5,
    "roughness": 0.5,
    "autoRotate": false,
    "cameraDistance": 2.5,
    "lightIntensity": 1.0,
    "lightColor": "#ffffff",
    "viewMode": "solid"
  },
  "forceRegenerate": false
}
```

### Thumbnail Status
```
GET /api/models/{modelId}/previews/{size}
```

## Configuration

### Backend Configuration

Add to `appsettings.json`:
```json
{
  "ThumbnailGeneration": {
    "DefaultSettings": {
      "Width": 800,
      "Height": 600,
      "BackgroundColor": "#1a1a1a",
      "ModelColor": "#888888",
      "Metalness": 0.5,
      "Roughness": 0.5,
      "AutoRotate": false,
      "CameraDistance": 2.5,
      "Lighting": "studio",
      "ViewMode": "solid",
      "LightIntensity": 1.0,
      "LightColor": "#ffffff"
    },
    "SupportedFormats": [".stl", ".obj", ".fbx", ".gltf", ".glb", ".ply"],
    "MaxProcessingTime": 300,
    "RetryAttempts": 3
  }
}
```

### Frontend Configuration

The thumbnail generator uses the existing Three.js setup and doesn't require additional configuration.

## Usage Examples

### Automatic Generation

When uploading a model without a thumbnail:

```typescript
// The system automatically detects missing thumbnails
// and generates them in the background
const model = await modelsService.uploadModel({
  modelData: { name: "My Model", ... },
  files: [stlFile]
});

// Check thumbnail status
const thumbnailStatus = await thumbnailService.getThumbnailStatus(model.id);
```

### Custom Generation

Creating a custom thumbnail:

```typescript
const settings = {
  backgroundColor: "#000000",
  modelColor: "#ff6b6b",
  metalness: 0.8,
  roughness: 0.2,
  autoRotate: true,
  cameraDistance: 3.0,
  lightIntensity: 1.5,
  lightColor: "#ffffff",
  viewMode: "solid"
};

const result = await thumbnailService.generateCustomThumbnail(modelId, {
  modelFileUrl: model.fileUrl,
  fileType: "stl",
  settings: settings
});
```

## Performance Considerations

### Backend Performance
- **Parallel Processing**: Multiple thumbnails can be generated simultaneously
- **Caching**: Generated thumbnails are cached in object storage
- **Background Jobs**: Non-blocking thumbnail generation
- **Resource Management**: Automatic cleanup of temporary files

### Frontend Performance
- **Lazy Loading**: Three.js components load on demand
- **Canvas Optimization**: Efficient canvas-to-blob conversion
- **Memory Management**: Proper cleanup of WebGL contexts
- **Responsive Design**: Optimized for various screen sizes

## Error Handling

### Common Issues
1. **Unsupported File Format**: Graceful fallback to default thumbnail
2. **Rendering Failures**: Automatic retry with different settings
3. **Memory Issues**: Resource cleanup and error recovery
4. **Network Timeouts**: Configurable timeout settings

### Error Recovery
- **Automatic Retries**: Up to 3 retry attempts for failed generations
- **Fallback Thumbnails**: Default thumbnails for failed generations
- **Status Tracking**: Detailed error messages and status updates
- **User Notifications**: Clear feedback on generation status

## Future Enhancements

### Planned Features
1. **Batch Processing**: Generate thumbnails for multiple models
2. **Advanced Materials**: PBR materials and textures
3. **Animation Support**: Animated thumbnails (GIF/WebM)
4. **AI Enhancement**: AI-powered thumbnail optimization
5. **Template System**: Predefined thumbnail templates
6. **Batch Operations**: Bulk thumbnail generation for existing models

### Technical Improvements
1. **GPU Acceleration**: WebGL-based rendering improvements
2. **Compression**: Advanced image compression algorithms
3. **CDN Integration**: Global thumbnail distribution
4. **Analytics**: Thumbnail generation metrics and monitoring

## Troubleshooting

### Common Problems

1. **Thumbnails Not Generating**
   - Check file format support
   - Verify model file integrity
   - Check background job status

2. **Poor Quality Thumbnails**
   - Adjust render settings
   - Increase resolution
   - Check lighting configuration

3. **Slow Generation**
   - Monitor system resources
   - Check background job queue
   - Optimize model file size

### Debug Information

Enable debug logging in `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "PolyBucket.Api.Features.Models.GenerateModelPreview": "Debug"
    }
  }
}
```

## Security Considerations

1. **File Validation**: Strict file type and size validation
2. **Access Control**: Authentication required for custom generation
3. **Resource Limits**: Timeout and memory limits for rendering
4. **Input Sanitization**: Validation of all user inputs
5. **Storage Security**: Secure object storage with access controls 