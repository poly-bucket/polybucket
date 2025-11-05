/**
 * Template Engine
 * 
 * Handles template processing and variable substitution
 */

import Handlebars from 'handlebars';

export class TemplateEngine {
  constructor() {
    this.registerHelpers();
  }

  /**
   * Register Handlebars helpers
   */
  registerHelpers() {
    // Convert string to camelCase
    Handlebars.registerHelper('camelCase', (str) => {
      return str.replace(/-([a-z])/g, (g) => g[1].toUpperCase());
    });

    // Convert string to PascalCase
    Handlebars.registerHelper('pascalCase', (str) => {
      const camelCase = str.replace(/-([a-z])/g, (g) => g[1].toUpperCase());
      return camelCase.charAt(0).toUpperCase() + camelCase.slice(1);
    });

    // Convert string to kebab-case
    Handlebars.registerHelper('kebabCase', (str) => {
      return str.replace(/([A-Z])/g, '-$1').toLowerCase().replace(/^-/, '');
    });

    // Convert string to snake_case
    Handlebars.registerHelper('snakeCase', (str) => {
      return str.replace(/([A-Z])/g, '_$1').toLowerCase().replace(/^_/, '');
    });

    // Convert string to title case
    Handlebars.registerHelper('titleCase', (str) => {
      return str.replace(/-/g, ' ').replace(/\b\w/g, l => l.toUpperCase());
    });

    // Format date
    Handlebars.registerHelper('formatDate', (date) => {
      return new Date(date).toISOString().split('T')[0];
    });

    // Generate UUID
    Handlebars.registerHelper('uuid', () => {
      return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
        const r = Math.random() * 16 | 0;
        const v = c === 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
      });
    });

    // Conditional helper
    Handlebars.registerHelper('if_eq', function(a, b, options) {
      if (a === b) {
        return options.fn(this);
      } else {
        return options.inverse(this);
      }
    });

    // Array helper
    Handlebars.registerHelper('each_with_index', function(array, options) {
      let result = '';
      for (let i = 0; i < array.length; i++) {
        result += options.fn({ ...this, index: i, item: array[i] });
      }
      return result;
    });
  }

  /**
   * Process template content with data
   */
  process(template, data) {
    try {
      const compiledTemplate = Handlebars.compile(template);
      return compiledTemplate(data);
    } catch (error) {
      throw new Error(`Template processing failed: ${error.message}`);
    }
  }

  /**
   * Process template file
   */
  async processFile(templatePath, outputPath, data) {
    const fs = await import('fs-extra');
    const template = await fs.readFile(templatePath, 'utf8');
    const processed = this.process(template, data);
    await fs.writeFile(outputPath, processed);
  }

  /**
   * Get template variables from content
   */
  getVariables(template) {
    const variables = new Set();
    const regex = /\{\{([^}]+)\}\}/g;
    let match;
    
    while ((match = regex.exec(template)) !== null) {
      const variable = match[1].trim();
      // Extract variable name (remove helpers and modifiers)
      const varName = variable.split(' ')[0].split('.')[0];
      if (!varName.includes('(') && !varName.includes(')')) {
        variables.add(varName);
      }
    }
    
    return Array.from(variables);
  }

  /**
   * Validate template data
   */
  validateData(template, data) {
    const requiredVariables = this.getVariables(template);
    const missingVariables = requiredVariables.filter(variable => 
      !(variable in data) || data[variable] === undefined
    );
    
    if (missingVariables.length > 0) {
      throw new Error(`Missing required template variables: ${missingVariables.join(', ')}`);
    }
    
    return true;
  }
}
