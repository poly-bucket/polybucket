# PolyBucket Starter Template

A comprehensive starter template and development toolkit for creating PolyBucket plugins. This plugin provides everything you need to get started with plugin development, including scaffolding tools, templates, examples, and documentation.

## 🚀 Features

- **Plugin Scaffolding**: Create new plugins with proper structure and configuration
- **Validation Tools**: Validate plugin structure and configuration
- **Testing Framework**: Built-in testing tools for plugin development
- **Packaging Tools**: Package plugins for distribution
- **Comprehensive Templates**: Multiple template types for different plugin categories
- **Documentation**: Complete guides and examples for plugin development

## 📦 Installation

Install this plugin through the PolyBucket marketplace or directly from GitHub:

```bash
# Via marketplace (recommended)
polybucket install polybucket-starter-template

# Direct from GitHub
git clone https://github.com/polybucket/polybucket-starter-template.git
cd polybucket-starter-template
npm install
```

## 🛠️ Available Tools

### `create-plugin`
Scaffolds a new PolyBucket plugin with proper structure and configuration.

```bash
polybucket create-plugin my-awesome-plugin
```

**Options:**
- `--template`: Choose template type (basic, advanced, tool-only)
- `--category`: Set plugin category
- `--description`: Set plugin description
- `--author`: Set plugin author

### `validate-plugin`
Validates plugin structure, configuration, and code quality.

```bash
polybucket validate-plugin ./my-plugin
```

**Checks:**
- Package.json configuration
- Plugin structure compliance
- Code quality and best practices
- Documentation completeness

### `test-plugin`
Runs plugin tests and validates functionality.

```bash
polybucket test-plugin ./my-plugin
```

**Features:**
- Unit test execution
- Integration test validation
- Code coverage reporting
- Performance benchmarking

### `package-plugin`
Packages plugin for distribution and marketplace submission.

```bash
polybucket package-plugin ./my-plugin
```

**Output:**
- Creates distribution package
- Generates marketplace metadata
- Validates marketplace requirements
- Creates installation package

## 📁 Plugin Structure

This plugin demonstrates the recommended structure for PolyBucket plugins:

```
my-plugin/
├── README.md                 # Plugin documentation
├── package.json              # Plugin metadata and dependencies
├── polybucket.config.js      # Plugin configuration
├── src/
│   ├── index.js              # Main plugin entry point
│   ├── tools/                # Tool implementations
│   │   ├── my-tool.js
│   │   └── another-tool.js
│   ├── templates/            # Code templates (optional)
│   │   └── my-template/
│   └── utils/                # Utility functions
├── examples/                 # Usage examples (optional)
│   └── basic-usage/
├── docs/                     # Additional documentation (optional)
│   └── api-reference.md
└── tests/                    # Plugin tests
    ├── unit/
    └── integration/
```

## 🎯 Template Types

### Basic Plugin Template
Simple plugin structure for basic functionality:
- Single tool implementation
- Minimal configuration
- Basic documentation
- Simple test structure

### Advanced Plugin Template
Comprehensive plugin structure for complex functionality:
- Multiple tools and utilities
- Advanced configuration options
- Comprehensive documentation
- Full test suite with coverage

### Tool-Only Plugin Template
Lightweight template for single-purpose tools:
- Single tool focus
- Minimal dependencies
- Streamlined structure
- Quick development cycle

## 📚 Documentation

- **[Getting Started Guide](docs/getting-started.md)** - Quick start tutorial
- **[Plugin Structure Guide](docs/plugin-structure.md)** - Detailed structure explanation
- **[Tool Development Guide](docs/tool-development.md)** - How to create tools
- **[Best Practices Guide](docs/best-practices.md)** - Development best practices
- **[API Reference](docs/api-reference.md)** - Complete API documentation

## 🧪 Examples

This plugin includes several example plugins to demonstrate different patterns:

- **[Hello World](examples/hello-world/)** - Basic plugin example
- **[File Processor](examples/file-processor/)** - File manipulation plugin
- **[Data Transformer](examples/data-transformer/)** - Data processing plugin

## 🔧 Development

### Prerequisites
- Node.js 18.0.0 or higher
- npm or yarn package manager
- Git for version control

### Local Development
```bash
# Clone the repository
git clone https://github.com/polybucket/polybucket-starter-template.git
cd polybucket-starter-template

# Install dependencies
npm install

# Run in development mode
npm run dev

# Run tests
npm test

# Validate plugin
npm run validate
```

### Contributing
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Support

- **Documentation**: [GitHub Wiki](https://github.com/polybucket/polybucket-starter-template/wiki)
- **Issues**: [GitHub Issues](https://github.com/polybucket/polybucket-starter-template/issues)
- **Discussions**: [GitHub Discussions](https://github.com/polybucket/polybucket-starter-template/discussions)
- **Community**: [PolyBucket Discord](https://discord.gg/polybucket)

## 🙏 Acknowledgments

- PolyBucket community for feedback and contributions
- Open source tools and libraries that make this possible
- Plugin developers who inspire new features and improvements

---

**Made with ❤️ by the PolyBucket Team**
