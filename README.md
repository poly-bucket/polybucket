# PolyBucket - Self-Hosted 3D File Sharing Platform
![Discord](https://img.shields.io/discord/785605975621107743)

PolyBucket is a self-hosted alternative to platforms like Thingiverse, Makerworld, Printables, and Cults3D. It allows users to share, discover, and download 3D files, including models for 3D printing, CNC machines, laser cutters, and more.


# 2026 Status:
Polybucket has been in development for many years. I've been working on this project entirely alone. This is my largest personal project that continues to grow in scope. I will be the first to say that I find many issues in the repo. Mostly due to the heavy usage of AI related tooling that has helped me scale the project to the size and feature set it is today. There is much work ahead of me. I'm very open to pull requests, suggestions and community help. [Join the Discord](https://discord.gg/EX94hH5RYt).

# FAQ

## Does it work?
Yes. Expect bugs. 

## Minio
At the time of writing this, Minio decided to pull out of the community supported Docker image. The stack does work with the latest Minio container. I do still plan to support it as a default. There are plans for supporting other, more commerical object storage solutions.

## Roadmap?
I am the roadmap.

## How can I help?
Contact me on Discord.

## Features

### File & Model Management

- **Multiple File Format Support**: Upload and manage STL, 3MF, OBJ, GCODE, and other 3D file formats
- **Model Versioning**: Track multiple versions of your models with full version history
- **Model Categorization**: Organize models with custom categories and tags
- **Model Privacy Controls**: Set models as public, private, or unlisted
- **Model Analytics**: Track downloads, views, likes, and engagement metrics
- **Model Moderation**: Built-in moderation queue for reviewing and approving models before publication
- **Model Remixing**: Mark models as remixes and link to original sources
- **Print Settings**: Store and manage print settings, layer heights, infill percentages, and material requirements
- **3D Model Viewer**: Integrated Three.js viewer for interactive 3D previews directly in the browser
- **Thumbnail Generation**: Automatic thumbnail generation for model previews

### User Management & Authentication

- **Local Authentication**: Secure username/password authentication with email verification
- **OAuth Integration**: Optional OAuth support for external authentication providers
- **Two-Factor Authentication (2FA)**: Enhanced security with 2FA support
- **User Profiles**: Customizable user profiles with avatars, bios, and social links
- **User Settings**: Comprehensive user settings management
- **Password Management**: Password reset, change, and recovery functionality
- **Role-Based Access Control (RBAC)**: Granular permission system with roles (User, Moderator, Admin)
- **User Statistics**: Track user activity, uploads, downloads, and engagement
- **User Banning**: Admin tools for managing user accounts and bans

### Social Features

- **Likes & Favorites**: Like models and favorite collections
- **Comments System**: Threaded comments on models with moderation capabilities
- **User Following**: Follow other users to see their latest uploads
- **Collections**: Create and manage custom collections of models
- **Public & Private Collections**: Share collections publicly or keep them private
- **User Activity Feed**: View activity from users you follow
- **Model Sharing**: Share models via direct links

### Search & Discovery

- **Advanced Search**: Search across models, users, and collections
- **Search Filters**: Filter by category, tags, file type, and more
- **Sorting Options**: Sort by relevance, popularity, date, downloads, and likes
- **Federated Search**: Search across connected federation instances
- **Tag System**: Tag models for better discoverability
- **Featured Models**: Highlight featured models on the homepage
- **Trending Content**: Discover trending models and popular uploads

### Moderation & Reporting

- **Content Reporting**: Report inappropriate content, spam, copyright violations, and other issues
- **Moderation Dashboard**: Comprehensive dashboard for moderators to review and handle reports
- **Moderation Queue**: Queue system for reviewing models before publication
- **Audit Logs**: Track all moderation actions and administrative changes
- **Report Analytics**: Analytics dashboard showing report trends and statistics
- **Comment Moderation**: Moderate and manage comments on models
- **User Moderation**: Tools for moderating user behavior and content

### Plugins & Extensibility

- **Plugin System**: Extensible architecture with a comprehensive plugin API
- **Plugin Marketplace**: Dedicated marketplace for discovering and installing plugins
- **Core Features as Plugins**: Many core features are implemented as plugins for maximum flexibility
- **Plugin Development Tools**: Starter templates and tools for plugin development
- **Plugin Hooks**: Integration points for plugins to extend functionality
- **Custom Metadata**: Plugin system for adding custom metadata fields to models
- **Theme Support**: Plugin-based theme system for customizing the UI

### Printer & Filament Management

- **Printer Profiles**: Create and manage printer profiles with detailed specifications
- **Printer Database**: Support for FDM, SLA, DLP, and SLS printer types
- **Build Volume Tracking**: Track build volumes and printer capabilities
- **Filament Management**: Manage filament inventory with manufacturer, type, color, and diameter
- **Material Compatibility**: Track which materials work with which printers
- **Print Settings Integration**: Link print settings to specific printer and filament combinations

### Administration

- **Admin Control Panel**: Comprehensive admin dashboard for system management
- **System Settings**: Configure site-wide settings, themes, and behavior
- **Category Management**: Create and manage model categories
- **User Management**: Admin tools for managing users, roles, and permissions
- **Analytics Dashboard**: System-wide analytics and statistics
- **Storage Management**: Monitor and manage storage usage and quotas
- **API Key Management**: Generate and manage API keys for programmatic access
- **Audit Logging**: Complete audit trail of administrative actions

### Federation

- **Federation Support**: Connect with other PolyBucket instances to share repositories
- **Cross-Instance Discovery**: Discover and access models from federated instances
- **Federated Authentication**: Optional authentication across federated instances
- **Federation Analytics**: Track federation activity and connections

### Storage & Files

- **Object Storage**: Flexible storage backend using MinIO (default), AWS S3, or other S3-compatible providers
- **File Upload Management**: Upload, manage, and organize files
- **Storage Quotas**: Configurable storage quotas per user
- **File Versioning**: Track file versions and changes
- **Storage Analytics**: Monitor storage usage and capacity

### API & Integration

- **RESTful API**: Comprehensive REST API for all features
- **API Authentication**: Secure API access with JWT tokens
- **API Rate Limiting**: Configurable rate limits for API endpoints
- **API Documentation**: Full API documentation for developers
- **Webhook Support**: Extend functionality with webhooks (via plugins)

### UI & Experience

- **Modern React UI**: Built with React, TypeScript, and Redux
- **Responsive Design**: Mobile-friendly responsive design
- **Theme System**: Customizable themes with plugin support
- **Dark Mode**: Built-in dark mode support
- **Accessibility**: Focus on accessibility and user experience
- **Real-time Updates**: Real-time updates for notifications and activity

## Tech Stack

- **Backend**: C# with ASP.NET Core
- **Frontend**: React + TypeScript with Redux
- **3D Rendering**: Three.js
- **Database**: PostgreSQL
- **Storage**: MinIO (default), AWS S3, and others
- **Deployment**: Docker

## License

This project is licensed under the PolyForm Noncommercial License 1.0.0.

**Copyright © Cody Fraker**

**Original Repository:** https://github.com/poly-bucket/polybucket

This software is the original work of Cody Fraker. Any distribution, fork, or derivative work must include proper attribution as specified in the [LICENSE](LICENSE) file.
 