# GitHub Actions Workflows

This directory contains GitHub Actions workflows for automated builds and deployments.

## docker-build.yml

Automatically builds and publishes Docker images for the PolyBucket backend and frontend when code is pushed to the repository.

### Triggers

- **Push to main/master branch**: Builds and pushes images to GitHub Container Registry
- **Pull Requests**: Builds images for testing (does not push)
- **Tags (v*)**: Creates versioned images when you tag releases

### Image Locations

Images are published to GitHub Container Registry:
- Backend: `ghcr.io/YOUR_USERNAME/polybucket/polybucket-api`
- Frontend: `ghcr.io/YOUR_USERNAME/polybucket/polybucket-frontend`

### Image Tags

Images are tagged with:
- `latest` - Latest build from default branch
- `main` or `master` - Latest build from that branch
- `{branch}-{sha}` - Specific commit from a branch
- `v1.0.0` - Semantic version tags (if you create a tag like `v1.0.0`)
- `1.0` - Major.minor version (if you create a tag like `v1.0.0`)

### Usage

1. **Automatic**: Just push to your repository - images are built automatically
2. **Pull on Server**: 
   ```bash
   docker pull ghcr.io/YOUR_USERNAME/polybucket/polybucket-api:latest
   docker pull ghcr.io/YOUR_USERNAME/polybucket/polybucket-frontend:latest
   ```

### Permissions

The workflow uses `GITHUB_TOKEN` which is automatically provided by GitHub Actions. No additional secrets are required.

### Multi-Platform Support

Images are built for both `linux/amd64` and `linux/arm64` architectures, making them compatible with:
- Standard x86_64 servers
- ARM-based servers (like Raspberry Pi, AWS Graviton, etc.)

### Caching

Builds use GitHub Actions cache to speed up subsequent builds by caching Docker layers.

