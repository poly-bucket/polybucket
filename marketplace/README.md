# PolyBucket Marketplace

The PolyBucket Plugin Marketplace is a separate microservice that provides a curated, community-driven platform for plugin discovery, installation, and management.

## Architecture

This marketplace operates as an independent service within the PolyBucket monorepo, enabling seamless integration while maintaining deployment flexibility.

### Services

- **marketplace-api**: C# ASP.NET Core API (Port 5001)
- **marketplace-frontend**: Next.js React application (Port 3001)
- **marketplace-db**: PostgreSQL database (Port 5433)
- **redis**: Shared Redis instance for caching

## Development Setup

### Prerequisites

- Docker and Docker Compose
- .NET 8.0 SDK (for local development)
- Node.js 18+ (for local development)

### Quick Start

1. **Start all services (main app + marketplace)**:
   ```bash
   docker-compose -f docker-compose.marketplace.yml up
   ```

2. **Start only marketplace services**:
   ```bash
   cd marketplace
   docker-compose up
   ```

3. **Access the applications**:
   - Main PolyBucket: http://localhost:3000
   - Marketplace: http://localhost:3001
   - Main API: http://localhost:5000
   - Marketplace API: http://localhost:5001

### Local Development

#### Backend Development

```bash
cd marketplace/backend
dotnet restore
dotnet run
```

#### Frontend Development

```bash
cd marketplace/frontend
npm install
npm run dev
```

## Project Structure

```
marketplace/
├── backend/                    # C# ASP.NET Core API
│   ├── PolyBucket.Marketplace.Api/
│   │   ├── Controllers/       # API Controllers
│   │   ├── Services/          # Business Logic
│   │   ├── Models/           # Data Models
│   │   ├── Data/             # Database Context
│   │   └── Program.cs        # Application Entry Point
│   └── Dockerfile
├── frontend/                  # Next.js React Application
│   ├── src/
│   │   ├── app/             # Next.js App Router
│   │   ├── components/      # React Components
│   │   ├── lib/             # Utilities
│   │   └── types/           # TypeScript Types
│   ├── package.json
│   └── Dockerfile
├── docker-compose.yml        # Marketplace Services
└── README.md
```

## API Endpoints

### Plugins
- `GET /api/plugins` - List plugins with filtering
- `GET /api/plugins/{id}` - Get plugin details
- `POST /api/plugins/{id}/install` - Install plugin
- `POST /api/plugins/{id}/rate` - Rate plugin
- `POST /api/plugins/{id}/review` - Add review

### Categories
- `GET /api/categories` - List categories
- `GET /api/categories/{id}/plugins` - Get plugins by category

### Analytics
- `POST /api/analytics/installations` - Record installation
- `GET /api/analytics/plugins/{id}/stats` - Get plugin statistics

## Frontend Features

### Pages
- **Home**: Featured plugins, categories, search
- **Plugin Details**: Plugin information, reviews, installation
- **Categories**: Browse plugins by category
- **Developer Dashboard**: Plugin management for developers

### Components
- **PluginCard**: Plugin listing component
- **PluginDetails**: Detailed plugin view
- **SearchAndFilters**: Search and filtering interface
- **ReviewSystem**: Plugin reviews and ratings

## Database Schema

### Core Tables
- `plugins` - Plugin information
- `plugin_versions` - Plugin version history
- `plugin_categories` - Plugin categories
- `plugin_reviews` - User reviews
- `plugin_ratings` - User ratings
- `plugin_downloads` - Download tracking
- `plugin_submissions` - Plugin submission workflow

## Environment Variables

### Backend
- `ConnectionStrings__DefaultConnection` - Database connection
- `PolyBucket__ApiUrl` - Main PolyBucket API URL
- `Authentication__Authority` - JWT authority
- `Authentication__Audience` - JWT audience

### Frontend
- `NEXT_PUBLIC_API_URL` - Marketplace API URL
- `NEXT_PUBLIC_POLYBUCKET_URL` - Main PolyBucket URL

## Deployment

### Production Deployment

1. **Build and deploy**:
   ```bash
   docker-compose -f docker-compose.marketplace.yml -f docker-compose.prod.yml up -d
   ```

2. **Environment configuration**:
   - Set production environment variables
   - Configure SSL certificates
   - Set up monitoring and logging

### Domain Configuration

- **Main App**: `polybucket.com`
- **Marketplace**: `marketplace.polybucket.com` or `plugins.polybucket.com`

## Development Workflow

### Adding New Features

1. **Backend**: Add controllers, services, and models
2. **Frontend**: Create components and pages
3. **Database**: Add migrations for schema changes
4. **Testing**: Write unit and integration tests

### Plugin Development

1. **Create plugin** using PolyBucket CLI
2. **Test locally** with marketplace integration
3. **Submit to marketplace** via API or web interface
4. **Review process** and approval workflow

## Monitoring and Analytics

### Metrics
- Plugin downloads and installations
- User engagement and retention
- Developer activity and submissions
- Performance and error tracking

### Logging
- Structured logging with Serilog
- Centralized log aggregation
- Error tracking and alerting

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## License

This project is part of the PolyBucket ecosystem and follows the same licensing terms.
