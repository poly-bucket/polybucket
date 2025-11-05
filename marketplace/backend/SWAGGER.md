# Swagger API Documentation

The PolyBucket Marketplace API includes comprehensive Swagger/OpenAPI documentation that enables developers to explore, test, and integrate with all available endpoints.

## Accessing Swagger UI

### Local Development

When running the API locally, Swagger UI is available at:

```
http://localhost:10280/swagger
```

### Docker/Production

When running in Docker or production, Swagger is accessible at:

```
http://<host>:<port>/swagger
```

By default, the marketplace API runs on port **10280**.

### Configuration

Swagger can be enabled or disabled via configuration:

**appsettings.json:**
```json
{
  "EnableSwagger": true
}
```

Set `EnableSwagger` to `false` to disable Swagger in production environments.

## Features

### Interactive API Testing

- **Try It Out**: Test endpoints directly from the Swagger UI
- **Request/Response Examples**: See example requests and responses
- **Schema Documentation**: View detailed data models and properties

### Authentication

Swagger UI includes JWT Bearer token authentication support:

1. Click the **Authorize** button at the top of the Swagger UI
2. Enter your JWT token in the format: `Bearer <your-token>`
3. Click **Authorize** to authenticate all requests
4. Protected endpoints will now include your authentication token

### Getting an Authentication Token

To obtain a JWT token:

1. Use the `/api/auth/github/url` endpoint to get the GitHub OAuth URL
2. Complete the OAuth flow via `/api/auth/github/callback`
3. The response will include a `token` field with your JWT token
4. Use this token in the Swagger UI authorization

## API Endpoints

The Swagger documentation includes all available endpoints organized by controller:

### Authentication (`/api/auth`)
- GitHub OAuth authentication
- Token refresh
- User profile management

### Plugins (`/api/plugins`)
- Browse and search plugins
- Get plugin details
- Download plugins
- Record installations
- Main API integration endpoints

### Developer (`/api/developer`)
- Developer statistics and analytics
- Plugin management (create, update, delete)
- Developer dashboard features

### Reviews (`/api/plugins/{pluginId}/reviews`)
- Create and manage reviews
- View plugin ratings
- Mark reviews as helpful

### User Management (`/api/usermanagement`)
- Role management
- User status management
- Permission management

### Files (`/api/files`)
- Download plugin files
- File retrieval

## Integration Examples

### Using the API in Your Application

The Swagger documentation provides OpenAPI 3.0 specification that can be used to:

1. **Generate Client SDKs**: Use tools like OpenAPI Generator or NSwag to generate client libraries
2. **API Testing**: Import the OpenAPI spec into Postman, Insomnia, or other API testing tools
3. **Documentation**: Use the spec to generate documentation for your integration

### Accessing the OpenAPI JSON

The OpenAPI specification is available at:

```
http://localhost:10280/swagger/v1/swagger.json
```

You can download this JSON file and use it with:
- OpenAPI Generator
- Postman
- Insomnia
- Swagger Editor
- Any OpenAPI-compatible tool

## Best Practices

1. **Test in Development**: Use Swagger UI to test endpoints during development
2. **Generate Client Code**: Use the OpenAPI spec to generate type-safe client code
3. **Document Your Integration**: Share the Swagger URL with your team for API exploration
4. **Security**: Disable Swagger in production by setting `EnableSwagger: false` in production environments

## Troubleshooting

### Swagger Not Loading

1. Check that `EnableSwagger` is set to `true` in `appsettings.json`
2. Verify the application is running on the correct port
3. Check browser console for any JavaScript errors
4. Ensure the XML documentation file is generated (check `bin/Debug/net8.0/` or `bin/Release/net8.0/`)

### Authentication Not Working

1. Ensure you're using the correct token format: `Bearer <token>`
2. Verify your token hasn't expired (JWT tokens expire after 1 hour)
3. Use the `/api/auth/refresh` endpoint to get a new token

### Missing Endpoints

1. Ensure all controllers have proper route attributes
2. Check that XML documentation is being generated
3. Verify controllers are properly registered in the DI container

## Support

For issues or questions about the API:
- Check the Swagger documentation for endpoint details
- Review the XML comments in the code for detailed parameter descriptions
- Contact support at support@polybucket.com

