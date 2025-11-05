/**
 * PolyBucket Starter Template Plugin
 * 
 * Main entry point for the PolyBucket Starter Template plugin.
 * This plugin provides development tools and templates for creating
 * PolyBucket plugins.
 */

import { createPluginTool } from './tools/create-plugin.js';
import { validatePluginTool } from './tools/validate-plugin.js';
import { testPluginTool } from './tools/test-plugin.js';
import { packagePluginTool } from './tools/package-plugin.js';
import { logger } from './utils/logger.js';
import config from '../polybucket.config.js';

/**
 * Plugin initialization function
 * Called when the plugin is loaded by PolyBucket
 */
export async function initialize(context) {
  logger.info('🚀 Initializing PolyBucket Starter Template plugin');
  
  try {
    // Register all tools with PolyBucket
    await context.registerTool('create-plugin', createPluginTool);
    await context.registerTool('validate-plugin', validatePluginTool);
    await context.registerTool('test-plugin', testPluginTool);
    await context.registerTool('package-plugin', packagePluginTool);
    
    logger.info('✅ All tools registered successfully');
    
    // Display welcome message
    if (context.isInteractive) {
      console.log('\n🎉 PolyBucket Starter Template loaded successfully!');
      console.log('📚 Available tools:');
      console.log('  • create-plugin    - Scaffold new plugins');
      console.log('  • validate-plugin  - Validate plugin structure');
      console.log('  • test-plugin      - Run plugin tests');
      console.log('  • package-plugin   - Package for distribution');
      console.log('\n💡 Run "polybucket create-plugin --help" to get started\n');
    }
    
    return {
      success: true,
      message: 'PolyBucket Starter Template plugin initialized successfully'
    };
    
  } catch (error) {
    logger.error('❌ Failed to initialize plugin:', error);
    return {
      success: false,
      error: error.message
    };
  }
}

/**
 * Plugin cleanup function
 * Called when the plugin is unloaded
 */
export async function cleanup(context) {
  logger.info('🧹 Cleaning up PolyBucket Starter Template plugin');
  
  try {
    // Unregister tools
    await context.unregisterTool('create-plugin');
    await context.unregisterTool('validate-plugin');
    await context.unregisterTool('test-plugin');
    await context.unregisterTool('package-plugin');
    
    logger.info('✅ Plugin cleanup completed');
    
    return {
      success: true,
      message: 'Plugin cleanup completed successfully'
    };
    
  } catch (error) {
    logger.error('❌ Failed to cleanup plugin:', error);
    return {
      success: false,
      error: error.message
    };
  }
}

/**
 * Plugin information
 */
export const pluginInfo = {
  name: config.name,
  version: config.version,
  description: config.description,
  author: 'PolyBucket Team',
  license: 'MIT',
  repository: 'https://github.com/polybucket/polybucket-starter-template',
  homepage: 'https://github.com/polybucket/polybucket-starter-template#readme',
  bugs: 'https://github.com/polybucket/polybucket-starter-template/issues',
  keywords: config.polybucket.tags,
  tools: Object.keys(config.tools),
  compatibility: config.compatibility
};

// Export tools for direct access
export {
  createPluginTool,
  validatePluginTool,
  testPluginTool,
  packagePluginTool
};
