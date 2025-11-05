# Getting Started with PolyBucket Plugin Development

Welcome to PolyBucket plugin development! This guide will help you create your first plugin using the PolyBucket Starter Template.

## 🚀 Quick Start

### 1. Install the Starter Template

```bash
# Install via marketplace
polybucket install polybucket-starter-template

# Or clone directly
git clone https://github.com/polybucket/polybucket-starter-template.git
cd polybucket-starter-template
npm install
```

### 2. Create Your First Plugin

```bash
# Create a basic plugin
polybucket create-plugin my-awesome-plugin

# Or create with specific options
polybucket create-plugin my-plugin --template advanced --category development
```

### 3. Develop Your Plugin

```bash
# Navigate to your plugin
cd my-awesome-plugin

# Install dependencies
npm install

# Run in development mode
npm run dev

# Validate your plugin
polybucket validate-plugin .

# Test your plugin
polybucket test-plugin .
```

## 📁 Plugin Structure

Every PolyBucket plugin follows a standard structure:

```
my-plugin/
├── README.md                 # Plugin documentation
├── package.json              # Plugin metadata and dependencies
├── polybucket.config.js      # Plugin configuration (optional)
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

## 🛠️ Available Templates

### Basic Plugin Template
Perfect for simple plugins with single functionality:
- Single tool implementation
- Minimal configuration
- Basic documentation
- Simple test structure

### Advanced Plugin Template
Comprehensive structure for complex functionality:
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

## 🔧 Plugin Configuration

### package.json

The `package.json` file contains essential plugin metadata:

```json
{
  "name": "my-plugin",
  "version": "1.0.0",
  "description": "My awesome PolyBucket plugin",
  "main": "src/index.js",
  "polybucket": {
    "pluginType": "utilities",
    "version": "1.0.0",
    "compatibility": {
      "polybucket": ">=1.0.0"
    },
    "tools": ["my-tool"],
    "categories": ["utilities"],
    "tags": ["plugin", "utilities"]
  }
}
```

### polybucket.config.js (Optional)

For advanced configuration:

```javascript
export default {
  name: 'my-plugin',
  version: '1.0.0',
  tools: {
    'my-tool': {
      name: 'My Tool',
      description: 'Description of my tool',
      usage: 'polybucket my-tool <input> [options]',
      options: {
        verbose: {
          type: 'boolean',
          description: 'Enable verbose output',
          default: false
        }
      }
    }
  },
  hooks: {
    install: async (context) => {
      console.log('Plugin installed successfully!');
    }
  }
};
```

## 🧪 Testing Your Plugin

### Unit Tests

Create unit tests for individual functions:

```javascript
// tests/unit/my-tool.test.js
import { myTool } from '../../src/tools/my-tool.js';

describe('My Tool', () => {
  test('should process input correctly', async () => {
    const context = {
      args: ['test-input'],
      options: {}
    };
    
    const result = await myTool(context);
    
    expect(result.success).toBe(true);
    expect(result.result).toBe('processed-test-input');
  });
});
```

### Integration Tests

Test the complete plugin workflow:

```javascript
// tests/integration/plugin.test.js
import { initialize, cleanup } from '../../src/index.js';

describe('Plugin Integration', () => {
  test('should initialize successfully', async () => {
    const mockContext = {
      registerTool: jest.fn(),
      isInteractive: false
    };
    
    const result = await initialize(mockContext);
    
    expect(result.success).toBe(true);
    expect(mockContext.registerTool).toHaveBeenCalled();
  });
});
```

## 📦 Packaging Your Plugin

### Package for Distribution

```bash
# Package your plugin
polybucket package-plugin .

# Package for marketplace submission
polybucket package-plugin . --marketplace
```

### Marketplace Submission

1. **Validate your plugin**:
   ```bash
   polybucket validate-plugin . --strict
   ```

2. **Run all tests**:
   ```bash
   polybucket test-plugin . --coverage
   ```

3. **Package for marketplace**:
   ```bash
   polybucket package-plugin . --marketplace --format zip
   ```

4. **Submit to marketplace**:
   - Upload the generated package
   - Fill out marketplace metadata
   - Submit for review

## 📚 Best Practices

### Code Quality
- Use TypeScript for better type safety
- Follow consistent coding style
- Add comprehensive error handling
- Write meaningful commit messages

### Documentation
- Write clear README files
- Document all public APIs
- Include usage examples
- Provide troubleshooting guides

### Testing
- Write unit tests for all functions
- Include integration tests
- Aim for high code coverage
- Test error conditions

### Performance
- Optimize for speed and memory usage
- Use efficient algorithms
- Minimize dependencies
- Profile your code

## 🆘 Getting Help

- **Documentation**: Check the [Plugin Structure Guide](plugin-structure.md)
- **Examples**: Look at the [examples directory](../examples/)
- **Community**: Join our [Discord server](https://discord.gg/polybucket)
- **Issues**: Report bugs on [GitHub Issues](https://github.com/polybucket/polybucket-starter-template/issues)

## 🎯 Next Steps

1. **Explore Examples**: Check out the example plugins in the `examples/` directory
2. **Read Documentation**: Review the [Plugin Structure Guide](plugin-structure.md)
3. **Learn Tool Development**: Follow the [Tool Development Guide](tool-development.md)
4. **Follow Best Practices**: Read the [Best Practices Guide](best-practices.md)

Happy plugin development! 🚀
