import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import NavigationBar from '../components/common/NavigationBar';
import {
  People as PeopleIcon,
  ViewInAr as ModelIcon,
  Settings as SettingsIcon,
  Person as PersonIcon,
  Security as SecurityIcon,
  Extension as ExtensionIcon,
  Dashboard as DashboardIcon,
  Gavel as ModerationIcon,
  AdminPanelSettings as RoleIcon,
  Storage as StorageIcon,
  Notifications as NotificationIcon,
  Analytics as AnalyticsIcon,
  Add as AddIcon,
  Visibility,
  VisibilityOff,
  ContentCopy as CopyIcon,
  Palette as PaletteIcon,
  EmojiEmotions as IconsIcon,
  Public as FederationIcon
} from '@mui/icons-material';
import {
  DashboardTab,
  UsersTab,
  RolesTab,
  ModelsTab,
  SystemTab,
  AuthTab,
  TokensTab,
  ThemeTab,
  PluginsTab,
  FontAwesomeTab
} from '../components/admin/tabs';
import FederationManagement from './federation-management/FederationManagement';

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
      id={`admin-tabpanel-${index}`}
      aria-labelledby={`admin-tab-${index}`}
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

const AdminControlPanel: React.FC = () => {
  const navigate = useNavigate();
  const [tabValue, setTabValue] = useState(0);

  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  const handleBackToDashboard = () => {
    navigate('/dashboard');
  };

  const tabs = [
    { label: 'Dashboard', icon: <DashboardIcon />, component: <DashboardTab /> },
    { label: 'Users', icon: <PeopleIcon />, component: <UsersTab /> },
    { label: 'Roles', icon: <RoleIcon />, component: <RolesTab /> },
    { label: 'Models', icon: <ModelIcon />, component: <ModelsTab /> },
    { label: 'System', icon: <SettingsIcon />, component: <SystemTab /> },
    { label: 'Auth', icon: <SecurityIcon />, component: <AuthTab /> },
    { label: 'Tokens', icon: <SecurityIcon />, component: <TokensTab /> },
    { label: 'Federation', icon: <FederationIcon />, component: <FederationManagement /> },
    { label: 'Theme', icon: <PaletteIcon />, component: <ThemeTab /> },
    { label: 'Plugins', icon: <ExtensionIcon />, component: <PluginsTab /> },
    { label: 'FontAwesome', icon: <IconsIcon />, component: <FontAwesomeTab /> },
  ];

  return (
    <div className="lg-container min-h-screen">
      {/* Navigation Bar */}
      <NavigationBar
        title="Admin Control Panel"
        showSearch={false}
        showUploadButton={false}
        showHomeLink={true}
      />

      {/* Navigation Tabs */}
      <div className="lg-tabs">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <nav>
            {tabs.map((tab, index) => (
              <button
                key={index}
                onClick={() => setTabValue(index)}
                className={`lg-tab-button ${tabValue === index ? 'active' : ''}`}
              >
                {tab.icon}
                {tab.label}
              </button>
            ))}
          </nav>
        </div>
      </div>
      
      {/* Main Content Area */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        {tabs.map((tab, index) => (
          <TabPanel key={index} value={tabValue} index={index}>
            <div className="lg-tab-panel">
              {tab.component}
            </div>
          </TabPanel>
        ))}
      </div>
    </div>
  );
};

export default AdminControlPanel;
