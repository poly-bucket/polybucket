/**
 * PolyBucket Starter Template Plugin Configuration
 * 
 * This configuration defines how the plugin integrates with PolyBucket
 * and what tools it provides to users.
 */

export default {
  // Plugin metadata
  name: 'polybucket-starter-template',
  version: '1.0.0',
  description: 'A comprehensive starter template and development toolkit for creating PolyBucket plugins',
  
  // Plugin type and category
  type: 'development-tools',
  category: 'development',
  
  // Compatibility requirements
  compatibility: {
    polybucket: '>=1.0.0',
    node: '>=18.0.0'
  },
  
  // Available tools
  tools: {
    'create-plugin': {
      name: 'Create Plugin',
      description: 'Scaffolds a new PolyBucket plugin with proper structure and configuration',
      category: 'scaffolding',
      usage: 'polybucket create-plugin <name> [options]',
      options: {
        template: {
          type: 'string',
          description: 'Template type to use',
          choices: ['basic', 'advanced', 'tool-only'],
          default: 'basic'
        },
        category: {
          type: 'string',
          description: 'Plugin category',
          choices: ['development', 'productivity', 'utilities', 'data', 'ui'],
          default: 'utilities'
        },
        description: {
          type: 'string',
          description: 'Plugin description',
          default: 'A PolyBucket plugin'
        },
        author: {
          type: 'string',
          description: 'Plugin author name',
          default: 'Plugin Developer'
        },
        output: {
          type: 'string',
          description: 'Output directory',
          default: './'
        }
      },
      examples: [
        'polybucket create-plugin my-plugin',
        'polybucket create-plugin my-plugin --template advanced --category development',
        'polybucket create-plugin my-plugin --description "My awesome plugin" --author "John Doe"'
      ]
    },
    
    'validate-plugin': {
      name: 'Validate Plugin',
      description: 'Validates plugin structure, configuration, and code quality',
      category: 'validation',
      usage: 'polybucket validate-plugin <path> [options]',
      options: {
        strict: {
          type: 'boolean',
          description: 'Enable strict validation mode',
          default: false
        },
        fix: {
          type: 'boolean',
          description: 'Automatically fix common issues',
          default: false
        },
        format: {
          type: 'string',
          description: 'Output format',
          choices: ['text', 'json', 'markdown'],
          default: 'text'
        }
      },
      examples: [
        'polybucket validate-plugin ./my-plugin',
        'polybucket validate-plugin ./my-plugin --strict --fix',
        'polybucket validate-plugin ./my-plugin --format json'
      ]
    },
    
    'test-plugin': {
      name: 'Test Plugin',
      description: 'Runs plugin tests and validates functionality',
      category: 'testing',
      usage: 'polybucket test-plugin <path> [options]',
      options: {
        coverage: {
          type: 'boolean',
          description: 'Generate code coverage report',
          default: false
        },
        watch: {
          type: 'boolean',
          description: 'Watch mode for continuous testing',
          default: false
        },
        verbose: {
          type: 'boolean',
          description: 'Verbose output',
          default: false
        }
      },
      examples: [
        'polybucket test-plugin ./my-plugin',
        'polybucket test-plugin ./my-plugin --coverage --watch',
        'polybucket test-plugin ./my-plugin --verbose'
      ]
    },
    
    'package-plugin': {
      name: 'Package Plugin',
      description: 'Packages plugin for distribution and marketplace submission',
      category: 'packaging',
      usage: 'polybucket package-plugin <path> [options]',
      options: {
        output: {
          type: 'string',
          description: 'Output directory for package',
          default: './dist'
        },
        format: {
          type: 'string',
          description: 'Package format',
          choices: ['zip', 'tar', 'directory'],
          default: 'zip'
        },
        marketplace: {
          type: 'boolean',
          description: 'Prepare for marketplace submission',
          default: false
        }
      },
      examples: [
        'polybucket package-plugin ./my-plugin',
        'polybucket package-plugin ./my-plugin --marketplace --format zip',
        'polybucket package-plugin ./my-plugin --output ./packages'
      ]
    }
  },
  
  // Plugin hooks
  hooks: {
    // Called when plugin is installed
    install: async (context) => {
      console.log('🎉 PolyBucket Starter Template installed successfully!');
      console.log('📚 Run "polybucket create-plugin --help" to get started');
    },
    
    // Called when plugin is uninstalled
    uninstall: async (context) => {
      console.log('👋 PolyBucket Starter Template uninstalled');
    },
    
    // Called when plugin is updated
    update: async (context) => {
      console.log('🔄 PolyBucket Starter Template updated');
    }
  },
  
  // Plugin permissions
  permissions: {
    filesystem: {
      read: ['**/*'],
      write: ['**/*']
    },
    network: {
      fetch: true
    }
  },
  
  // Plugin dependencies
  dependencies: {
    required: [],
    optional: ['jest', 'eslint', 'prettier']
  },
  
  // Plugin configuration schema
  configSchema: {
    type: 'object',
    properties: {
      defaultTemplate: {
        type: 'string',
        default: 'basic',
        description: 'Default template type for new plugins'
      },
      autoValidate: {
        type: 'boolean',
        default: true,
        description: 'Automatically validate plugins after creation'
      },
      includeTests: {
        type: 'boolean',
        default: true,
        description: 'Include test files in generated plugins'
      },
      includeDocs: {
        type: 'boolean',
        default: true,
        description: 'Include documentation in generated plugins'
      }
    }
  }
};
