# Hello World Plugin

A simple hello world plugin for PolyBucket that demonstrates basic plugin structure and functionality.

## 🚀 Features

- **Hello Tool**: Simple greeting functionality
- **Easy Integration**: Basic integration with PolyBucket
- **Example Code**: Perfect starting point for new plugins

## 📦 Installation

```bash
# Via marketplace (recommended)
polybucket install hello-world

# Direct from GitHub
git clone https://github.com/polybucket/hello-world.git
cd hello-world
npm install
```

## 🛠️ Usage

### Basic Usage

```bash
# Say hello
polybucket hello

# Say hello to someone
polybucket hello --name "John"

# Say hello with custom message
polybucket hello --message "Good morning"
```

### Examples

```bash
# Simple hello
polybucket hello

# Personalized greeting
polybucket hello --name "Alice" --message "Welcome to PolyBucket!"

# Get help
polybucket hello --help
```

## 🔧 Configuration

```json
{
  "plugins": {
    "hello-world": {
      "enabled": true,
      "options": {
        "defaultName": "World",
        "defaultMessage": "Hello"
      }
    }
  }
}
```

## 📚 API Reference

### helloTool(context)

Main tool function for greeting functionality.

**Parameters:**
- `context` (Object): Tool execution context
  - `args` (Array): Command line arguments
  - `options` (Object): Command line options
    - `name` (string): Name to greet
    - `message` (string): Custom greeting message

**Returns:**
- `Promise<Object>`: Tool execution result

## 🧪 Testing

```bash
npm test
```

## 🔧 Development

```bash
# Clone and setup
git clone https://github.com/polybucket/hello-world.git
cd hello-world
npm install

# Development mode
npm run dev

# Run tests
npm test
```

## 📄 License

MIT License - see [LICENSE](LICENSE) file for details.

---

**Made with ❤️ by PolyBucket Team**
