/**
 * Create Plugin Tool
 * 
 * Scaffolds a new PolyBucket plugin with proper structure and configuration
 */

import inquirer from 'inquirer';
import chalk from 'chalk';
import ora from 'ora';
import { FileSystemUtils } from '../utils/filesystem.js';
import { logger } from '../utils/logger.js';
import { TemplateEngine } from '../utils/template-engine.js';

export async function createPluginTool(context) {
  const { args, options } = context;
  
  try {
    logger.info('🚀 Starting plugin creation process');
    
    // Get plugin name from arguments
    const pluginName = args[0];
    if (!pluginName) {
      logger.error('Plugin name is required');
      console.log('\nUsage: polybucket create-plugin <name> [options]');
      console.log('\nOptions:');
      console.log('  --template <type>     Template type (basic, advanced, tool-only)');
      console.log('  --category <cat>      Plugin category');
      console.log('  --description <desc> Plugin description');
      console.log('  --author <name>       Plugin author');
      console.log('  --output <dir>        Output directory');
      return { success: false, error: 'Plugin name is required' };
    }

    // Validate plugin name
    if (!isValidPluginName(pluginName)) {
      logger.error('Invalid plugin name. Use lowercase letters, numbers, and hyphens only.');
      return { success: false, error: 'Invalid plugin name' };
    }

    // Collect plugin information
    const pluginInfo = await collectPluginInfo(pluginName, options);
    
    // Determine output directory
    const outputDir = FileSystemUtils.joinPath(options.output || './', pluginName);
    
    // Check if directory already exists
    if (await FileSystemUtils.exists(outputDir)) {
      logger.warn(`Directory ${outputDir} already exists`);
      const { overwrite } = await inquirer.prompt([{
        type: 'confirm',
        name: 'overwrite',
        message: 'Do you want to overwrite the existing directory?',
        default: false
      }]);
      
      if (!overwrite) {
        logger.info('Plugin creation cancelled');
        return { success: false, error: 'Directory already exists' };
      }
    }

    // Create plugin structure
    const spinner = ora('Creating plugin structure...').start();
    
    try {
      await createPluginStructure(outputDir, pluginInfo);
      spinner.succeed('Plugin structure created');
      
      // Generate files from templates
      const templateSpinner = ora('Generating files from templates...').start();
      await generatePluginFiles(outputDir, pluginInfo);
      templateSpinner.succeed('Files generated from templates');
      
      // Install dependencies if package.json exists
      const packageJsonPath = FileSystemUtils.joinPath(outputDir, 'package.json');
      if (await FileSystemUtils.exists(packageJsonPath)) {
        const installSpinner = ora('Installing dependencies...').start();
        await installDependencies(outputDir);
        installSpinner.succeed('Dependencies installed');
      }
      
      // Display success message
      logger.success(`Plugin "${pluginName}" created successfully!`);
      console.log(`\n📁 Location: ${outputDir}`);
      console.log(`\n📚 Next steps:`);
      console.log(`  1. cd ${pluginName}`);
      console.log(`  2. polybucket validate-plugin .`);
      console.log(`  3. polybucket test-plugin .`);
      console.log(`  4. Start developing your plugin!`);
      
      return {
        success: true,
        message: `Plugin "${pluginName}" created successfully`,
        outputDir,
        pluginInfo
      };
      
    } catch (error) {
      spinner.fail('Failed to create plugin structure');
      throw error;
    }
    
  } catch (error) {
    logger.error('Failed to create plugin:', error.message);
    return {
      success: false,
      error: error.message
    };
  }
}

/**
 * Validate plugin name
 */
function isValidPluginName(name) {
  return /^[a-z0-9-]+$/.test(name) && name.length > 0 && name.length <= 50;
}

/**
 * Collect plugin information from user
 */
