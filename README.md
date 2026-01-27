# PolyBucket - Self-Hosted 3D File Sharing Platform

![Discord](https://img.shields.io/discord/785605975621107743)
![License](https://img.shields.io/badge/license-PolyForm%20Noncommercial-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![React](https://img.shields.io/badge/React-18+-61dafb)
![TypeScript](https://img.shields.io/badge/TypeScript-5.0+-3178c6)

**PolyBucket** is a self-hosted, open-source alternative to platforms like Thingiverse, Makerworld, Printables, and Cults3D. It allows users to share, discover, and download 3D files, including models for 3D printing, CNC machines, laser cutters, and more. Built with modern technologies and designed for extensibility, PolyBucket gives you complete control over your 3D model repository.

**Keywords**: self-hosted 3D file sharing, Thingiverse alternative, 3D model repository, STL file hosting, 3D printing platform, open source 3D sharing, self-hosted Thingiverse, 3D model management, 3D file storage, maker community platform, 3D printing community, CNC file sharing, laser cutting files, 3D model viewer, 3D printing database, federated 3D sharing, 3D model marketplace, 3D printing software, model versioning, 3D model collaboration

## Table of Contents

- [Features](#features)
- [Quick Start](#quick-start)
- [Installation](#installation)
- [Use Cases](#use-cases)
- [Comparison](#comparison-with-other-platforms)
- [Requirements](#requirements)
- [Tech Stack](#tech-stack)
- [Contributing](#contributing)
- [FAQ](#faq)
- [License](#license)


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
Contact me [on Discord](https://discord.gg/EX94hH5RYt) or see the [Contributing](#contributing) section above.

## Screenshots & Demo

> **Note**: Screenshots coming soon! If you'd like to contribute screenshots or a demo video, please open a pull request.

## Troubleshooting

### Common Issues

**Q: Docker containers won't start**
- Ensure Docker and Docker Compose are properly installed
- Check that ports 3000, 5000, 5432, and 9000 are not in use
- Review logs: `docker-compose logs`

**Q: Database connection errors**
- Verify PostgreSQL is running and accessible
- Check connection string in appsettings.json
- Ensure database exists and migrations have run

**Q: Storage upload failures**
- Verify MinIO/S3 credentials are correct
- Check storage bucket permissions
- Ensure storage service is accessible from the API

**Q: Frontend won't connect to backend**
- Verify API URL in frontend configuration
- Check CORS settings in backend
- Ensure backend is running on the expected port

For more troubleshooting help, visit our [Discord](https://discord.gg/EX94hH5RYt) or check the [documentation](https://github.com/poly-bucket/polybucket/wiki).

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

## Quick Start

Get PolyBucket up and running in minutes with Docker:

```bash
# Clone the repository
git clone https://github.com/poly-bucket/polybucket.git
cd polybucket

# Start with Docker Compose
docker-compose up -d
```

Access the application at `http://localhost:3000` and complete the first-time setup wizard.

## Installation

### Prerequisites

- **Docker** and **Docker Compose** (recommended)
- **.NET 8.0 SDK** (for local backend development)
- **Node.js 18+** and **npm/yarn** (for local frontend development)
- **PostgreSQL 14+** (if not using Docker)
- **MinIO** or **S3-compatible storage** (if not using Docker)

### Docker Installation (Recommended)

The easiest way to get started is using Docker Compose:

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

### Manual Installation

For development or production deployments without Docker:

1. **Backend Setup**:
   ```bash
   cd backend/PolyBucket.Api
   dotnet restore
   dotnet run
   ```

2. **Frontend Setup**:
   ```bash
   cd frontend
   npm install
   npm run dev
   ```

3. **Database Setup**:
   - Configure PostgreSQL connection string
   - Run migrations: `dotnet ef database update`

4. **Storage Setup**:
   - Configure MinIO or S3-compatible storage
   - Set storage credentials in appsettings.json

See the [documentation](https://github.com/poly-bucket/polybucket/wiki) for detailed setup instructions.

## Use Cases

PolyBucket is perfect for:

- **3D Printing Communities**: Host your own repository for 3D printable models
- **Maker Spaces**: Share designs within your organization or community
- **Educational Institutions**: Create a private repository for students and faculty
- **Commercial Use**: Host proprietary 3D models for internal use
- **CNC & Laser Cutting**: Manage files for CNC machines and laser cutters
- **Federated Networks**: Connect multiple instances to share repositories
- **Custom Branding**: White-label solution with full customization
- **Privacy-Focused Users**: Keep your designs private and self-hosted
- **Plugin Developers**: Extend functionality with custom plugins
- **Enterprise Deployments**: Full control over data and infrastructure

## Comparison with Other Platforms

| Feature | PolyBucket | Thingiverse | Printables | Cults3D |
|---------|-----------|-------------|------------|---------|
| Self-Hosted | ✅ Yes | ❌ No | ❌ No | ❌ No |
| Open Source | ✅ Yes | ❌ No | ❌ No | ❌ No |
| Plugin System | ✅ Yes | ❌ No | ❌ No | ❌ No |
| Federation | ✅ Yes | ❌ No | ❌ No | ❌ No |
| Full Data Control | ✅ Yes | ❌ No | ❌ No | ❌ No |
| Custom Branding | ✅ Yes | ❌ No | ❌ No | ❌ No |
| API Access | ✅ Full REST API | ⚠️ Limited | ⚠️ Limited | ⚠️ Limited |
| Moderation Tools | ✅ Advanced | ✅ Basic | ✅ Basic | ✅ Basic |
| Printer Management | ✅ Yes | ❌ No | ⚠️ Limited | ❌ No |
| Free & Open | ✅ Yes | ✅ Free | ✅ Free | ⚠️ Freemium |

## Requirements

### Minimum System Requirements

- **CPU**: 2 cores
- **RAM**: 4GB
- **Storage**: 20GB (varies based on model library size)
- **OS**: Linux, Windows, or macOS (Docker recommended)

### Recommended System Requirements

- **CPU**: 4+ cores
- **RAM**: 8GB+
- **Storage**: 100GB+ SSD
- **OS**: Linux with Docker
- **Network**: Stable internet connection for federation features

### Software Dependencies

- Docker 20.10+ and Docker Compose 2.0+ (for containerized deployment)
- PostgreSQL 14+ (or use included Docker image)
- MinIO or S3-compatible storage (or use included Docker image)
- Modern web browser (Chrome, Firefox, Safari, Edge)

## Tech Stack

- **Backend**: C# with ASP.NET Core 8.0
- **Frontend**: React 18+ with TypeScript and Redux
- **3D Rendering**: Three.js for interactive model previews
- **Database**: PostgreSQL 14+
- **Storage**: MinIO (default), AWS S3, and other S3-compatible providers
- **Deployment**: Docker and Docker Compose
- **Authentication**: JWT tokens with optional OAuth support
- **API**: RESTful API with comprehensive documentation

## Contributing

We welcome contributions! PolyBucket is an open-source project and thrives on community involvement.

### How to Contribute

1. **Fork the repository** and create your feature branch
2. **Make your changes** following our coding standards
3. **Write tests** for new features (if applicable)
4. **Submit a pull request** with a clear description of your changes

### Areas Where We Need Help

- 🐛 **Bug Fixes**: Help us squash bugs and improve stability
- 📚 **Documentation**: Improve docs, add examples, write tutorials
- 🎨 **UI/UX Improvements**: Enhance the user interface and experience
- 🔌 **Plugins**: Create and share plugins with the community
- 🌐 **Translations**: Help translate PolyBucket to other languages
- ⚡ **Performance**: Optimize queries, improve caching, reduce load times
- 🧪 **Testing**: Write tests, improve test coverage

### Getting Help

- 💬 [Join our Discord](https://discord.gg/EX94hH5RYt) for real-time discussions
- 📖 Check the [documentation](https://github.com/poly-bucket/polybucket/wiki)
- 🐛 Report bugs via [GitHub Issues](https://github.com/poly-bucket/polybucket/issues)
- 💡 Suggest features via [GitHub Discussions](https://github.com/poly-bucket/polybucket/discussions)

## Related Projects

- **[PolyBucket Marketplace](marketplace/README.md)**: Plugin marketplace for discovering and installing plugins
- **[PolyBucket Starter Template](examples/polybucket-starter-template/README.md)**: Template for creating new plugins
- **[Dark Theme Plugin](examples/plugins/dark-theme-plugin/)**: Example plugin demonstrating theme customization

## License

This project is licensed under the PolyForm Noncommercial License 1.0.0.

**Copyright © Cody Fraker**

**Original Repository:** https://github.com/poly-bucket/polybucket

This software is the original work of Cody Fraker. Any distribution, fork, or derivative work must include proper attribution as specified in the [LICENSE](LICENSE) file.

---

## Star History

If you find PolyBucket useful, please consider giving it a ⭐ on GitHub! Your support helps the project grow.

## Support

- 💬 [Discord Community](https://discord.gg/EX94hH5RYt) - Join our community for support and discussions
- 📖 [Documentation](https://github.com/poly-bucket/polybucket/wiki) - Comprehensive guides and API docs
- 🐛 [Report Issues](https://github.com/poly-bucket/polybucket/issues) - Found a bug? Let us know!
- 💡 [Feature Requests](https://github.com/poly-bucket/polybucket/discussions) - Have an idea? Share it with us!

## Acknowledgments

Special thanks to all contributors, testers, and community members who help make PolyBucket better every day.
 