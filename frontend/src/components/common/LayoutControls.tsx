import React, { useState } from 'react';
import { useUserSettings } from '../../context/UserSettingsContext';
import { 
  GridView, 
  ViewList, 
  CropSquare, 
  Crop169, 
  Crop32, 
  SpaceBar,
  ViewColumn,
  Settings as SettingsIcon,
  Tune
} from '@mui/icons-material';

interface LayoutControlsProps {
  className?: string;
  showAdvanced?: boolean;
  compact?: boolean;
}

const LayoutControls: React.FC<LayoutControlsProps> = ({ 
  className = '', 
  showAdvanced = false,
  compact = false 
}) => {
  const { settings, updateSettings } = useUserSettings();
  const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null);
  const [showAdvancedControls, setShowAdvancedControls] = useState(showAdvanced);
  
  const currentViewType = settings?.dashboardViewType || 'grid';
  const currentCardSize = settings?.cardSize || 'medium';
  const currentSpacing = settings?.cardSpacing || 'normal';
  const currentColumns = settings?.gridColumns || 4;
  
  // Convert string values to numeric for sliders
  const getSpacingValue = (spacing: string) => {
    switch (spacing) {
      case 'compact': return 0;
      case 'spacious': return 2;
      default: return 1; // normal
    }
  };
  
  const getCardSizeValue = (size: string) => {
    switch (size) {
      case 'small': return 0;
      case 'large': return 2;
      default: return 1; // medium
    }
  };
  
  const getSpacingFromValue = (value: number) => {
    switch (value) {
      case 0: return 'compact';
      case 2: return 'spacious';
      default: return 'normal';
    }
  };
  
  const getCardSizeFromValue = (value: number) => {
    switch (value) {
      case 0: return 'small';
      case 2: return 'large';
      default: return 'medium';
    }
  };
  
  const handleViewTypeChange = async (viewType: 'grid' | 'list') => {
    await updateSettings({ dashboardViewType: viewType });
  };
  
  const handleSpacingChange = async (value: number) => {
    const spacing = getSpacingFromValue(value);
    await updateSettings({ cardSpacing: spacing });
  };
  
  const handleCardSizeChange = async (value: number) => {
    const size = getCardSizeFromValue(value);
    await updateSettings({ cardSize: size });
  };
  
  const handleColumnsChange = async (columns: number) => {
    await updateSettings({ gridColumns: columns });
  };
  
  const handlePopoverOpen = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };
  
  const handlePopoverClose = () => {
    setAnchorEl(null);
  };
  
  const open = Boolean(anchorEl);
  
  if (compact) {
    return (
      <div className={`flex items-center space-x-2 ${className}`}>
        <span className="text-sm text-white/60">View:</span>
        <div className="flex bg-white/10 rounded-lg p-1">
          <button
            onClick={() => handleViewTypeChange('grid')}
            className={`p-2 rounded-md transition-all duration-200 flex items-center space-x-2 ${
              currentViewType === 'grid'
                ? 'bg-blue-500 text-white'
                : 'text-white/60 hover:text-white hover:bg-white/10'
            }`}
            title="Grid View"
          >
            <GridView className="w-4 h-4" />
            <span className="text-xs hidden sm:inline">Grid</span>
          </button>
          <button
            onClick={() => handleViewTypeChange('list')}
            className={`p-2 rounded-md transition-all duration-200 flex items-center space-x-2 ${
              currentViewType === 'list'
                ? 'bg-blue-500 text-white'
                : 'text-white/60 hover:text-white hover:bg-white/10'
            }`}
            title="List View"
          >
            <ViewList className="w-4 h-4" />
            <span className="text-xs hidden sm:inline">List</span>
          </button>
        </div>
        
        {/* Layout Settings Popover Trigger */}
        <button
          onClick={handlePopoverOpen}
          className="p-2 text-white/60 hover:text-white hover:bg-white/10 rounded-md transition-all duration-200"
          title="Layout Settings"
        >
          <Tune className="w-4 h-4" />
        </button>
        
        {/* Layout Settings Popover */}
        {open && (
          <div className="fixed inset-0 z-50" onClick={handlePopoverClose}>
            <div 
              className="absolute right-0 top-12 w-80 bg-gray-800 border border-gray-600 rounded-lg shadow-xl p-4 transform transition-all duration-200 animate-in slide-in-from-top-2"
              style={{
                right: '0px',
                top: '48px',
                maxHeight: 'calc(100vh - 100px)',
                overflowY: 'auto'
              }}
              onClick={(e) => e.stopPropagation()}
            >
              <div className="space-y-4">
                <div className="flex items-center justify-between">
                  <h3 className="text-sm font-medium text-white">Layout Settings</h3>
                  <button
                    onClick={handlePopoverClose}
                    className="text-white/60 hover:text-white"
                  >
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                </div>
                
                {/* Card Size Slider */}
                <div className="space-y-2">
                  <div className="flex items-center justify-between">
                    <label className="text-xs text-white/80">Card Size</label>
                    <span className="text-xs text-white/60 capitalize">{currentCardSize}</span>
                  </div>
                  <div className="relative">
                    <input
                      type="range"
                      min="0"
                      max="2"
                      step="1"
                      value={getCardSizeValue(currentCardSize)}
                      onChange={(e) => handleCardSizeChange(parseInt(e.target.value))}
                      className="w-full h-2 bg-gray-600 rounded-lg appearance-none cursor-pointer slider"
                    />
                    <div className="flex justify-between text-xs text-white/40 mt-1">
                      <span>Small</span>
                      <span>Medium</span>
                      <span>Large</span>
                    </div>
                  </div>
                </div>
                
                {/* Spacing Slider */}
                <div className="space-y-2">
                  <div className="flex items-center justify-between">
                    <label className="text-xs text-white/80">Spacing</label>
                    <span className="text-xs text-white/60 capitalize">{currentSpacing}</span>
                  </div>
                  <div className="relative">
                    <input
                      type="range"
                      min="0"
                      max="2"
                      step="1"
                      value={getSpacingValue(currentSpacing)}
                      onChange={(e) => handleSpacingChange(parseInt(e.target.value))}
                      className="w-full h-2 bg-gray-600 rounded-lg appearance-none cursor-pointer slider"
                    />
                    <div className="flex justify-between text-xs text-white/40 mt-1">
                      <span>Compact</span>
                      <span>Normal</span>
                      <span>Spacious</span>
                    </div>
                  </div>
                </div>
                
                {/* Grid Columns (only for grid view) */}
                {currentViewType === 'grid' && (
                  <div className="space-y-2">
                    <div className="flex items-center justify-between">
                      <label className="text-xs text-white/80">Columns</label>
                      <span className="text-xs text-white/60">{currentColumns}</span>
                    </div>
                    <div className="flex bg-white/10 rounded-lg p-1">
                      {[2, 3, 4, 5, 6].map((columns) => (
                        <button
                          key={columns}
                          onClick={() => handleColumnsChange(columns)}
                          className={`flex-1 p-2 rounded-md transition-all duration-200 text-xs ${
                            currentColumns === columns
                              ? 'bg-blue-500 text-white'
                              : 'text-white/60 hover:text-white hover:bg-white/10'
                          }`}
                        >
                          {columns}
                        </button>
                      ))}
                    </div>
                  </div>
                )}
                
                {/* Preview */}
                <div className="pt-2 border-t border-white/10">
                  <div className="text-xs text-white/60">
                    Preview: {currentViewType} view, {currentCardSize} cards, {currentSpacing} spacing
                    {currentViewType === 'grid' && `, ${currentColumns} columns`}
                  </div>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    );
  }
  
  return (
    <div className={`space-y-4 ${className}`}>
      {/* View Type Toggle */}
      <div className="flex items-center space-x-4">
        <span className="text-sm font-medium text-white">View Type:</span>
        <div className="flex bg-white/10 rounded-lg p-1">
          <button
            onClick={() => handleViewTypeChange('grid')}
            className={`p-2 rounded-md transition-all duration-200 flex items-center space-x-2 ${
              currentViewType === 'grid'
                ? 'bg-blue-500 text-white'
                : 'text-white/60 hover:text-white hover:bg-white/10'
            }`}
            title="Grid View"
          >
            <GridView className="w-4 h-4" />
            <span className="text-xs">Grid</span>
          </button>
          <button
            onClick={() => handleViewTypeChange('list')}
            className={`p-2 rounded-md transition-all duration-200 flex items-center space-x-2 ${
              currentViewType === 'list'
                ? 'bg-blue-500 text-white'
                : 'text-white/60 hover:text-white hover:bg-white/10'
            }`}
            title="List View"
          >
            <ViewList className="w-4 h-4" />
            <span className="text-xs">List</span>
          </button>
        </div>
      </div>
      
      {/* Advanced Controls */}
      {showAdvancedControls && (
        <div className="space-y-4 pt-4 border-t border-white/10">
          {/* Card Size Slider */}
          <div className="space-y-2">
            <div className="flex items-center justify-between">
              <label className="text-sm font-medium text-white">Card Size</label>
              <span className="text-sm text-white/60 capitalize">{currentCardSize}</span>
            </div>
            <div className="relative">
              <input
                type="range"
                min="0"
                max="2"
                step="1"
                value={getCardSizeValue(currentCardSize)}
                onChange={(e) => handleCardSizeChange(parseInt(e.target.value))}
                className="w-full h-2 bg-gray-600 rounded-lg appearance-none cursor-pointer slider"
              />
              <div className="flex justify-between text-xs text-white/40 mt-1">
                <span>Small</span>
                <span>Medium</span>
                <span>Large</span>
              </div>
            </div>
          </div>
          
          {/* Spacing Slider */}
          <div className="space-y-2">
            <div className="flex items-center justify-between">
              <label className="text-sm font-medium text-white">Spacing</label>
              <span className="text-sm text-white/60 capitalize">{currentSpacing}</span>
            </div>
            <div className="relative">
              <input
                type="range"
                min="0"
                max="2"
                step="1"
                value={getSpacingValue(currentSpacing)}
                onChange={(e) => handleSpacingChange(parseInt(e.target.value))}
                className="w-full h-2 bg-gray-600 rounded-lg appearance-none cursor-pointer slider"
              />
              <div className="flex justify-between text-xs text-white/40 mt-1">
                <span>Compact</span>
                <span>Normal</span>
                <span>Spacious</span>
              </div>
            </div>
          </div>
          
          {/* Grid Columns (only for grid view) */}
          {currentViewType === 'grid' && (
            <div className="space-y-2">
              <label className="text-sm font-medium text-white">Grid Columns</label>
              <div className="flex bg-white/10 rounded-lg p-1">
                {[2, 3, 4, 5, 6].map((columns) => (
                  <button
                    key={columns}
                    onClick={() => handleColumnsChange(columns)}
                    className={`flex-1 p-2 rounded-md transition-all duration-200 text-xs ${
                      currentColumns === columns
                        ? 'bg-blue-500 text-white'
                        : 'text-white/60 hover:text-white hover:bg-white/10'
                    }`}
                  >
                    {columns}
                  </button>
                ))}
              </div>
            </div>
          )}
        </div>
      )}
      
      {/* Toggle Advanced Controls */}
      <button
        onClick={() => setShowAdvancedControls(!showAdvancedControls)}
        className="flex items-center space-x-2 text-sm text-white/60 hover:text-white transition-colors duration-200"
      >
        <SettingsIcon className="w-4 h-4" />
        <span>{showAdvancedControls ? 'Hide' : 'Show'} Advanced Settings</span>
      </button>
    </div>
  );
};

export default LayoutControls;