async function collectPluginInfo(pluginName, options) {
  const questions = [];
  
  // Template selection
  if (!options.template) {
    questions.push({
      type: 'list',
      name: 'template',
      message: 'Choose template type:',
      choices: [
        { name: 'Basic Plugin - Simple structure for basic functionality', value: 'basic' },
        { name: 'Advanced Plugin - Comprehensive structure for complex functionality', value: 'advanced' },
        { name: 'Tool-Only Plugin - Lightweight template for single-purpose tools', value: 'tool-only' }
      ],
      default: 'basic'
    });
  }
  
  // Category selection
  if (!options.category) {
    questions.push({
      type: 'list',
      name: 'category',
      message: 'Choose plugin category:',
      choices: [
        { name: 'Development - Tools for developers', value: 'development' },
        { name: 'Productivity - Tools to improve workflow', value: 'productivity' },
        { name: 'Utilities - General utility tools', value: 'utilities' },
        { name: 'Data - Data processing and manipulation', value: 'data' },
        { name: 'UI - User interface components', value: 'ui' }
      ],
      default: 'utilities'
    });
  }
  
  // Description
  if (!options.description) {
    questions.push({
      type: 'input',
      name: 'description',
      message: 'Plugin description:',
      default: `A PolyBucket plugin for ${pluginName.replace(/-/g, ' ')}`,
      validate: (input) => input.length > 0 || 'Description is required'
    });
  }
  
  // Author
  if (!options.author) {
    questions.push({
      type: 'input',
      name: 'author',
      message: 'Plugin author:',
      default: 'Plugin Developer',
      validate: (input) => input.length > 0 || 'Author is required'
    });
  }
  
  // Ask questions if needed
  const answers = questions.length > 0 ? await inquirer.prompt(questions) : {};
  
  return {
    name: pluginName,
    template: options.template || answers.template,
    category: options.category || answers.category,
    description: options.description || answers.description,
    author: options.author || answers.author,
    version: '1.0.0',
    license: 'MIT'
  };
}

/**
 * Create plugin directory structure
 */
async function createPluginStructure(outputDir, pluginInfo) {
  await FileSystemUtils.ensureDir(outputDir);
  
  // Create main directories
  const directories = [
    'src',
    'src/tools',
    'src/utils',
    'tests',
    'tests/unit',
    'tests/integration',
    'docs',
    'examples'
  ];
  
  for (const dir of directories) {
    await FileSystemUtils.ensureDir(FileSystemUtils.joinPath(outputDir, dir));
  }
  
  // Add template-specific directories
  if (pluginInfo.template === 'advanced') {
    await FileSystemUtils.ensureDir(FileSystemUtils.joinPath(outputDir, 'src/templates'));
    await FileSystemUtils.ensureDir(FileSystemUtils.joinPath(outputDir, 'src/config'));
  }
}

/**
 * Generate plugin files from templates
 */
async function generatePluginFiles(outputDir, pluginInfo) {
  const templateEngine = new TemplateEngine();
  const templateDir = FileSystemUtils.joinPath(FileSystemUtils.getTemplatesDir(), pluginInfo.template);
  
  // Copy template files
  await FileSystemUtils.copyDir(templateDir, outputDir, {
    filter: (src) => {
      // Skip template files that should be processed
      return !src.endsWith('.hbs') && !src.endsWith('.template');
    }
  });
  
  // Process template files
  const templateFiles = await FileSystemUtils.findFiles(outputDir, /\.(hbs|template)$/);
  
  for (const templateFile of templateFiles) {
    const content = await FileSystemUtils.readFile(templateFile);
    const processedContent = templateEngine.process(content, pluginInfo);
    
    // Write processed content to new file
    const outputFile = templateFile.replace(/\.(hbs|template)$/, '');
    await FileSystemUtils.writeFile(outputFile, processedContent);
    
    // Remove template file
    await FileSystemUtils.remove(templateFile);
  }
}

/**
 * Install dependencies
 */
async function installDependencies(outputDir) {
  const { exec } = await import('child_process');
  const { promisify } = await import('util');
  const execAsync = promisify(exec);
  
  try {
    await execAsync('npm install', { cwd: outputDir });
  } catch (error) {
    logger.warn('Failed to install dependencies:', error.message);
  }
}
