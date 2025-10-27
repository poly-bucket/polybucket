import React, { createContext, useContext, useEffect, useState } from 'react';
import { usePlugin } from '../PluginAPI';

interface DarkThemeContextType {
  isDark: boolean;
  toggleTheme: () => void;
  accentColor: string;
  setAccentColor: (color: string) => void;
  enableAnimations: boolean;
  setEnableAnimations: (enabled: boolean) => void;
  fontFamily: string;
  setFontFamily: (family: string) => void;
}

const DarkThemeContext = createContext<DarkThemeContextType | undefined>(undefined);

interface DarkThemeProviderProps {
  children: React.ReactNode;
}

export const DarkThemeProvider: React.FC<DarkThemeProviderProps> = ({ children }) => {
  const plugin = usePlugin('dark-theme-plugin');
  const [isDark, setIsDark] = useState(false);
  const [accentColor, setAccentColor] = useState('#007bff');
  const [enableAnimations, setEnableAnimations] = useState(true);
  const [fontFamily, setFontFamily] = useState('system-ui, -apple-system, sans-serif');

  useEffect(() => {
    // Load saved theme settings
    const savedSettings = plugin.storage.get('darkThemeSettings');
    if (savedSettings) {
      setAccentColor(savedSettings.accentColor || '#007bff');
      setEnableAnimations(savedSettings.enableAnimations !== false);
      setFontFamily(savedSettings.fontFamily || 'system-ui, -apple-system, sans-serif');
    }
  }, [plugin]);

  useEffect(() => {
    if (isDark) {
      applyDarkTheme();
    } else {
      removeDarkTheme();
    }
  }, [isDark, accentColor, enableAnimations, fontFamily]);

  const applyDarkTheme = () => {
    const root = document.documentElement;
    
    // Apply CSS variables
    root.style.setProperty('--primary-color', accentColor);
    root.style.setProperty('--secondary-color', '#6c757d');
    root.style.setProperty('--background-color', '#1a1a1a');
    root.style.setProperty('--surface-color', '#2d2d2d');
    root.style.setProperty('--text-color', '#ffffff');
    root.style.setProperty('--text-muted', '#b0b0b0');
    root.style.setProperty('--border-color', '#404040');
    root.style.setProperty('--shadow-color', 'rgba(0, 0, 0, 0.3)');
    root.style.setProperty('--success-color', '#28a745');
    root.style.setProperty('--warning-color', '#ffc107');
    root.style.setProperty('--danger-color', '#dc3545');
    root.style.setProperty('--info-color', '#17a2b8');
    root.style.setProperty('--font-family', fontFamily);
    
    // Add dark theme class
    root.classList.add('dark-theme');
    
    // Apply animations setting
    if (enableAnimations) {
      root.classList.add('theme-animations');
    } else {
      root.classList.remove('theme-animations');
    }

    // Save settings
    plugin.storage.set('darkThemeSettings', {
      accentColor,
      enableAnimations,
      fontFamily
    });
  };

  const removeDarkTheme = () => {
    const root = document.documentElement;
    
    // Remove CSS variables
    root.style.removeProperty('--primary-color');
    root.style.removeProperty('--secondary-color');
    root.style.removeProperty('--background-color');
    root.style.removeProperty('--surface-color');
    root.style.removeProperty('--text-color');
    root.style.removeProperty('--text-muted');
    root.style.removeProperty('--border-color');
    root.style.removeProperty('--shadow-color');
    root.style.removeProperty('--success-color');
    root.style.removeProperty('--warning-color');
    root.style.removeProperty('--danger-color');
    root.style.removeProperty('--info-color');
    root.style.removeProperty('--font-family');
    
    // Remove dark theme class
    root.classList.remove('dark-theme');
    root.classList.remove('theme-animations');
  };

  const toggleTheme = () => {
    setIsDark(!isDark);
  };

  const contextValue: DarkThemeContextType = {
    isDark,
    toggleTheme,
    accentColor,
    setAccentColor,
    enableAnimations,
    setEnableAnimations,
    fontFamily,
    setFontFamily
  };

  return (
    <DarkThemeContext.Provider value={contextValue}>
      {children}
    </DarkThemeContext.Provider>
  );
};

