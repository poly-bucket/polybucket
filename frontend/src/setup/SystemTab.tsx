import React, { useState, useEffect } from 'react';
import { useAppSelector } from '../utils/hooks';
import api from '../utils/axiosConfig';

const SystemTab: React.FC = () => {
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
            <div className="lg-spinner h-8 w-8 mx-auto"></div>
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

export default SystemTab;
