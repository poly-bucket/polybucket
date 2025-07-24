import React, { useState, useEffect } from 'react';
import { useAppSelector } from '../../store';

interface TokenSettings {
  accessTokenExpiryHours: number;
  refreshTokenExpiryDays: number;
  enableRefreshTokens: boolean;
}

const TokenSettings: React.FC = () => {
  const { user } = useAppSelector((state) => state.auth);
  const [settings, setSettings] = useState<TokenSettings>({
    accessTokenExpiryHours: 1,
    refreshTokenExpiryDays: 7,
    enableRefreshTokens: true
  });

  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [success, setSuccess] = useState('');
  const [error, setError] = useState('');

  useEffect(() => {
    loadSettings();
  }, []);

  const loadSettings = async () => {
    try {
      setLoading(true);
      setError('');

      const response = await fetch('/api/system-settings/token', {
        headers: {
          'Authorization': `Bearer ${user?.accessToken}`,
          'Content-Type': 'application/json'
        }
      });

      if (response.ok) {
        const data = await response.json();
        setSettings(data);
      } else {
        setError('Failed to load token settings');
      }
    } catch (err) {
      setError('Failed to load token settings');
      console.error('Error loading token settings:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleSave = async () => {
    try {
      setSaving(true);
      setError('');
      setSuccess('');

      const response = await fetch('/api/system-settings/token', {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${user?.accessToken}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(settings)
      });

      if (response.ok) {
        setSuccess('Token settings saved successfully');
      } else {
        const errorData = await response.json();
        setError(errorData.message || 'Failed to save token settings');
      }
    } catch (err) {
      setError('Failed to save token settings');
      console.error('Error saving token settings:', err);
    } finally {
      setSaving(false);
    }
  };

  const handleSettingChange = (key: keyof TokenSettings, value: any) => {
    setSettings(prev => ({
      ...prev,
      [key]: value
    }));
  };

  if (loading) {
    return (
      <div className="lg-card p-6">
        <div className="text-center py-4">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-400 mx-auto"></div>
          <p className="text-white/60 mt-2">Loading token settings...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="lg-card p-6">
      <h3 className="text-lg font-medium text-white mb-4">Token Settings</h3>
      
      {error && (
        <div className="mb-4 p-3 bg-red-500/10 border border-red-500/20 rounded-md">
          <p className="text-red-400 text-sm">{error}</p>
        </div>
      )}
      
      {success && (
        <div className="mb-4 p-3 bg-green-500/10 border border-green-500/20 rounded-md">
          <p className="text-green-400 text-sm">{success}</p>
        </div>
      )}

      <div className="space-y-6">
        {/* Access Token Expiry */}
        <div>
          <label className="block text-sm font-medium text-white/80 mb-2">
            Access Token Expiry (Hours)
          </label>
          <div className="flex items-center space-x-4">
            <input
              type="number"
              min="1"
              max="24"
              value={settings.accessTokenExpiryHours}
              onChange={(e) => handleSettingChange('accessTokenExpiryHours', parseInt(e.target.value))}
              className="lg-input w-32"
            />
            <span className="text-white/60 text-sm">
              Current: {settings.accessTokenExpiryHours} hour{settings.accessTokenExpiryHours !== 1 ? 's' : ''}
            </span>
          </div>
          <p className="text-white/40 text-xs mt-1">
            How long access tokens remain valid (1-24 hours)
          </p>
        </div>

        {/* Refresh Token Expiry */}
        <div>
          <label className="block text-sm font-medium text-white/80 mb-2">
            Refresh Token Expiry (Days)
          </label>
          <div className="flex items-center space-x-4">
            <input
              type="number"
              min="1"
              max="365"
              value={settings.refreshTokenExpiryDays}
              onChange={(e) => handleSettingChange('refreshTokenExpiryDays', parseInt(e.target.value))}
              className="lg-input w-32"
            />
            <span className="text-white/60 text-sm">
              Current: {settings.refreshTokenExpiryDays} day{settings.refreshTokenExpiryDays !== 1 ? 's' : ''}
            </span>
          </div>
          <p className="text-white/40 text-xs mt-1">
            How long refresh tokens remain valid (1-365 days)
          </p>
        </div>

        {/* Enable Refresh Tokens */}
        <div className="flex items-center justify-between">
          <div>
            <div className="text-white font-medium">Enable Refresh Tokens</div>
            <div className="text-sm text-white/60">
              Allow users to refresh their access tokens automatically
            </div>
          </div>
          <input
            type="checkbox"
            checked={settings.enableRefreshTokens}
            onChange={(e) => handleSettingChange('enableRefreshTokens', e.target.checked)}
            className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
          />
        </div>

        {/* Current Configuration Summary */}
        <div className="bg-white/5 p-4 rounded-md">
          <h4 className="text-white font-medium mb-2">Current Configuration</h4>
          <div className="space-y-1 text-sm text-white/60">
            <div>• Access tokens expire after: {settings.accessTokenExpiryHours} hour{settings.accessTokenExpiryHours !== 1 ? 's' : ''}</div>
            <div>• Refresh tokens expire after: {settings.refreshTokenExpiryDays} day{settings.refreshTokenExpiryDays !== 1 ? 's' : ''}</div>
            <div>• Refresh tokens are: {settings.enableRefreshTokens ? 'enabled' : 'disabled'}</div>
          </div>
        </div>

        {/* Save Button */}
        <div className="flex justify-end">
          <button
            onClick={handleSave}
            disabled={saving}
            className="lg-button lg-button-primary disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {saving ? 'Saving...' : 'Save Settings'}
          </button>
        </div>
      </div>
    </div>
  );
};

export default TokenSettings; 