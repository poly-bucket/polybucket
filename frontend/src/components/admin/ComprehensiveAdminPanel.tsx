import React, { useState, useEffect } from 'react';
import { useAppSelector } from '../../utils/hooks';
import api from '../../utils/axiosConfig';
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
  Palette as PaletteIcon
} from '@mui/icons-material';
import RoleManagement from './RoleManagement';
import TokenSettings from './TokenSettings';
import ThemeCustomization from './ThemeCustomization';

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
        <div className="p-6">
          {children}
        </div>
      )}
    </div>
  );
}

// Role Management Panel
const RoleManagementPanel: React.FC = () => {
  return <RoleManagement />;
};

// User Management Panel
const UserManagementPanel: React.FC = () => {
  const [userTab, setUserTab] = useState(0);
  const [selectedUser, setSelectedUser] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [createUserOpen, setCreateUserOpen] = useState(false);
  const [isCreatingUser, setIsCreatingUser] = useState(false);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' as 'success' | 'error' });
  const [showPassword, setShowPassword] = useState(false);
  const [newUserForm, setNewUserForm] = useState({
    email: '',
    username: '',
    password: '',
    role: 'User'
  });

  // Mock user data
  const [users] = useState([
    { email: 'admin@example.com', username: 'admin', role: 'Admin', status: 'Active', lastLogin: '2024-01-15' },
    { email: 'moderator@example.com', username: 'moderator', role: 'Moderator', status: 'Active', lastLogin: '2024-01-14' },
    { email: 'user1@example.com', username: 'user1', role: 'User', status: 'Active', lastLogin: '2024-01-13' },
    { email: 'user2@example.com', username: 'user2', role: 'User', status: 'Banned', lastLogin: '2024-01-10' },
  ]);

  const handleUserClick = (userEmail: string) => {
    setSelectedUser(userEmail);
  };

  const handleUserAction = (action: string, userEmail: string) => {
    console.log(`${action} user: ${userEmail}`);
    setSnackbar({ open: true, message: `${action} action performed on ${userEmail}`, severity: 'success' });
  };

  const handleCreateUser = async () => {
    setIsCreatingUser(true);
    // Simulate API call
    await new Promise(resolve => setTimeout(resolve, 1000));
    setIsCreatingUser(false);
    setCreateUserOpen(false);
    setSnackbar({ open: true, message: 'User created successfully', severity: 'success' });
    // Reset form
    setNewUserForm({ email: '', username: '', password: '', role: 'User' });
  };

  const handleFormChange = (field: string, value: string) => {
    setNewUserForm(prev => ({ ...prev, [field]: value }));
  };

  const handleCloseCreateDialog = () => {
    setCreateUserOpen(false);
    setNewUserForm({ email: '', username: '', password: '', role: 'User' });
  };

  const copyPasswordToClipboard = () => {
    navigator.clipboard.writeText(newUserForm.password);
    setSnackbar({ open: true, message: 'Password copied to clipboard', severity: 'success' });
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-2xl font-bold text-white">User Management</h2>
        <button
          onClick={() => setCreateUserOpen(true)}
          className="lg-button lg-button-primary"
        >
          <AddIcon className="w-5 h-5" />
          Create User
        </button>
      </div>

      {/* Search and Filters */}
      <div className="lg-card p-4">
        <div className="flex gap-4">
          <div className="flex-1">
            <input
              type="text"
              placeholder="Search users..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="lg-input"
            />
          </div>
          <select className="lg-input w-48">
            <option>All Roles</option>
            <option>Admin</option>
            <option>Moderator</option>
            <option>User</option>
          </select>
          <select className="lg-input w-48">
            <option>All Status</option>
            <option>Active</option>
            <option>Banned</option>
            <option>Inactive</option>
          </select>
        </div>
      </div>

      {/* Users Table */}
      <div className="lg-card">
        <div className="overflow-x-auto">
          <table className="lg-table">
            <thead>
              <tr>
                <th>User</th>
                <th>Role</th>
                <th>Status</th>
                <th>Last Login</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {users.map((user) => (
                <tr key={user.email} className="cursor-pointer hover:bg-white/5">
                  <td>
                    <div className="flex items-center space-x-3">
                      <div className="w-8 h-8 bg-indigo-500 rounded-full flex items-center justify-center text-white text-sm font-medium">
                        {user.username.charAt(0).toUpperCase()}
                      </div>
                      <div>
                        <div className="font-medium text-white">{user.username}</div>
                        <div className="text-sm text-white/60">{user.email}</div>
                      </div>
                    </div>
                  </td>
                  <td>
                    <span className={`lg-badge ${
                      user.role === 'Admin' ? 'lg-badge-error' :
                      user.role === 'Moderator' ? 'lg-badge-warning' :
                      'lg-badge-info'
                    }`}>
                      {user.role}
                    </span>
                  </td>
                  <td>
                    <span className={`lg-badge ${
                      user.status === 'Active' ? 'lg-badge-success' : 'lg-badge-error'
                    }`}>
                      {user.status}
                    </span>
                  </td>
                  <td className="text-white/80">{user.lastLogin}</td>
                  <td>
                    <div className="flex space-x-2">
                      <button
                        onClick={() => handleUserAction('Edit', user.email)}
                        className="lg-button text-sm"
                      >
                        Edit
                      </button>
                      <button
                        onClick={() => handleUserAction('Ban', user.email)}
                        className="lg-button lg-button-secondary text-sm"
                      >
                        Ban
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Create User Dialog */}
      {createUserOpen && (
        <div className="lg-modal-overlay">
          <div className="lg-modal p-6 max-w-md w-full">
            <h3 className="text-xl font-bold text-white mb-4">Create New User</h3>
            
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-white/80 mb-2">Email</label>
                <input
                  type="email"
                  value={newUserForm.email}
                  onChange={(e) => handleFormChange('email', e.target.value)}
                  className="lg-input"
                  placeholder="user@example.com"
                />
              </div>
              
              <div>
                <label className="block text-sm font-medium text-white/80 mb-2">Username</label>
                <input
                  type="text"
                  value={newUserForm.username}
                  onChange={(e) => handleFormChange('username', e.target.value)}
                  className="lg-input"
                  placeholder="username"
                />
              </div>
              
              <div>
                <label className="block text-sm font-medium text-white/80 mb-2">Password</label>
                <div className="relative">
                  <input
                    type={showPassword ? 'text' : 'password'}
                    value={newUserForm.password}
                    onChange={(e) => handleFormChange('password', e.target.value)}
                    className="lg-input pr-12"
                    placeholder="Generate password"
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword(!showPassword)}
                    className="absolute right-2 top-1/2 transform -translate-y-1/2 text-white/60 hover:text-white"
                  >
                    {showPassword ? <VisibilityOff className="w-5 h-5" /> : <Visibility className="w-5 h-5" />}
                  </button>
                </div>
                <button
                  onClick={() => handleFormChange('password', Math.random().toString(36).slice(-8))}
                  className="text-sm text-indigo-400 hover:text-indigo-300 mt-1"
                >
                  Generate Password
                </button>
              </div>
              
              <div>
                <label className="block text-sm font-medium text-white/80 mb-2">Role</label>
                <select
                  value={newUserForm.role}
                  onChange={(e) => handleFormChange('role', e.target.value)}
                  className="lg-input"
                >
                  <option value="User">User</option>
                  <option value="Moderator">Moderator</option>
                  <option value="Admin">Admin</option>
                </select>
              </div>
            </div>
            
            <div className="flex justify-end space-x-3 mt-6">
              <button
                onClick={handleCloseCreateDialog}
                className="lg-button"
              >
                Cancel
              </button>
              <button
                onClick={handleCreateUser}
                disabled={isCreatingUser}
                className="lg-button lg-button-primary"
              >
                {isCreatingUser ? 'Creating...' : 'Create User'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

// Model Management Panel
const ModelManagementPanel: React.FC = () => (
  <div className="space-y-6">
    <div className="flex justify-between items-center">
      <h2 className="text-2xl font-bold text-white">Model Management</h2>
      <button className="lg-button lg-button-primary">
        <AddIcon className="w-5 h-5" />
        Upload Model
      </button>
    </div>

    <div className="lg-card p-6">
      <h3 className="text-lg font-medium text-white mb-4">Model Statistics</h3>
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="lg-card p-4">
          <div className="text-2xl font-bold text-indigo-400">1,234</div>
          <div className="text-sm text-white/60">Total Models</div>
        </div>
        <div className="lg-card p-4">
          <div className="text-2xl font-bold text-green-400">89</div>
          <div className="text-sm text-white/60">Pending Review</div>
        </div>
        <div className="lg-card p-4">
          <div className="text-2xl font-bold text-red-400">12</div>
          <div className="text-sm text-white/60">Flagged Models</div>
        </div>
      </div>
    </div>

    <div className="lg-card p-6">
      <h3 className="text-lg font-medium text-white mb-4">Recent Activity</h3>
      <div className="space-y-3">
        <div className="flex items-center justify-between p-3 bg-white/5 rounded-lg">
          <div>
            <div className="text-white font-medium">New model uploaded</div>
            <div className="text-sm text-white/60">user123 uploaded "Dragon Model"</div>
          </div>
          <span className="text-sm text-white/40">2 hours ago</span>
        </div>
        <div className="flex items-center justify-between p-3 bg-white/5 rounded-lg">
          <div>
            <div className="text-white font-medium">Model approved</div>
            <div className="text-sm text-white/60">"Robot Arm" by user456 approved</div>
          </div>
          <span className="text-sm text-white/40">4 hours ago</span>
        </div>
      </div>
    </div>
  </div>
);

// System Settings Panel
const SystemSettingsPanel: React.FC = () => {
  const { user } = useAppSelector((state) => state.auth);
  const [emailSettings, setEmailSettings] = useState({
    smtpServer: 'smtp.gmail.com',
    smtpPort: 587,
    username: '',
    password: '',
    fromEmail: '',
    enableSSL: true
  });

  const [isSaving, setIsSaving] = useState(false);
  const [isTesting, setIsTesting] = useState(false);
  const [setupStatus, setSetupStatus] = useState<any>(null);
  const [isLoadingSetup, setIsLoadingSetup] = useState(false);

  const fetchEmailSettings = async () => {
    // Mock API call
    console.log('Fetching email settings...');
  };

  const fetchSetupStatus = async () => {
    setIsLoadingSetup(true);
    try {
      const response = await api.get('/SystemSetup/status');

      if (response.status === 200) {
        const status = response.data;
        setSetupStatus(status);
      }
    } catch (error) {
      console.error('Error fetching setup status:', error);
    } finally {
      setIsLoadingSetup(false);
    }
  };

  const navigateToSetup = () => {
    window.location.href = '/setup';
  };

  useEffect(() => {
    fetchEmailSettings();
    fetchSetupStatus();
  }, []);

  const handleEmailSettingsChange = (field: string, value: any) => {
    setEmailSettings(prev => ({ ...prev, [field]: value }));
  };

  const saveEmailSettings = async () => {
    setIsSaving(true);
    // Simulate API call
    await new Promise(resolve => setTimeout(resolve, 1000));
    setIsSaving(false);
    console.log('Email settings saved:', emailSettings);
  };

  const testEmailConfiguration = async () => {
    setIsTesting(true);
    // Simulate API call
    await new Promise(resolve => setTimeout(resolve, 2000));
    setIsTesting(false);
    console.log('Testing email configuration...');
  };

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold text-white">System Settings</h2>

      {/* Setup Access */}
      <div className="lg-card p-6">
        <h3 className="text-lg font-medium text-white mb-4">First-Time Setup</h3>
        
        {isLoadingSetup ? (
          <div className="text-center py-4">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-400 mx-auto"></div>
            <p className="text-white/60 mt-2">Loading setup status...</p>
          </div>
        ) : setupStatus ? (
          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <div>
                <div className="text-white font-medium">Setup Status</div>
                <div className="text-sm text-white/60">
                  {setupStatus.isFirstTimeSetup ? 'Setup incomplete' : 'Setup completed'}
                </div>
              </div>
              <div className="text-right">
                <div className="text-white font-medium">{setupStatus.completedSteps}/{setupStatus.totalSteps}</div>
                <div className="text-sm text-white/60">Steps completed</div>
              </div>
            </div>
            
            {setupStatus.isFirstTimeSetup && (
              <div className="lg-badge-warning p-3">
                <span className="text-sm">
                  First-time setup is not complete. Some features may be limited until setup is finished.
                </span>
              </div>
            )}
            
            <div className="flex space-x-3">
              <button
                onClick={navigateToSetup}
                className="lg-button lg-button-primary"
              >
                {setupStatus.isFirstTimeSetup ? 'Continue Setup' : 'Access Setup'}
              </button>
              <button
                onClick={fetchSetupStatus}
                className="lg-button lg-button-secondary"
              >
                Refresh Status
              </button>
            </div>
          </div>
        ) : (
          <div className="text-center py-4">
            <p className="text-white/60">Unable to load setup status</p>
            <button
              onClick={fetchSetupStatus}
              className="lg-button lg-button-secondary mt-2"
            >
              Retry
            </button>
          </div>
        )}
      </div>

      {/* Email Configuration */}
      <div className="lg-card p-6">
        <h3 className="text-lg font-medium text-white mb-4">Email Configuration</h3>
        
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-white/80 mb-2">SMTP Server</label>
            <input
              type="text"
              value={emailSettings.smtpServer}
              onChange={(e) => handleEmailSettingsChange('smtpServer', e.target.value)}
              className="lg-input"
            />
          </div>
          
          <div>
            <label className="block text-sm font-medium text-white/80 mb-2">SMTP Port</label>
            <input
              type="number"
              value={emailSettings.smtpPort}
              onChange={(e) => handleEmailSettingsChange('smtpPort', parseInt(e.target.value))}
              className="lg-input"
            />
          </div>
          
          <div>
            <label className="block text-sm font-medium text-white/80 mb-2">Username</label>
            <input
              type="text"
              value={emailSettings.username}
              onChange={(e) => handleEmailSettingsChange('username', e.target.value)}
              className="lg-input"
            />
          </div>
          
          <div>
            <label className="block text-sm font-medium text-white/80 mb-2">Password</label>
            <input
              type="password"
              value={emailSettings.password}
              onChange={(e) => handleEmailSettingsChange('password', e.target.value)}
              className="lg-input"
            />
          </div>
          
          <div>
            <label className="block text-sm font-medium text-white/80 mb-2">From Email</label>
            <input
              type="email"
              value={emailSettings.fromEmail}
              onChange={(e) => handleEmailSettingsChange('fromEmail', e.target.value)}
              className="lg-input"
            />
          </div>
          
          <div className="flex items-center">
            <input
              type="checkbox"
              id="enableSSL"
              checked={emailSettings.enableSSL}
              onChange={(e) => handleEmailSettingsChange('enableSSL', e.target.checked)}
              className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
            />
            <label htmlFor="enableSSL" className="ml-2 text-sm text-white/80">
              Enable SSL/TLS
            </label>
          </div>
        </div>
        
        <div className="flex space-x-3 mt-6">
          <button
            onClick={saveEmailSettings}
            disabled={isSaving}
            className="lg-button lg-button-primary"
          >
            {isSaving ? 'Saving...' : 'Save Settings'}
          </button>
          <button
            onClick={testEmailConfiguration}
            disabled={isTesting}
            className="lg-button lg-button-secondary"
          >
            {isTesting ? 'Testing...' : 'Test Configuration'}
          </button>
        </div>
      </div>

      {/* Storage Settings */}
      <div className="lg-card p-6">
        <h3 className="text-lg font-medium text-white mb-4">Storage Settings</h3>
        
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-white/80 mb-2">Storage Provider</label>
            <select className="lg-input">
              <option value="local">Local Storage</option>
              <option value="s3">Amazon S3</option>
              <option value="azure">Azure Blob Storage</option>
              <option value="gcp">Google Cloud Storage</option>
            </select>
          </div>
          
          <div>
            <label className="block text-sm font-medium text-white/80 mb-2">Storage Path</label>
            <input
              type="text"
              className="lg-input"
              placeholder="/var/www/uploads"
            />
          </div>
          
          <div>
            <label className="block text-sm font-medium text-white/80 mb-2">Max File Size (MB)</label>
            <input
              type="number"
              className="lg-input"
              defaultValue={100}
            />
          </div>
        </div>
      </div>

      {/* Security Settings */}
      <div className="lg-card p-6">
        <h3 className="text-lg font-medium text-white mb-4">Security Settings</h3>
        
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <div>
              <div className="text-white font-medium">Two-Factor Authentication</div>
              <div className="text-sm text-white/60">Require 2FA for admin accounts</div>
            </div>
            <input
              type="checkbox"
              className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
            />
          </div>
          
          <div className="flex items-center justify-between">
            <div>
              <div className="text-white font-medium">Session Timeout</div>
              <div className="text-sm text-white/60">Auto-logout after inactivity</div>
            </div>
            <select className="lg-input w-32">
              <option value="30">30 min</option>
              <option value="60">1 hour</option>
              <option value="120">2 hours</option>
              <option value="0">Never</option>
            </select>
          </div>
          
          <div className="flex items-center justify-between">
            <div>
              <div className="text-white font-medium">Rate Limiting</div>
              <div className="text-sm text-white/60">Limit API requests per minute</div>
            </div>
            <input
              type="number"
              className="lg-input w-32"
              defaultValue={100}
            />
          </div>
        </div>
      </div>
    </div>
  );
};

// Authentication Settings Panel
const AuthenticationSettingsPanel: React.FC = () => (
  <div className="space-y-6">
    <h2 className="text-2xl font-bold text-white">Authentication Settings</h2>
    
    <div className="lg-card p-6">
      <h3 className="text-lg font-medium text-white mb-4">OAuth Providers</h3>
      
      <div className="space-y-4">
        <div className="flex items-center justify-between p-4 bg-white/5 rounded-lg">
          <div className="flex items-center space-x-3">
            <div className="w-8 h-8 bg-blue-500 rounded flex items-center justify-center">
              <span className="text-white font-bold">G</span>
            </div>
            <div>
              <div className="text-white font-medium">Google OAuth</div>
              <div className="text-sm text-white/60">Sign in with Google</div>
            </div>
          </div>
          <input
            type="checkbox"
            className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
          />
        </div>
        
        <div className="flex items-center justify-between p-4 bg-white/5 rounded-lg">
          <div className="flex items-center space-x-3">
            <div className="w-8 h-8 bg-black rounded flex items-center justify-center">
              <span className="text-white font-bold">G</span>
            </div>
            <div>
              <div className="text-white font-medium">GitHub OAuth</div>
              <div className="text-sm text-white/60">Sign in with GitHub</div>
            </div>
          </div>
          <input
            type="checkbox"
            className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
          />
        </div>
      </div>
    </div>
    
    <div className="lg-card p-6">
      <h3 className="text-lg font-medium text-white mb-4">Password Policy</h3>
      
      <div className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-white/80 mb-2">Minimum Password Length</label>
          <input
            type="number"
            className="lg-input w-32"
            defaultValue={8}
          />
        </div>
        
        <div className="flex items-center">
          <input
            type="checkbox"
            id="requireUppercase"
            className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
          />
          <label htmlFor="requireUppercase" className="ml-2 text-sm text-white/80">
            Require uppercase letters
          </label>
        </div>
        
        <div className="flex items-center">
          <input
            type="checkbox"
            id="requireNumbers"
            className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
          />
          <label htmlFor="requireNumbers" className="ml-2 text-sm text-white/80">
            Require numbers
          </label>
        </div>
        
        <div className="flex items-center">
          <input
            type="checkbox"
            id="requireSpecialChars"
            className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
          />
          <label htmlFor="requireSpecialChars" className="ml-2 text-sm text-white/80">
            Require special characters
          </label>
        </div>
      </div>
    </div>
  </div>
);

// Token Settings Panel
const TokenSettingsPanel: React.FC = () => (
  <div className="space-y-6">
    <h2 className="text-2xl font-bold text-white">Token Settings</h2>
    <TokenSettings />
  </div>
);

// Comprehensive Plugin Management Panel
const PluginsPanel: React.FC = () => {
  const [plugins, setPlugins] = useState([
    { id: 'comments', name: 'Comments Plugin', status: 'enabled', version: '1.0.0' },
    { id: 'federation', name: 'Federation Plugin', status: 'disabled', version: '1.2.0' },
    { id: 'analytics', name: 'Analytics Plugin', status: 'enabled', version: '2.1.0' },
  ]);

  const [selectedPlugin, setSelectedPlugin] = useState<string | null>(null);
  const [settingsOpen, setSettingsOpen] = useState(false);
  const [hooksOpen, setHooksOpen] = useState(false);

  const loadPluginData = async () => {
    // Mock API call
    console.log('Loading plugin data...');
  };

  useEffect(() => {
    loadPluginData();
  }, []);

  const handlePluginSelect = async (pluginId: string) => {
    setSelectedPlugin(pluginId);
    console.log('Selected plugin:', pluginId);
  };

  const handleTogglePlugin = async (pluginId: string, currentStatus: string) => {
    const newStatus = currentStatus === 'enabled' ? 'disabled' : 'enabled';
    setPlugins(prev => prev.map(plugin => 
      plugin.id === pluginId ? { ...plugin, status: newStatus } : plugin
    ));
    console.log(`Plugin ${pluginId} ${newStatus}`);
  };

  const handleOpenSettings = async (pluginId: string) => {
    setSelectedPlugin(pluginId);
    setSettingsOpen(true);
  };

  const handleOpenHooks = async () => {
    setHooksOpen(true);
  };

  const handleSaveSettings = async (settings: Record<string, any>) => {
    console.log('Saving plugin settings:', settings);
    setSettingsOpen(false);
  };

  const handleReloadPlugins = async () => {
    console.log('Reloading plugins...');
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-2xl font-bold text-white">Plugin Management</h2>
        <button
          onClick={handleReloadPlugins}
          className="lg-button lg-button-secondary"
        >
          Reload Plugins
        </button>
      </div>

      <div className="lg-card p-6">
        <h3 className="text-lg font-medium text-white mb-4">Installed Plugins</h3>
        
        <div className="space-y-4">
          {plugins.map((plugin) => (
            <div key={plugin.id} className="flex items-center justify-between p-4 bg-white/5 rounded-lg">
              <div className="flex items-center space-x-3">
                <div className="w-10 h-10 bg-indigo-500 rounded flex items-center justify-center">
                  <ExtensionIcon className="w-6 h-6 text-white" />
                </div>
                <div>
                  <div className="text-white font-medium">{plugin.name}</div>
                  <div className="text-sm text-white/60">Version {plugin.version}</div>
                </div>
              </div>
              
              <div className="flex items-center space-x-2">
                <span className={`lg-badge ${
                  plugin.status === 'enabled' ? 'lg-badge-success' : 'lg-badge-error'
                }`}>
                  {plugin.status}
                </span>
                
                <button
                  onClick={() => handleTogglePlugin(plugin.id, plugin.status)}
                  className={`lg-button text-sm ${
                    plugin.status === 'enabled' ? 'lg-button-secondary' : 'lg-button-primary'
                  }`}
                >
                  {plugin.status === 'enabled' ? 'Disable' : 'Enable'}
                </button>
                
                <button
                  onClick={() => handleOpenSettings(plugin.id)}
                  className="lg-button text-sm"
                >
                  Settings
                </button>
              </div>
            </div>
          ))}
        </div>
      </div>

      <div className="lg-card p-6">
        <h3 className="text-lg font-medium text-white mb-4">Plugin Hooks</h3>
        <p className="text-white/60 mb-4">
          Configure how plugins interact with the system through hooks and events.
        </p>
        <button
          onClick={handleOpenHooks}
          className="lg-button lg-button-primary"
        >
          Configure Hooks
        </button>
      </div>

      {/* Plugin Settings Dialog */}
      {settingsOpen && selectedPlugin && (
        <div className="lg-modal-overlay">
          <div className="lg-modal p-6 max-w-2xl w-full">
            <h3 className="text-xl font-bold text-white mb-4">
              {plugins.find(p => p.id === selectedPlugin)?.name} Settings
            </h3>
            
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-white/80 mb-2">API Key</label>
                <input
                  type="text"
                  className="lg-input"
                  placeholder="Enter API key..."
                />
              </div>
              
              <div>
                <label className="block text-sm font-medium text-white/80 mb-2">Webhook URL</label>
                <input
                  type="url"
                  className="lg-input"
                  placeholder="https://example.com/webhook"
                />
              </div>
              
              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="enableDebug"
                  className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
                />
                <label htmlFor="enableDebug" className="ml-2 text-sm text-white/80">
                  Enable debug mode
                </label>
              </div>
            </div>
            
            <div className="flex justify-end space-x-3 mt-6">
              <button
                onClick={() => setSettingsOpen(false)}
                className="lg-button"
              >
                Cancel
              </button>
              <button
                onClick={() => handleSaveSettings({})}
                className="lg-button lg-button-primary"
              >
                Save Settings
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Plugin Hooks Dialog */}
      {hooksOpen && (
        <div className="lg-modal-overlay">
          <div className="lg-modal p-6 max-w-4xl w-full">
            <h3 className="text-xl font-bold text-white mb-4">Plugin Hooks Configuration</h3>
            
            <div className="space-y-4">
              <div className="lg-card p-4">
                <h4 className="text-lg font-medium text-white mb-2">Model Events</h4>
                <div className="space-y-2">
                  <div className="flex items-center justify-between">
                    <span className="text-white/80">onModelUpload</span>
                    <select className="lg-input w-48">
                      <option>Comments Plugin</option>
                      <option>Analytics Plugin</option>
                      <option>None</option>
                    </select>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-white/80">onModelDelete</span>
                    <select className="lg-input w-48">
                      <option>Analytics Plugin</option>
                      <option>Comments Plugin</option>
                      <option>None</option>
                    </select>
                  </div>
                </div>
              </div>
              
              <div className="lg-card p-4">
                <h4 className="text-lg font-medium text-white mb-2">User Events</h4>
                <div className="space-y-2">
                  <div className="flex items-center justify-between">
                    <span className="text-white/80">onUserLogin</span>
                    <select className="lg-input w-48">
                      <option>Analytics Plugin</option>
                      <option>None</option>
                    </select>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-white/80">onUserRegister</span>
                    <select className="lg-input w-48">
                      <option>Analytics Plugin</option>
                      <option>None</option>
                    </select>
                  </div>
                </div>
              </div>
            </div>
            
            <div className="flex justify-end space-x-3 mt-6">
              <button
                onClick={() => setHooksOpen(false)}
                className="lg-button"
              >
                Cancel
              </button>
              <button
                onClick={() => setHooksOpen(false)}
                className="lg-button lg-button-primary"
              >
                Save Configuration
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

// Analytics Panel
const AnalyticsPanel: React.FC = () => (
  <div className="space-y-6">
    <h2 className="text-2xl font-bold text-white">Analytics Dashboard</h2>
    
    <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
      <div className="lg-card p-4">
        <div className="text-2xl font-bold text-indigo-400">1,234</div>
        <div className="text-sm text-white/60">Total Users</div>
      </div>
      <div className="lg-card p-4">
        <div className="text-2xl font-bold text-green-400">5,678</div>
        <div className="text-sm text-white/60">Total Models</div>
      </div>
      <div className="lg-card p-4">
        <div className="text-2xl font-bold text-yellow-400">12,345</div>
        <div className="text-sm text-white/60">Downloads</div>
      </div>
      <div className="lg-card p-4">
        <div className="text-2xl font-bold text-blue-400">89%</div>
        <div className="text-sm text-white/60">Uptime</div>
      </div>
    </div>
    
    <div className="lg-card p-6">
      <h3 className="text-lg font-medium text-white mb-4">Usage Analytics</h3>
      <div className="lg-badge-info p-4">
        <span className="text-lg">Analytics dashboard will be implemented here. This will include charts for user activity, 
        model uploads over time, popular models, and system performance metrics.</span>
      </div>
    </div>
  </div>
);

const ComprehensiveAdminPanel: React.FC = () => {
  const [tabValue, setTabValue] = useState(0);

  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  const tabs = [
    { label: 'Dashboard', icon: <DashboardIcon />, component: <AnalyticsPanel /> },
    { label: 'Users', icon: <PeopleIcon />, component: <UserManagementPanel /> },
    { label: 'Roles', icon: <RoleIcon />, component: <RoleManagementPanel /> },
    { label: 'Models', icon: <ModelIcon />, component: <ModelManagementPanel /> },
    { label: 'System', icon: <SettingsIcon />, component: <SystemSettingsPanel /> },
    { label: 'Auth', icon: <SecurityIcon />, component: <AuthenticationSettingsPanel /> },
    { label: 'Tokens', icon: <SecurityIcon />, component: <TokenSettingsPanel /> },
    { label: 'Theme', icon: <PaletteIcon />, component: <ThemeCustomization /> },
    { label: 'Plugins', icon: <ExtensionIcon />, component: <PluginsPanel /> },
  ];

  return (
    <div className="lg-container min-h-screen">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="lg-card p-6">
          <div className="flex justify-between items-center mb-6">
            <h1 className="text-3xl font-bold text-white">
              Admin Control Panel
            </h1>
            <div className="flex items-center gap-3">
              <div className="relative">
                <NotificationIcon className="w-6 h-6 text-white/80" />
                <span className="absolute -top-1 -right-1 w-4 h-4 bg-yellow-500 rounded-full text-xs text-white flex items-center justify-center">
                  3
                </span>
              </div>
              <span className="lg-badge lg-badge-warning">Development Mode</span>
            </div>
          </div>
          
          <div className="border-b border-white/10 mb-6">
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
          
          {tabs.map((tab, index) => (
            <TabPanel key={index} value={tabValue} index={index}>
              {tab.component}
            </TabPanel>
          ))}
        </div>
      </div>
    </div>
  );
};

export default ComprehensiveAdminPanel; 