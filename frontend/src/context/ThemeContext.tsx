import React, { createContext, useContext, useEffect, useState, ReactNode } from 'react';

export interface ThemeColors {
  primary: string;
  primaryLight: string;
  primaryDark: string;
  secondary: string;
  secondaryLight: string;
  secondaryDark: string;
  accent: string;
  accentLight: string;
  accentDark: string;
  bgPrimary: string;
  bgSecondary: string;
  bgTertiary: string;
}

export interface ThemeContextType {
  colors: ThemeColors;
  updateColors: (newColors: Partial<ThemeColors>) => void;
  resetToDefaults: () => void;
  isCustomized: boolean;
}

const defaultColors: ThemeColors = {
  primary: '#6366f1',
  primaryLight: '#818cf8',
  primaryDark: '#4f46e5',
  secondary: '#8b5cf6',
  secondaryLight: '#a78bfa',
  secondaryDark: '#7c3aed',
  accent: '#06b6d4',
  accentLight: '#22d3ee',
  accentDark: '#0891b2',
  bgPrimary: '#0f0f23',
  bgSecondary: '#1a1a2e',
  bgTertiary: '#16213e',
};

const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

export const useTheme = () => {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error('useTheme must be used within a ThemeProvider');
  }
  return context;
};

interface ThemeProviderProps {
  children: ReactNode;
}

export const ThemeProvider: React.FC<ThemeProviderProps> = ({ children }) => {
  const [colors, setColors] = useState<ThemeColors>(defaultColors);
  const [isCustomized, setIsCustomized] = useState(false);

  // Load saved colors from localStorage on mount
  useEffect(() => {
    const savedColors = localStorage.getItem('polybucket-theme-colors');
    if (savedColors) {
      try {
        const parsedColors = JSON.parse(savedColors);
        setColors(parsedColors);
        setIsCustomized(true);
        applyColorsToCSS(parsedColors);
      } catch (error) {
        console.error('Failed to parse saved theme colors:', error);
      }
    }
  }, []);

  // Apply colors to CSS custom properties
  const applyColorsToCSS = (themeColors: ThemeColors) => {
    const root = document.documentElement;
    
    root.style.setProperty('--lg-primary', themeColors.primary);
    root.style.setProperty('--lg-primary-light', themeColors.primaryLight);
    root.style.setProperty('--lg-primary-dark', themeColors.primaryDark);
    root.style.setProperty('--lg-secondary', themeColors.secondary);
    root.style.setProperty('--lg-secondary-light', themeColors.secondaryLight);
    root.style.setProperty('--lg-secondary-dark', themeColors.secondaryDark);
    root.style.setProperty('--lg-accent', themeColors.accent);
    root.style.setProperty('--lg-accent-light', themeColors.accentLight);
    root.style.setProperty('--lg-accent-dark', themeColors.accentDark);
    root.style.setProperty('--lg-bg-primary', themeColors.bgPrimary);
    root.style.setProperty('--lg-bg-secondary', themeColors.bgSecondary);
    root.style.setProperty('--lg-bg-tertiary', themeColors.bgTertiary);
  };

  const updateColors = (newColors: Partial<ThemeColors>) => {
    const updatedColors = { ...colors, ...newColors };
    setColors(updatedColors);
    setIsCustomized(true);
    
    // Save to localStorage
    localStorage.setItem('polybucket-theme-colors', JSON.stringify(updatedColors));
    
    // Apply to CSS
    applyColorsToCSS(updatedColors);
  };

  const resetToDefaults = () => {
    setColors(defaultColors);
    setIsCustomized(false);
    
    // Remove from localStorage
    localStorage.removeItem('polybucket-theme-colors');
    
    // Reset CSS properties
    const root = document.documentElement;
    root.style.removeProperty('--lg-primary');
    root.style.removeProperty('--lg-primary-light');
    root.style.removeProperty('--lg-primary-dark');
    root.style.removeProperty('--lg-secondary');
    root.style.removeProperty('--lg-secondary-light');
    root.style.removeProperty('--lg-secondary-dark');
    root.style.removeProperty('--lg-accent');
    root.style.removeProperty('--lg-accent-light');
    root.style.removeProperty('--lg-accent-dark');
    root.style.removeProperty('--lg-bg-primary');
    root.style.removeProperty('--lg-bg-secondary');
    root.style.removeProperty('--lg-bg-tertiary');
  };

  // Apply colors whenever they change
  useEffect(() => {
    applyColorsToCSS(colors);
  }, [colors]);

  const value: ThemeContextType = {
    colors,
    updateColors,
    resetToDefaults,
    isCustomized,
  };

  return (
    <ThemeContext.Provider value={value}>
      {children}
    </ThemeContext.Provider>
  );
};

export default ThemeProvider; 