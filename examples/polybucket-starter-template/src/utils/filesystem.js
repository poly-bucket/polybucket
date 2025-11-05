/**
 * File system utilities for the PolyBucket Starter Template plugin
 */

import fs from 'fs-extra';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

export class FileSystemUtils {
  /**
   * Get the plugin root directory
   */
  static getPluginRoot() {
    return path.resolve(__dirname, '../../');
  }

  /**
   * Get the templates directory
   */
  static getTemplatesDir() {
    return path.resolve(this.getPluginRoot(), 'src/templates');
  }

  /**
   * Get the examples directory
   */
  static getExamplesDir() {
    return path.resolve(this.getPluginRoot(), 'examples');
  }

  /**
   * Ensure directory exists
   */
  static async ensureDir(dirPath) {
    await fs.ensureDir(dirPath);
  }

  /**
   * Copy directory recursively
   */
  static async copyDir(src, dest, options = {}) {
    await fs.copy(src, dest, {
      overwrite: true,
      ...options
    });
  }

  /**
   * Copy file
   */
  static async copyFile(src, dest) {
    await fs.copy(src, dest);
  }

  /**
   * Read file content
   */
  static async readFile(filePath, encoding = 'utf8') {
    return await fs.readFile(filePath, encoding);
  }

  /**
   * Write file content
   */
  static async writeFile(filePath, content, encoding = 'utf8') {
    await fs.ensureDir(path.dirname(filePath));
    await fs.writeFile(filePath, content, encoding);
  }

  /**
   * Check if file exists
   */
  static async exists(filePath) {
    return await fs.pathExists(filePath);
  }

  /**
   * Get file stats
   */
  static async stat(filePath) {
    return await fs.stat(filePath);
  }

  /**
   * List directory contents
   */
  static async readdir(dirPath) {
    return await fs.readdir(dirPath);
  }

  /**
   * Remove file or directory
   */
  static async remove(filePath) {
    await fs.remove(filePath);
  }

  /**
   * Find files matching pattern
   */
  static async findFiles(dirPath, pattern) {
    const files = [];
    
    const walkDir = async (dir) => {
      const entries = await fs.readdir(dir, { withFileTypes: true });
      
      for (const entry of entries) {
        const fullPath = path.join(dir, entry.name);
        
        if (entry.isDirectory()) {
          await walkDir(fullPath);
        } else if (entry.isFile() && entry.name.match(pattern)) {
          files.push(fullPath);
        }
      }
    };
    
    await walkDir(dirPath);
    return files;
  }

  /**
   * Get relative path from base
   */
  static getRelativePath(from, to) {
    return path.relative(from, to);
  }

  /**
   * Resolve path
   */
  static resolvePath(...paths) {
    return path.resolve(...paths);
  }

  /**
   * Join paths
   */
  static joinPath(...paths) {
    return path.join(...paths);
  }
}