export const useDarkTheme = () => {
  const context = useContext(DarkThemeContext);
  if (!context) {
    throw new Error('useDarkTheme must be used within DarkThemeProvider');
  }
  return context;
};

// Theme Toggle Component
export const ThemeToggle: React.FC = () => {
  const { isDark, toggleTheme } = useDarkTheme();

  return (
    <button
      onClick={toggleTheme}
      className="inline-flex items-center px-3 py-2 border border-gray-300 shadow-sm text-sm leading-4 font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 dark:bg-gray-800 dark:text-white dark:border-gray-600 dark:hover:bg-gray-700"
      title={isDark ? 'Switch to light theme' : 'Switch to dark theme'}
    >
      {isDark ? (
        <svg className="h-4 w-4" fill="currentColor" viewBox="0 0 20 20">
          <path fillRule="evenodd" d="M10 2a1 1 0 011 1v1a1 1 0 11-2 0V3a1 1 0 011-1zm4 8a4 4 0 11-8 0 4 4 0 018 0zm-.464 4.95l.707.707a1 1 0 001.414-1.414l-.707-.707a1 1 0 00-1.414 1.414zm2.12-10.607a1 1 0 010 1.414l-.706.707a1 1 0 11-1.414-1.414l.707-.707a1 1 0 011.414 0zM17 11a1 1 0 100-2h-1a1 1 0 100 2h1zm-7 4a1 1 0 011 1v1a1 1 0 11-2 0v-1a1 1 0 011-1zM5.05 6.464A1 1 0 106.465 5.05l-.708-.707a1 1 0 00-1.414 1.414l.707.707zm1.414 8.486l-.707.707a1 1 0 01-1.414-1.414l.707-.707a1 1 0 011.414 1.414zM4 11a1 1 0 100-2H3a1 1 0 000 2h1z" clipRule="evenodd" />
        </svg>
      ) : (
        <svg className="h-4 w-4" fill="currentColor" viewBox="0 0 20 20">
          <path d="M17.293 13.293A8 8 0 016.707 2.707a8.001 8.001 0 1010.586 10.586z" />
        </svg>
      )}
    </button>
  );
};

// Theme Settings Component
export const ThemeSettings: React.FC = () => {
  const { 
    accentColor, 
    setAccentColor, 
    enableAnimations, 
    setEnableAnimations, 
    fontFamily, 
    setFontFamily 
  } = useDarkTheme();

  return (
    <div className="space-y-4 p-4 bg-white dark:bg-gray-800 rounded-lg shadow">
      <h3 className="text-lg font-medium text-gray-900 dark:text-white">Dark Theme Settings</h3>
      
      <div>
        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
          Accent Color
        </label>
        <div className="flex items-center space-x-2">
          <input
            type="color"
            value={accentColor}
            onChange={(e) => setAccentColor(e.target.value)}
            className="h-8 w-16 border border-gray-300 rounded cursor-pointer"
          />
          <input
            type="text"
            value={accentColor}
            onChange={(e) => setAccentColor(e.target.value)}
            className="flex-1 px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
          />
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
          Font Family
        </label>
        <select
          value={fontFamily}
          onChange={(e) => setFontFamily(e.target.value)}
          className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:border-gray-600 dark:text-white"
        >
          <option value="system-ui, -apple-system, sans-serif">System Default</option>
          <option value="Inter, sans-serif">Inter</option>
          <option value="Roboto, sans-serif">Roboto</option>
          <option value="Open Sans, sans-serif">Open Sans</option>
          <option value="Lato, sans-serif">Lato</option>
        </select>
      </div>

      <div className="flex items-center">
        <input
          type="checkbox"
          id="enableAnimations"
          checked={enableAnimations}
          onChange={(e) => setEnableAnimations(e.target.checked)}
          className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
        />
        <label htmlFor="enableAnimations" className="ml-2 block text-sm text-gray-700 dark:text-gray-300">
          Enable smooth animations and transitions
        </label>
      </div>
    </div>
  );
};

export default DarkThemeProvider;
