import React, { useState, useEffect } from 'react';
import { useTheme, ThemeColors } from '../../context/ThemeContext';
import { useAppSelector } from '../../utils/hooks';

interface ColorPickerProps {
  label: string;
  color: string;
  onChange: (color: string) => void;
  description?: string;
}

const ColorPicker: React.FC<ColorPickerProps> = ({ label, color, onChange, description }) => (
  <div className="lg-card p-4">
    <div className="flex items-center justify-between mb-2">
      <label className="text-sm font-medium text-white">{label}</label>
      <div className="flex items-center gap-2">
        <div 
          className="w-6 h-6 rounded border border-gray-600"
          style={{ backgroundColor: color }}
        />
        <input
          type="color"
          value={color}
          onChange={(e) => onChange(e.target.value)}
          className="w-6 h-6 border border-gray-600 rounded cursor-pointer"
        />
      </div>
    </div>
    <input
      type="text"
      value={color}
      onChange={(e) => onChange(e.target.value)}
      className="lg-input text-xs"
      placeholder="#000000"
    />
    {description && (
      <p className="text-xs text-gray-400 mt-1">{description}</p>
    )}
  </div>
);

const ThemeCustomization: React.FC = () => {
  const { user } = useAppSelector((state) => state.auth);
  const { colors, updateColors, resetToDefaults, isCustomized } = useTheme();
  const [localColors, setLocalColors] = useState<ThemeColors>(colors);
  const [isSaving, setIsSaving] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // Load theme settings from backend on mount
  useEffect(() => {
    loadThemeSettings();
  }, []);

  const loadThemeSettings = async () => {
    try {
      setIsLoading(true);
      
      // Get current theme configuration from the extensible theme system
      const configResponse = await fetch('http://localhost:11666/api/system-settings/extensible-theme/configuration', {
        headers: {
          'Authorization': `Bearer ${user?.accessToken}`,
          'Content-Type': 'application/json'
        }
      });

      if (configResponse.ok) {
        const configData = await configResponse.json();
        const backendColors: ThemeColors = {
          primary: configData.primaryColor || '#6366f1',
          primaryLight: configData.primaryLightColor || '#818cf8',
          primaryDark: configData.primaryDarkColor || '#4f46e5',
          secondary: configData.secondaryColor || '#8b5cf6',
          secondaryLight: configData.secondaryLightColor || '#a78bfa',
          secondaryDark: configData.secondaryDarkColor || '#7c3aed',
          accent: configData.accentColor || '#06b6d4',
          accentLight: configData.accentLightColor || '#22d3ee',
          accentDark: configData.accentDarkColor || '#0891b2',
          bgPrimary: configData.backgroundPrimaryColor || '#0f0f23',
          bgSecondary: configData.backgroundSecondaryColor || '#1a1a2e',
          bgTertiary: configData.backgroundTertiaryColor || '#16213e',
        };
        
        setLocalColors(backendColors);
        updateColors(backendColors);
      } else {
        console.warn('Failed to load theme settings from backend, using defaults');
      }
    } catch (error) {
      console.error('Error loading theme settings:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleColorChange = (key: keyof ThemeColors, value: string) => {
    setLocalColors(prev => ({ ...prev, [key]: value }));
  };

  const handleSave = async () => {
    setIsSaving(true);
    setError('');
    setSuccess('');
    
    try {
      const response = await fetch('http://localhost:11666/api/system-settings/extensible-theme/configuration', {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${user?.accessToken}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          primaryColor: localColors.primary,
          primaryLightColor: localColors.primaryLight,
          primaryDarkColor: localColors.primaryDark,
          secondaryColor: localColors.secondary,
          secondaryLightColor: localColors.secondaryLight,
          secondaryDarkColor: localColors.secondaryDark,
          accentColor: localColors.accent,
          accentLightColor: localColors.accentLight,
          accentDarkColor: localColors.accentDark,
          backgroundPrimaryColor: localColors.bgPrimary,
          backgroundSecondaryColor: localColors.bgSecondary,
          backgroundTertiaryColor: localColors.bgTertiary,
        })
      });

      if (response.ok) {
        const data = await response.json();
        if (data.success) {
          updateColors(localColors);
          setSuccess('Theme settings saved successfully!');
          setTimeout(() => setSuccess(''), 3000);
        } else {
          setError(data.message || 'Failed to save theme settings');
        }
      } else {
        const errorData = await response.json();
        setError(errorData.message || 'Failed to save theme settings');
      }
    } catch (error) {
      console.error('Failed to save theme:', error);
      setError('An error occurred while saving theme settings');
    } finally {
      setIsSaving(false);
    }
  };

  const handleReset = async () => {
    try {
      setIsSaving(true);
      setError('');
      setSuccess('');
      
      const response = await fetch('http://localhost:11666/api/system-settings/extensible-theme/reset', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${user?.accessToken}`,
          'Content-Type': 'application/json'
        }
      });

      if (response.ok) {
        const data = await response.json();
        if (data.success) {
          setLocalColors(colors);
          resetToDefaults();
          setSuccess('Theme reset to defaults successfully!');
          setTimeout(() => setSuccess(''), 3000);
        } else {
          setError(data.message || 'Failed to reset theme');
        }
      } else {
        const errorData = await response.json();
        setError(errorData.message || 'Failed to reset theme');
      }
    } catch (error) {
      console.error('Failed to reset theme:', error);
      setError('An error occurred while resetting theme');
    } finally {
      setIsSaving(false);
    }
  };

  const presetThemes = [
    {
      name: 'Ocean Blue',
      colors: {
        primary: '#0ea5e9',
        primaryLight: '#38bdf8',
        primaryDark: '#0284c7',
        secondary: '#6366f1',
        secondaryLight: '#818cf8',
        secondaryDark: '#4f46e5',
        accent: '#06b6d4',
        accentLight: '#22d3ee',
        accentDark: '#0891b2',
        bgPrimary: '#0f172a',
        bgSecondary: '#1e293b',
        bgTertiary: '#334155',
      }
    },
    {
      name: 'Purple Dream',
      colors: {
        primary: '#8b5cf6',
        primaryLight: '#a78bfa',
        primaryDark: '#7c3aed',
        secondary: '#ec4899',
        secondaryLight: '#f472b6',
        secondaryDark: '#db2777',
        accent: '#f59e0b',
        accentLight: '#fbbf24',
        accentDark: '#d97706',
        bgPrimary: '#1e1b4b',
        bgSecondary: '#312e81',
        bgTertiary: '#4338ca',
      }
    },
    {
      name: 'Emerald Forest',
      colors: {
        primary: '#10b981',
        primaryLight: '#34d399',
        primaryDark: '#059669',
        secondary: '#059669',
        secondaryLight: '#10b981',
        secondaryDark: '#047857',
        accent: '#f59e0b',
        accentLight: '#fbbf24',
        accentDark: '#d97706',
        bgPrimary: '#064e3b',
        bgSecondary: '#065f46',
        bgTertiary: '#047857',
      }
    },
    {
      name: 'Sunset Orange',
      colors: {
        primary: '#f97316',
        primaryLight: '#fb923c',
        primaryDark: '#ea580c',
        secondary: '#ef4444',
        secondaryLight: '#f87171',
        secondaryDark: '#dc2626',
        accent: '#f59e0b',
        accentLight: '#fbbf24',
        accentDark: '#d97706',
        bgPrimary: '#451a03',
        bgSecondary: '#7c2d12',
        bgTertiary: '#92400e',
      }
    },
    {
      name: 'Midnight Purple',
      colors: {
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
      }
    }
  ];

  const applyPreset = (preset: typeof presetThemes[0]) => {
    setLocalColors(preset.colors);
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="lg-card p-6">
          <div className="flex items-center justify-center py-8">
            <div className="lg-spinner"></div>
            <span className="ml-3 text-white">Loading theme settings...</span>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="lg-card p-6">
        <div className="flex items-center justify-between mb-6">
          <div>
            <h2 className="text-xl font-semibold text-white">Theme Customization</h2>
            <p className="text-gray-400 text-sm">
              Customize the liquid glass design system colors for your site
            </p>
          </div>
          <div className="flex gap-2">
            <button
              onClick={handleReset}
              className="lg-button lg-button-secondary"
              disabled={!isCustomized}
            >
              Reset to Defaults
            </button>
            <button
              onClick={handleSave}
              className="lg-button lg-button-primary"
              disabled={isSaving}
            >
              {isSaving ? 'Saving...' : 'Save Changes'}
            </button>
          </div>
        </div>

        {/* Status Messages */}
        {error && (
          <div className="mb-4 p-3 bg-red-500 bg-opacity-20 border border-red-500 border-opacity-30 rounded-lg">
            <p className="text-red-300 text-sm">{error}</p>
          </div>
        )}
        
        {success && (
          <div className="mb-4 p-3 bg-green-500 bg-opacity-20 border border-green-500 border-opacity-30 rounded-lg">
            <p className="text-green-300 text-sm">{success}</p>
          </div>
        )}

        {/* Preset Themes */}
        <div className="mb-8">
          <h3 className="text-lg font-medium text-white mb-4">Preset Themes</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {presetThemes.map((preset) => (
              <button
                key={preset.name}
                onClick={() => applyPreset(preset)}
                className="lg-card p-4 text-left hover:scale-105 transition-transform"
              >
                <div className="flex items-center gap-3 mb-3">
                  <div className="flex gap-1">
                    <div 
                      className="w-4 h-4 rounded"
                      style={{ backgroundColor: preset.colors.primary }}
                    />
                    <div 
                      className="w-4 h-4 rounded"
                      style={{ backgroundColor: preset.colors.secondary }}
                    />
                    <div 
                      className="w-4 h-4 rounded"
                      style={{ backgroundColor: preset.colors.accent }}
                    />
                  </div>
                  <span className="font-medium text-white">{preset.name}</span>
                </div>
                <div 
                  className="w-full h-8 rounded"
                  style={{
                    background: `linear-gradient(135deg, ${preset.colors.bgPrimary} 0%, ${preset.colors.bgSecondary} 50%, ${preset.colors.bgTertiary} 100%)`
                  }}
                />
              </button>
            ))}
          </div>
        </div>

        {/* Color Customization */}
        <div>
          <h3 className="text-lg font-medium text-white mb-4">Custom Colors</h3>
          
          {/* Primary Colors */}
          <div className="mb-6">
            <h4 className="text-md font-medium text-white mb-3">Primary Colors</h4>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <ColorPicker
                label="Primary"
                color={localColors.primary}
                onChange={(color) => handleColorChange('primary', color)}
                description="Main brand color"
              />
              <ColorPicker
                label="Primary Light"
                color={localColors.primaryLight}
                onChange={(color) => handleColorChange('primaryLight', color)}
                description="Lighter variant for hover states"
              />
              <ColorPicker
                label="Primary Dark"
                color={localColors.primaryDark}
                onChange={(color) => handleColorChange('primaryDark', color)}
                description="Darker variant for active states"
              />
            </div>
          </div>

          {/* Secondary Colors */}
          <div className="mb-6">
            <h4 className="text-md font-medium text-white mb-3">Secondary Colors</h4>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <ColorPicker
                label="Secondary"
                color={localColors.secondary}
                onChange={(color) => handleColorChange('secondary', color)}
                description="Secondary brand color"
              />
              <ColorPicker
                label="Secondary Light"
                color={localColors.secondaryLight}
                onChange={(color) => handleColorChange('secondaryLight', color)}
                description="Lighter variant for hover states"
              />
              <ColorPicker
                label="Secondary Dark"
                color={localColors.secondaryDark}
                onChange={(color) => handleColorChange('secondaryDark', color)}
                description="Darker variant for active states"
              />
            </div>
          </div>

          {/* Accent Colors */}
          <div className="mb-6">
            <h4 className="text-md font-medium text-white mb-3">Accent Colors</h4>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <ColorPicker
                label="Accent"
                color={localColors.accent}
                onChange={(color) => handleColorChange('accent', color)}
                description="Accent color for highlights"
              />
              <ColorPicker
                label="Accent Light"
                color={localColors.accentLight}
                onChange={(color) => handleColorChange('accentLight', color)}
                description="Lighter variant for hover states"
              />
              <ColorPicker
                label="Accent Dark"
                color={localColors.accentDark}
                onChange={(color) => handleColorChange('accentDark', color)}
                description="Darker variant for active states"
              />
            </div>
          </div>

          {/* Background Colors */}
          <div className="mb-6">
            <h4 className="text-md font-medium text-white mb-3">Background Colors</h4>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <ColorPicker
                label="Background Primary"
                color={localColors.bgPrimary}
                onChange={(color) => handleColorChange('bgPrimary', color)}
                description="Main background color"
              />
              <ColorPicker
                label="Background Secondary"
                color={localColors.bgSecondary}
                onChange={(color) => handleColorChange('bgSecondary', color)}
                description="Secondary background color"
              />
              <ColorPicker
                label="Background Tertiary"
                color={localColors.bgTertiary}
                onChange={(color) => handleColorChange('bgTertiary', color)}
                description="Tertiary background color"
              />
            </div>
          </div>
        </div>

        {/* Preview */}
        <div className="mt-8">
          <h3 className="text-lg font-medium text-white mb-4">Preview</h3>
          <div 
            className="lg-card p-6"
            style={{
              background: `linear-gradient(135deg, ${localColors.bgPrimary} 0%, ${localColors.bgSecondary} 50%, ${localColors.bgTertiary} 100%)`
            }}
          >
            <div className="flex flex-wrap gap-4">
              <button 
                className="lg-button lg-button-primary"
                style={{
                  background: `linear-gradient(135deg, ${localColors.primary} 0%, ${localColors.primaryDark} 100%)`,
                  borderColor: localColors.primary
                }}
              >
                Primary Button
              </button>
              <button 
                className="lg-button lg-button-secondary"
                style={{
                  background: `linear-gradient(135deg, ${localColors.secondary} 0%, ${localColors.secondaryDark} 100%)`,
                  borderColor: localColors.secondary
                }}
              >
                Secondary Button
              </button>
              <button 
                className="lg-button lg-button-accent"
                style={{
                  background: `linear-gradient(135deg, ${localColors.accent} 0%, ${localColors.accentDark} 100%)`,
                  borderColor: localColors.accent
                }}
              >
                Accent Button
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ThemeCustomization; 