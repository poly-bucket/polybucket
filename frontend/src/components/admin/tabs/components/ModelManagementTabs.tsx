import React, { useState } from 'react';
import {
  Settings as SettingsIcon,
  Storage as StorageIcon,
  Category as CategoryIcon,
  AttachFile as FileIcon,
  Gavel as ModerationIcon,
  DeleteForever as DeleteIcon
} from '@mui/icons-material';
import ModelConfigurationSettingsComponent from './ModelConfigurationSettings';
import SiteModelSettingsComponent from './SiteModelSettings';
import CategoryManagement from './CategoryManagement';
import FileTypeSettings from './FileTypeSettings';
import ModelModerationActions from './ModelModerationActions';
import DeleteAllModels from './DeleteAllModels';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`model-tabpanel-${index}`}
      aria-labelledby={`model-tab-${index}`}
      {...other}
    >
      {value === index && (
        <div>
          {children}
        </div>
      )}
    </div>
  );
}

const ModelManagementTabs: React.FC = () => {
  const [tabValue, setTabValue] = useState(0);

  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  const tabs = [
    { 
      label: 'Configuration', 
      icon: <SettingsIcon />, 
      component: <ModelConfigurationSettingsComponent /> 
    },
    { 
      label: 'Site Settings', 
      icon: <StorageIcon />, 
      component: <SiteModelSettingsComponent /> 
    },
    { 
      label: 'Categories', 
      icon: <CategoryIcon />, 
      component: <CategoryManagement /> 
    },
    { 
      label: 'File Types', 
      icon: <FileIcon />, 
      component: <FileTypeSettings /> 
    },
    { 
      label: 'Moderation Actions', 
      icon: <ModerationIcon />, 
      component: <ModelModerationActions /> 
    },
    { 
      label: 'Bulk Operations', 
      icon: <DeleteIcon />, 
      component: <DeleteAllModels /> 
    },
  ];

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-2xl font-bold text-white">Model Management</h2>
      </div>

      {/* Model Management Tabs */}
      <div className="bg-gray-800 border-b border-gray-700">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <nav className="flex space-x-8">
            {tabs.map((tab, index) => (
              <button
                key={index}
                onClick={() => setTabValue(index)}
                className={`py-4 px-1 border-b-2 font-medium text-sm capitalize transition-colors duration-200 flex items-center gap-2 ${
                  tabValue === index
                    ? 'border-indigo-500 text-indigo-400'
                    : 'border-transparent text-white/60 hover:text-white/80 hover:border-white/20'
                }`}
              >
                {tab.icon}
                {tab.label}
              </button>
            ))}
          </nav>
        </div>
      </div>
      
      {/* Tab Content */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        {tabs.map((tab, index) => (
          <TabPanel key={index} value={tabValue} index={index}>
            {tab.component}
          </TabPanel>
        ))}
      </div>
    </div>
  );
};

export default ModelManagementTabs;
