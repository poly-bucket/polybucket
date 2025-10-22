import React, { useState, useEffect } from 'react';
import { useUserSettings } from '../../../context/UserSettingsContext';
import LayoutControls from '../../common/LayoutControls';
import { 
  GridView, 
  ViewList, 
  CropSquare, 
  Crop169, 
  Crop32, 
  SpaceBar,
  ViewColumn
} from '@mui/icons-material';

const DashboardLayoutSettingsTab: React.FC = () => {
  const { settings, updateSettings } = useUserSettings();
  const [localSettings, setLocalSettings] = useState({
    dashboardViewType: 'grid',
    cardSize: 'medium',
    cardSpacing: 'normal',
    gridColumns: 4
  });
  const [isSaving, setIsSaving] = useState(false);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' as 'success' | 'error' });

  useEffect(() => {
    if (settings) {
      setLocalSettings({
        dashboardViewType: settings.dashboardViewType || 'grid',
        cardSize: settings.cardSize || 'medium',
        cardSpacing: settings.cardSpacing || 'normal',
        gridColumns: settings.gridColumns || 4
      });
    }
  }, [settings]);

  const handleSettingChange = (key: string, value: any) => {
    setLocalSettings(prev => ({ ...prev, [key]: value }));
  };

  const saveSettings = async () => {
    setIsSaving(true);
    try {
      const success = await updateSettings(localSettings);
      if (success) {
        setSnackbar({ open: true, message: 'Dashboard layout settings updated successfully', severity: 'success' });
      } else {
        setSnackbar({ open: true, message: 'Failed to update dashboard layout settings', severity: 'error' });
      }
    } catch (error) {
      setSnackbar({ open: true, message: 'An error occurred while saving settings', severity: 'error' });
    } finally {
      setIsSaving(false);
    }
  };

  const viewTypeOptions = [
    { value: 'grid', label: 'Grid View', icon: GridView, description: 'Display models in a grid layout' },
    { value: 'list', label: 'List View', icon: ViewList, description: 'Display models in a list layout' }
  ];

  const cardSizeOptions = [
    { value: 'small', label: 'Small', icon: CropSquare, description: 'Compact cards for more items per row' },
    { value: 'medium', label: 'Medium', icon: Crop169, description: 'Standard card size' },
    { value: 'large', label: 'Large', icon: Crop32, description: 'Large cards for better visibility' }
  ];

  const spacingOptions = [
    { value: 'compact', label: 'Compact', icon: SpaceBar, description: 'Minimal spacing between cards' },
    { value: 'normal', label: 'Normal', icon: SpaceBar, description: 'Standard spacing' },
    { value: 'spacious', label: 'Spacious', icon: SpaceBar, description: 'Generous spacing between cards' }
  ];

  const gridColumnOptions = [
    { value: 2, label: '2 Columns', description: 'Large cards, fewer per row' },
    { value: 3, label: '3 Columns', description: 'Medium cards' },
    { value: 4, label: '4 Columns', description: 'Standard layout' },
    { value: 5, label: '5 Columns', description: 'Smaller cards, more per row' },
    { value: 6, label: '6 Columns', description: 'Compact grid layout' }
  ];

  return (
    <div className="space-y-6">
      {/* Quick Controls */}
      <div className="lg-card p-6">
        <h3 className="text-lg font-semibold text-white mb-4">Quick Layout Controls</h3>
        <p className="text-white/70 mb-4">
          Quickly adjust your layout settings. Changes apply immediately.
        </p>
        <LayoutControls showAdvanced={true} />
      </div>

      <div className="lg-card p-6">
        <h3 className="text-lg font-semibold text-white mb-4">Detailed Layout Settings</h3>
        <p className="text-white/70 mb-6">
          Customize how models are displayed in your dashboard. These settings affect the main model browsing interface.
        </p>

        {/* View Type */}
        <div className="mb-6">
          <label className="block text-sm font-medium text-white mb-3">View Type</label>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
            {viewTypeOptions.map((option) => (
              <button
                key={option.value}
                onClick={() => handleSettingChange('dashboardViewType', option.value)}
                className={`p-4 rounded-lg border-2 transition-all duration-200 ${
                  localSettings.dashboardViewType === option.value
                    ? 'border-blue-500 bg-blue-500/10'
                    : 'border-white/20 bg-white/5 hover:border-white/40'
                }`}
              >
                <div className="flex items-center space-x-3">
                  <option.icon className="w-5 h-5 text-white" />
                  <div className="text-left">
                    <div className="font-medium text-white">{option.label}</div>
                    <div className="text-sm text-white/60">{option.description}</div>
                  </div>
                </div>
              </button>
            ))}
          </div>
        </div>

        {/* Card Size */}
        <div className="mb-6">
          <label className="block text-sm font-medium text-white mb-3">Card Size</label>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
            {cardSizeOptions.map((option) => (
              <button
                key={option.value}
                onClick={() => handleSettingChange('cardSize', option.value)}
                className={`p-4 rounded-lg border-2 transition-all duration-200 ${
                  localSettings.cardSize === option.value
                    ? 'border-blue-500 bg-blue-500/10'
                    : 'border-white/20 bg-white/5 hover:border-white/40'
                }`}
              >
                <div className="flex items-center space-x-3">
                  <option.icon className="w-5 h-5 text-white" />
                  <div className="text-left">
                    <div className="font-medium text-white">{option.label}</div>
                    <div className="text-sm text-white/60">{option.description}</div>
                  </div>
                </div>
              </button>
            ))}
          </div>
        </div>

        {/* Card Spacing */}
        <div className="mb-6">
          <label className="block text-sm font-medium text-white mb-3">Card Spacing</label>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
            {spacingOptions.map((option) => (
              <button
                key={option.value}
                onClick={() => handleSettingChange('cardSpacing', option.value)}
                className={`p-4 rounded-lg border-2 transition-all duration-200 ${
                  localSettings.cardSpacing === option.value
                    ? 'border-blue-500 bg-blue-500/10'
                    : 'border-white/20 bg-white/5 hover:border-white/40'
                }`}
              >
                <div className="flex items-center space-x-3">
                  <option.icon className="w-5 h-5 text-white" />
                  <div className="text-left">
                    <div className="font-medium text-white">{option.label}</div>
                    <div className="text-sm text-white/60">{option.description}</div>
                  </div>
                </div>
              </button>
            ))}
          </div>
        </div>

        {/* Grid Columns (only show for grid view) */}
        {localSettings.dashboardViewType === 'grid' && (
          <div className="mb-6">
            <label className="block text-sm font-medium text-white mb-3">Grid Columns</label>
            <div className="grid grid-cols-1 md:grid-cols-5 gap-3">
              {gridColumnOptions.map((option) => (
                <button
                  key={option.value}
                  onClick={() => handleSettingChange('gridColumns', option.value)}
                  className={`p-4 rounded-lg border-2 transition-all duration-200 ${
                    localSettings.gridColumns === option.value
                      ? 'border-blue-500 bg-blue-500/10'
                      : 'border-white/20 bg-white/5 hover:border-white/40'
                  }`}
                >
                  <div className="flex items-center space-x-3">
                    <ViewColumn className="w-5 h-5 text-white" />
                    <div className="text-left">
                      <div className="font-medium text-white">{option.label}</div>
                      <div className="text-sm text-white/60">{option.description}</div>
                    </div>
                  </div>
                </button>
              ))}
            </div>
          </div>
        )}

        {/* Save Button */}
        <div className="flex justify-end pt-4 border-t border-white/10">
          <button
            onClick={saveSettings}
            disabled={isSaving}
            className="lg-button lg-button-primary"
          >
            {isSaving ? 'Saving...' : 'Save Layout Settings'}
          </button>
        </div>
      </div>

      {/* Preview Section */}
      <div className="lg-card p-6">
        <h4 className="text-md font-semibold text-white mb-4">Layout Preview</h4>
        <div className="bg-white/5 rounded-lg p-4">
          <div className="text-sm text-white/70 mb-3">
            Current settings: {localSettings.dashboardViewType} view, {localSettings.cardSize} cards, {localSettings.cardSpacing} spacing
            {localSettings.dashboardViewType === 'grid' && `, ${localSettings.gridColumns} columns`}
          </div>
          
          {/* Preview Grid */}
          <div className={`grid gap-${localSettings.cardSpacing === 'compact' ? '2' : localSettings.cardSpacing === 'spacious' ? '6' : '4'} ${
            localSettings.dashboardViewType === 'grid' 
              ? `grid-cols-${localSettings.gridColumns}` 
              : 'grid-cols-1'
          }`}>
            {Array.from({ length: localSettings.dashboardViewType === 'grid' ? 6 : 3 }).map((_, i) => (
              <div
                key={i}
                className={`bg-white/10 rounded-lg border border-white/20 ${
                  localSettings.cardSize === 'small' ? 'h-32' :
                  localSettings.cardSize === 'large' ? 'h-48' : 'h-40'
                }`}
              >
                <div className="p-3">
                  <div className="bg-white/20 rounded h-4 mb-2"></div>
                  <div className="bg-white/10 rounded h-3 w-2/3"></div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Snackbar */}
      {snackbar.open && (
        <div className={`fixed bottom-4 right-4 p-4 rounded-lg shadow-lg ${
          snackbar.severity === 'success' ? 'bg-green-600' : 'bg-red-600'
        } text-white`}>
          {snackbar.message}
        </div>
      )}
    </div>
  );
};

export default DashboardLayoutSettingsTab;
