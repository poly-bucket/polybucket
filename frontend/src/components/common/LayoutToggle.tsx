import React from 'react';
import { useUserSettings } from '../../context/UserSettingsContext';
import { GridView, ViewList } from '@mui/icons-material';

interface LayoutToggleProps {
  className?: string;
}

const LayoutToggle: React.FC<LayoutToggleProps> = ({ className = '' }) => {
  const { settings, updateSettings } = useUserSettings();
  
  const currentViewType = settings?.dashboardViewType || 'grid';
  
  const handleToggle = async () => {
    const newViewType = currentViewType === 'grid' ? 'list' : 'grid';
    await updateSettings({ dashboardViewType: newViewType });
  };
  
  return (
    <div className={`flex items-center space-x-2 ${className}`}>
      <span className="text-sm text-white/60">View:</span>
      <div className="flex bg-white/10 rounded-lg p-1">
        <button
          onClick={() => updateSettings({ dashboardViewType: 'grid' })}
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
          onClick={() => updateSettings({ dashboardViewType: 'list' })}
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
    </div>
  );
};

export default LayoutToggle;
