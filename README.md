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

## License

This project is licensed under the PolyForm Noncommercial License 1.0.0.

**Copyright © John Smith**

**Original Repository:** https://github.com/poly-bucket/polybucket

This software is the original work of Cody Fraker. Any distribution, fork, or derivative work must include proper attribution as specified in the [LICENSE](LICENSE) file.
 