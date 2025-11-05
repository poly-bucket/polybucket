/**
 * Logger utility for the PolyBucket Starter Template plugin
 */

import chalk from 'chalk';

class Logger {
  constructor() {
    this.levels = {
      error: 0,
      warn: 1,
      info: 2,
      debug: 3
    };
    this.currentLevel = this.levels.info;
  }

  setLevel(level) {
    this.currentLevel = this.levels[level] || this.levels.info;
  }

  formatMessage(level, message, ...args) {
    const timestamp = new Date().toISOString();
    const levelColor = {
      error: chalk.red,
      warn: chalk.yellow,
      info: chalk.blue,
      debug: chalk.gray
    };
    
    const coloredLevel = levelColor[level](`[${level.toUpperCase()}]`);
    const coloredMessage = level === 'error' ? chalk.red(message) : message;
    
    return `${chalk.gray(timestamp)} ${coloredLevel} ${coloredMessage}`;
  }

  log(level, message, ...args) {
    if (this.levels[level] <= this.currentLevel) {
      console.log(this.formatMessage(level, message, ...args));
    }
  }

  error(message, ...args) {
    this.log('error', message, ...args);
  }

  warn(message, ...args) {
    this.log('warn', message, ...args);
  }

  info(message, ...args) {
    this.log('info', message, ...args);
  }

  debug(message, ...args) {
    this.log('debug', message, ...args);
  }

  success(message, ...args) {
    console.log(chalk.green(`✅ ${message}`), ...args);
  }

  failure(message, ...args) {
    console.log(chalk.red(`❌ ${message}`), ...args);
  }

  warning(message, ...args) {
    console.log(chalk.yellow(`⚠️  ${message}`), ...args);
  }

  info(message, ...args) {
    console.log(chalk.blue(`ℹ️  ${message}`), ...args);
  }
}

export const logger = new Logger();
