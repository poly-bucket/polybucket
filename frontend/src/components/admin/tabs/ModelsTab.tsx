import React from 'react';
import { Add as AddIcon } from '@mui/icons-material';
import SiteModelSettingsComponent from './components/SiteModelSettings';
import CategoryManagement from './components/CategoryManagement';
import FileTypeSettings from './components/FileTypeSettings';

const ModelsTab: React.FC = () => {
  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-2xl font-bold text-white">Model Management</h2>
      </div>

      {/* Site Model Settings Section */}
      <SiteModelSettingsComponent />

      {/* Category Management Section */}
      <CategoryManagement />

      {/* File Type Settings Section */}
      <FileTypeSettings />
    </div>
  );
};

export default ModelsTab;
