import React, { useState, useEffect } from 'react';
import { useTheme, ThemeColors } from '../../context/ThemeContext';
import { useAppSelector } from '../../utils/hooks';
import themeService from '../../services/themeService';
import { ThemeDto } from '../../api/client';

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
      </div>
    </div>
    <div className="flex items-center gap-2">
      <div 
        className="w-8 h-8 rounded border border-gray-600 flex items-center justify-center cursor-pointer hover:scale-105 transition-transform"
        style={{ backgroundColor: color }}
        title="Click to pick a color"
        onClick={() => {
          const colorInput = document.createElement('input');
          colorInput.type = 'color';
          colorInput.value = color;
          colorInput.onchange = (e) => onChange((e.target as HTMLInputElement).value);
          colorInput.click();
        }}
      >
        <svg 
          className="w-4 h-4 text-white drop-shadow-lg" 
          fill="none" 
          stroke="currentColor" 
          viewBox="0 0 24 24"
        >
          <path 
            strokeLinecap="round" 
            strokeLinejoin="round" 
            strokeWidth={2} 
            d="M7 21a4 4 0 01-4-4V5a2 2 0 012-2h4a2 2 0 012 2v12a4 4 0 01-4 4zm0 0h12a2 2 0 002-2v-4a2 2 0 00-2-2h-2.343M11 7.343l1.657-1.657a2 2 0 012.828 0l2.829 2.829a2 2 0 010 2.828l-8.486 8.485M7 17h.01" 
          />
        </svg>
      </div>
      <div className="flex-1 relative">
        <input
          type="text"
          value={color}
          onChange={(e) => onChange(e.target.value)}
          className="lg-input text-xs pr-10"
          placeholder="#000000"
        />
        <button
          onClick={() => {
            navigator.clipboard.writeText(color);
            // Optional: Show a brief success message
            const button = document.activeElement as HTMLButtonElement;
            if (button) {
              const originalText = button.innerHTML;
              button.innerHTML = '✓';
              button.className = 'absolute right-2 top-1/2 transform -translate-y-1/2 text-green-400 hover:text-green-300 transition-colors';
              setTimeout(() => {
                button.innerHTML = originalText;
                button.className = 'absolute right-2 top-1/2 transform -translate-y-1/2 text-gray-400 hover:text-gray-300 transition-colors';
              }, 1000);
            }
          }}
          className="absolute right-2 top-1/2 transform -translate-y-1/2 text-gray-400 hover:text-gray-300 transition-colors"
          title="Copy hex code to clipboard"
        >
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z" />
          </svg>
        </button>
      </div>
    </div>
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
  const [themes, setThemes] = useState<ThemeDto[]>([]);
  const [activeTheme, setActiveTheme] = useState<ThemeDto | null>(null);
  
  // Theme creation modal state
  const [showThemeModal, setShowThemeModal] = useState(false);
  const [themeName, setThemeName] = useState('');
  const [themeDescription, setThemeDescription] = useState('');
  const [modalColors, setModalColors] = useState<ThemeColors>(colors);

  // Load theme settings from backend on mount
  useEffect(() => {
    loadThemeSettings();
  }, []);

  const loadThemeSettings = async () => {
    try {
      setIsLoading(true);
      
      // Load themes from backend using theme service
      const themesResponse = await themeService.getThemes();
      setThemes(themesResponse.themes || []);
      setActiveTheme(themesResponse.activeTheme || null);
      
      // If there's an active theme, use its colors
      if (themesResponse.activeTheme?.colors) {
        const frontendColors = themeService.convertToFrontendFormat(themesResponse.activeTheme.colors);
        setLocalColors(frontendColors);
        updateColors(frontendColors);
      } else {
        // Use default colors if no active theme
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
        setLocalColors(defaultColors);
        updateColors(defaultColors);
      }
    } catch (error) {
      console.error('Error loading theme settings:', error);
      // Fallback to default colors on error
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
      setLocalColors(defaultColors);
      updateColors(defaultColors);
    } finally {
      setIsLoading(false);
    }
  };

  const handleColorChange = (key: keyof ThemeColors, value: string) => {
    setLocalColors(prev => ({ ...prev, [key]: value }));
  };

  const handleReset = async () => {
    try {
      setIsSaving(true);
      setError('');
      setSuccess('');
      
      // Find the default theme
      const defaultTheme = themes.find(theme => theme.isDefault);
      
      if (defaultTheme && defaultTheme.id !== undefined) {
        // Set the default theme as active
        const success = await themeService.setActiveTheme(defaultTheme.id);
        if (success) {
          // Convert backend format to frontend format using theme service
          if (defaultTheme.colors) {
            const frontendColors = themeService.convertToFrontendFormat(defaultTheme.colors);
            setLocalColors(frontendColors);
            updateColors(frontendColors);
          }
          
          // Reload themes to get updated active state
          await loadThemeSettings();
          
          setSuccess('Theme reset to defaults successfully!');
          setTimeout(() => setSuccess(''), 3000);
        } else {
          setError('Failed to reset theme to defaults');
        }
      } else {
        // No default theme found, use hardcoded defaults
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
        setLocalColors(defaultColors);
        updateColors(defaultColors);
        setSuccess('Theme reset to defaults successfully!');
        setTimeout(() => setSuccess(''), 3000);
      }
    } catch (error) {
      console.error('Failed to reset theme:', error);
      setError('An error occurred while resetting theme');
    } finally {
      setIsSaving(false);
    }
  };

  const applyPreset = async (theme: ThemeDto) => {
    try {
      // Set the theme as active using theme service
      if (theme.id === undefined) {
        setError('Theme ID is missing');
        return;
      }
      
      const success = await themeService.setActiveTheme(theme.id);
      if (success) {
        // Convert backend format to frontend format using theme service
        if (theme.colors) {
          const frontendColors = themeService.convertToFrontendFormat(theme.colors);
          setLocalColors(frontendColors);
          updateColors(frontendColors);
        }
        
        // Reload themes to get updated active state
        await loadThemeSettings();
        
        setSuccess(`Theme "${theme.name || 'Unknown'}" activated successfully!`);
        setTimeout(() => setSuccess(''), 3000);
      } else {
        setError('Failed to activate theme');
      }
    } catch (error) {
      console.error('Failed to apply theme:', error);
      setError('An error occurred while applying theme');
    }
  };

  const createCustomTheme = async () => {
    // Open the modal instead of using prompt
    setModalColors(localColors);
    setThemeName('');
    setThemeDescription('Custom theme created by user');
    setShowThemeModal(true);
  };

  const handleCreateTheme = async () => {
    if (!themeName.trim()) {
      setError('Theme name is required');
      return;
    }

    try {
      setIsSaving(true);
      setError('');
      setSuccess('');
      
      // Import the required classes
      const { CreateThemeRequest, ThemeColorsDto } = await import('../../api/client');
      
      // Create a proper ThemeColorsDto instance
      const themeColorsDto = new ThemeColorsDto();
      themeColorsDto.primary = modalColors.primary;
      themeColorsDto.primaryLight = modalColors.primaryLight;
      themeColorsDto.primaryDark = modalColors.primaryDark;
      themeColorsDto.secondary = modalColors.secondary;
      themeColorsDto.secondaryLight = modalColors.secondaryLight;
      themeColorsDto.secondaryDark = modalColors.secondaryDark;
      themeColorsDto.accent = modalColors.accent;
      themeColorsDto.accentLight = modalColors.accentLight;
      themeColorsDto.accentDark = modalColors.accentDark;
      themeColorsDto.backgroundPrimary = modalColors.bgPrimary;
      themeColorsDto.backgroundSecondary = modalColors.bgSecondary;
      themeColorsDto.backgroundTertiary = modalColors.bgTertiary;
      
      // Create a new CreateThemeRequest instance
      const createRequest = new CreateThemeRequest();
      createRequest.name = themeName.trim();
      createRequest.description = themeDescription.trim() || 'Custom theme created by user';
      createRequest.isDefault = false;
      createRequest.colors = themeColorsDto;
      
      // Create theme using theme service
      const response = await themeService.createTheme(createRequest);
      
      if (response.success) {
        setSuccess('Custom theme created successfully!');
        setTimeout(() => setSuccess(''), 3000);
        
        // Close the modal
        setShowThemeModal(false);
        
        // Reload themes to include the new one
        await loadThemeSettings();
      } else {
        setError(response.message || 'Failed to create custom theme');
      }
    } catch (error) {
      console.error('Failed to create custom theme:', error);
      setError('An error occurred while creating custom theme');
    } finally {
      setIsSaving(false);
    }
  };

  const handleSave = async () => {
    // Open the modal for saving as custom theme
    setModalColors(localColors);
    setThemeName('');
    setThemeDescription('Custom theme created by user');
    setShowThemeModal(true);
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
              onClick={createCustomTheme}
              className="lg-button lg-button-secondary"
              disabled={isSaving}
            >
              Save as Custom Theme
            </button>
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
          <h3 className="text-lg font-medium text-white mb-4">Available Themes</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {themes.map((theme) => (
              <button
                key={theme.id}
                onClick={() => applyPreset(theme)}
                className={`lg-card p-4 text-left hover:scale-105 transition-transform ${
                  theme.isActive ? 'ring-2 ring-blue-500' : ''
                }`}
              >
                <div className="flex items-center gap-3 mb-3">
                  <div className="flex gap-1">
                    <div 
                      className="w-4 h-4 rounded"
                      style={{ backgroundColor: theme.colors?.primary || '#ccc' }}
                    />
                    <div 
                      className="w-4 h-4 rounded"
                      style={{ backgroundColor: theme.colors?.secondary || '#ccc' }}
                    />
                    <div 
                      className="w-4 h-4 rounded"
                      style={{ backgroundColor: theme.colors?.accent || '#ccc' }}
                    />
                  </div>
                  <span className="font-medium text-white">{theme.name || 'Unnamed Theme'}</span>
                  {theme.isActive && (
                    <span className="text-xs bg-blue-500 text-white px-2 py-1 rounded">Active</span>
                  )}
                  {theme.isDefault && (
                    <span className="text-xs bg-gray-500 text-white px-2 py-1 rounded">Default</span>
                  )}
                </div>
                {theme.colors && (
                  <div 
                    className="w-full h-8 rounded"
                    style={{
                      background: `linear-gradient(135deg, ${theme.colors.backgroundPrimary || '#ccc'} 0%, ${theme.colors.backgroundSecondary || '#ccc'} 50%, ${theme.colors.backgroundTertiary || '#ccc'} 100%)`
                    }}
                  />
                )}
                {theme.description && (
                  <p className="text-xs text-gray-400 mt-2">{theme.description}</p>
                )}
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

      {/* Theme Creation Modal */}
      {showThemeModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="lg-card p-6 w-full max-w-5xl max-h-[90vh] overflow-y-auto">
            <h3 className="text-xl font-semibold text-white mb-4">Create New Theme</h3>
            
            <div className="mb-4">
              <label className="text-sm font-medium text-white mb-2 block">Theme Name *</label>
              <input
                type="text"
                value={themeName}
                onChange={(e) => setThemeName(e.target.value)}
                className="lg-input text-white bg-gray-700 w-full"
                placeholder="e.g., Dark Mode, Light Mode"
              />
            </div>
            
            <div className="mb-4">
              <label className="text-sm font-medium text-white mb-2 block">Description (Optional)</label>
              <textarea
                value={themeDescription}
                onChange={(e) => setThemeDescription(e.target.value)}
                className="lg-input text-white bg-gray-700 w-full h-20 resize-none"
                placeholder="e.g., A dark theme for better readability"
              />
            </div>
            
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
              {/* Theme Details and Preview */}
              <div>
                <div className="mb-6">
                  <h4 className="text-md font-medium text-white mb-3">Preview</h4>
                  <div 
                    className="lg-card p-4"
                    style={{
                      background: `linear-gradient(135deg, ${modalColors.bgPrimary} 0%, ${modalColors.bgSecondary} 50%, ${modalColors.bgTertiary} 100%)`
                    }}
                  >
                    <div className="flex flex-wrap gap-3">
                      <button 
                        className="lg-button lg-button-primary"
                        style={{
                          background: `linear-gradient(135deg, ${modalColors.primary} 0%, ${modalColors.primaryDark} 100%)`,
                          borderColor: modalColors.primary
                        }}
                      >
                        Primary
                      </button>
                      <button 
                        className="lg-button lg-button-secondary"
                        style={{
                          background: `linear-gradient(135deg, ${modalColors.secondary} 0%, ${modalColors.secondaryDark} 100%)`,
                          borderColor: modalColors.secondary
                        }}
                      >
                        Secondary
                      </button>
                      <button 
                        className="lg-button lg-button-accent"
                        style={{
                          background: `linear-gradient(135deg, ${modalColors.accent} 0%, ${modalColors.accentDark} 100%)`,
                          borderColor: modalColors.accent
                        }}
                      >
                        Accent
                      </button>
                    </div>
                  </div>
                </div>
              </div>

              {/* Color Customization */}
              <div>
                <h4 className="text-md font-medium text-white mb-3">Customize Colors</h4>
                
                {/* Primary Colors */}
                <div className="mb-4">
                  <h5 className="text-sm font-medium text-white mb-2">Primary Colors</h5>
                  <div className="space-y-2">
                    <ColorPicker
                      label="Primary"
                      color={modalColors.primary}
                      onChange={(color) => setModalColors(prev => ({ ...prev, primary: color }))}
                      description="Main brand color"
                    />
                    <ColorPicker
                      label="Primary Light"
                      color={modalColors.primaryLight}
                      onChange={(color) => setModalColors(prev => ({ ...prev, primaryLight: color }))}
                      description="Lighter variant for hover states"
                    />
                    <ColorPicker
                      label="Primary Dark"
                      color={modalColors.primaryDark}
                      onChange={(color) => setModalColors(prev => ({ ...prev, primaryDark: color }))}
                      description="Darker variant for active states"
                    />
                  </div>
                </div>

                {/* Secondary Colors */}
                <div className="mb-4">
                  <h5 className="text-sm font-medium text-white mb-2">Secondary Colors</h5>
                  <div className="space-y-2">
                    <ColorPicker
                      label="Secondary"
                      color={modalColors.secondary}
                      onChange={(color) => setModalColors(prev => ({ ...prev, secondary: color }))}
                      description="Secondary brand color"
                    />
                    <ColorPicker
                      label="Secondary Light"
                      color={modalColors.secondaryLight}
                      onChange={(color) => setModalColors(prev => ({ ...prev, secondaryLight: color }))}
                      description="Lighter variant for hover states"
                    />
                    <ColorPicker
                      label="Secondary Dark"
                      color={modalColors.secondaryDark}
                      onChange={(color) => setModalColors(prev => ({ ...prev, secondaryDark: color }))}
                      description="Darker variant for active states"
                    />
                  </div>
                </div>

                {/* Accent Colors */}
                <div className="mb-4">
                  <h5 className="text-sm font-medium text-white mb-2">Accent Colors</h5>
                  <div className="space-y-2">
                    <ColorPicker
                      label="Accent"
                      color={modalColors.accent}
                      onChange={(color) => setModalColors(prev => ({ ...prev, accent: color }))}
                      description="Accent color for highlights"
                    />
                    <ColorPicker
                      label="Accent Light"
                      color={modalColors.accentLight}
                      onChange={(color) => setModalColors(prev => ({ ...prev, accentLight: color }))}
                      description="Lighter variant for hover states"
                    />
                    <ColorPicker
                      label="Accent Dark"
                      color={modalColors.accentDark}
                      onChange={(color) => setModalColors(prev => ({ ...prev, accentDark: color }))}
                      description="Darker variant for active states"
                    />
                  </div>
                </div>

                {/* Background Colors */}
                <div className="mb-4">
                  <h5 className="text-sm font-medium text-white mb-2">Background Colors</h5>
                  <div className="space-y-2">
                    <ColorPicker
                      label="Background Primary"
                      color={modalColors.bgPrimary}
                      onChange={(color) => setModalColors(prev => ({ ...prev, bgPrimary: color }))}
                      description="Main background color"
                    />
                    <ColorPicker
                      label="Background Secondary"
                      color={modalColors.bgSecondary}
                      onChange={(color) => setModalColors(prev => ({ ...prev, bgSecondary: color }))}
                      description="Secondary background color"
                    />
                    <ColorPicker
                      label="Background Tertiary"
                      color={modalColors.bgTertiary}
                      onChange={(color) => setModalColors(prev => ({ ...prev, bgTertiary: color }))}
                      description="Tertiary background color"
                    />
                  </div>
                </div>
              </div>
            </div>

            <div className="flex justify-end gap-3 mt-6 pt-4 border-t border-gray-600">
              <button
                onClick={() => setShowThemeModal(false)}
                className="lg-button lg-button-secondary"
              >
                Cancel
              </button>
              <button
                onClick={handleCreateTheme}
                className="lg-button lg-button-primary"
                disabled={isSaving || !themeName.trim()}
              >
                {isSaving ? 'Creating...' : 'Create Theme'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default ThemeCustomization; 