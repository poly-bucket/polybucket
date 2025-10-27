import React, { useState, useEffect } from 'react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faKey, faCheckCircle, faExclamationTriangle, faInfoCircle } from '@fortawesome/free-solid-svg-icons';

interface FontAwesomeSettings {
  isProEnabled: boolean;
  proLicenseKey: string;
  proKitUrl: string;
  useProIcons: boolean;
  fallbackToFree: boolean;
}

const FontAwesomeSettings: React.FC = () => {
  const [settings, setSettings] = useState<FontAwesomeSettings>({
    isProEnabled: false,
    proLicenseKey: '',
    proKitUrl: '',
    useProIcons: true,
    fallbackToFree: true
  });
  
  const [isLoading, setIsLoading] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [message, setMessage] = useState<{ type: 'success' | 'error' | 'info'; text: string } | null>(null);

  useEffect(() => {
    loadSettings();
  }, []);

  const loadSettings = async () => {
    setIsLoading(true);
    try {
      // TODO: Replace with actual API call
      const response = await fetch('/api/admin/fontawesome-settings');
      if (response.ok) {
        const data = await response.json();
        setSettings(data);
      }
    } catch (error) {
      console.warn('Failed to load FontAwesome settings:', error);
      // Use default settings for now
    } finally {
      setIsLoading(false);
    }
  };

  const handleSave = async () => {
    setIsSaving(true);
    setMessage(null);
    
    try {
      // TODO: Replace with actual API call
      const response = await fetch('/api/admin/fontawesome-settings', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(settings),
      });
      
      if (response.ok) {
        setMessage({ type: 'success', text: 'FontAwesome settings saved successfully!' });
      } else {
        throw new Error('Failed to save settings');
      }
    } catch (error) {
      setMessage({ type: 'error', text: 'Failed to save FontAwesome settings. Please try again.' });
    } finally {
      setIsSaving(false);
    }
  };

  const handleTestLicense = async () => {
    if (!settings.proLicenseKey.trim()) {
      setMessage({ type: 'error', text: 'Please enter a license key to test.' });
      return;
    }

    setIsLoading(true);
    setMessage(null);
    
    try {
      // TODO: Replace with actual API call
      const response = await fetch('/api/admin/fontawesome-settings/test-license', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ licenseKey: settings.proLicenseKey }),
      });
      
      if (response.ok) {
        const result = await response.json();
        if (result.isValid) {
          setMessage({ type: 'success', text: 'License key is valid! Pro icons are now available.' });
          setSettings(prev => ({ ...prev, isProEnabled: true }));
        } else {
          setMessage({ type: 'error', text: 'Invalid license key. Please check and try again.' });
        }
      } else {
        throw new Error('Failed to test license');
      }
    } catch (error) {
      setMessage({ type: 'error', text: 'Failed to test license key. Please try again.' });
    } finally {
      setIsLoading(false);
    }
  };

  const handleInputChange = (field: keyof FontAwesomeSettings, value: string | boolean) => {
    setSettings(prev => ({ ...prev, [field]: value }));
  };

  if (isLoading) {
    return (
      <div className="lg-card p-6">
        <div className="animate-pulse">
          <div className="h-6 bg-gray-700 rounded w-1/3 mb-4"></div>
          <div className="space-y-3">
            <div className="h-4 bg-gray-700 rounded"></div>
            <div className="h-4 bg-gray-700 rounded w-5/6"></div>
            <div className="h-4 bg-gray-700 rounded w-4/6"></div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="lg-card p-6">
      <div className="flex items-center mb-6">
        <FontAwesomeIcon icon={faKey} className="w-6 h-6 text-blue-400 mr-3" />
        <h3 className="text-lg font-medium text-white">FontAwesome Configuration</h3>
      </div>

      {/* Status Indicator */}
      <div className="mb-6 p-4 rounded-lg border border-gray-600">
        <div className="flex items-center">
          {settings.isProEnabled ? (
            <FontAwesomeIcon icon={faCheckCircle} className="w-5 h-5 text-green-400 mr-2" />
          ) : (
            <FontAwesomeIcon icon={faExclamationTriangle} className="w-5 h-5 text-yellow-400 mr-2" />
          )}
          <span className="text-white font-medium">
            {settings.isProEnabled ? 'Pro Icons Enabled' : 'Free Icons Only'}
          </span>
        </div>
        <p className="text-sm text-gray-400 mt-1">
          {settings.isProEnabled 
            ? 'Your FontAwesome Pro license is active. Users can access all Pro icons.'
            : 'Only free icons are available. Upgrade to Pro for access to premium icon sets.'
          }
        </p>
      </div>

      {/* License Key Configuration */}
      <div className="space-y-4 mb-6">
        <div>
          <label className="block text-sm font-medium text-white mb-2">
            FontAwesome Pro License Key
          </label>
          <div className="flex space-x-2">
            <input
              type="password"
              value={settings.proLicenseKey}
              onChange={(e) => handleInputChange('proLicenseKey', e.target.value)}
              placeholder="Enter your Pro license key"
              className="lg-input flex-1"
            />
            <button
              onClick={handleTestLicense}
              disabled={!settings.proLicenseKey.trim() || isLoading}
              className="lg-button lg-button-secondary disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Test License
            </button>
          </div>
          <p className="mt-1 text-sm text-gray-400">
            Get your license key from{' '}
            <a 
              href="https://fontawesome.com/pro" 
              target="_blank" 
              rel="noopener noreferrer"
              className="text-blue-400 hover:text-blue-300 underline"
            >
              FontAwesome Pro
            </a>
          </p>
        </div>

        <div>
          <label className="block text-sm font-medium text-white mb-2">
            Pro Kit URL (Optional)
          </label>
          <input
            type="url"
            value={settings.proKitUrl}
            onChange={(e) => handleInputChange('proKitUrl', e.target.value)}
            placeholder="https://kit.fontawesome.com/your-kit-id.js"
            className="lg-input"
          />
          <p className="mt-1 text-sm text-gray-400">
            If you have a custom FontAwesome kit, enter the URL here
          </p>
        </div>
      </div>

      {/* Behavior Settings */}
      <div className="space-y-4 mb-6">
        <h4 className="text-md font-medium text-white">Icon Behavior</h4>
        
        <div className="flex items-center justify-between">
          <div>
            <div className="text-white font-medium">Use Pro Icons When Available</div>
            <div className="text-sm text-gray-400">Automatically use Pro icons for better icon variety</div>
          </div>
          <input
            type="checkbox"
            checked={settings.useProIcons}
            onChange={(e) => handleInputChange('useProIcons', e.target.checked)}
            className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded bg-transparent"
          />
        </div>
        
        <div className="flex items-center justify-between">
          <div>
            <div className="text-white font-medium">Fallback to Free Icons</div>
            <div className="text-sm text-gray-400">Show free alternatives when Pro icons aren't available</div>
          </div>
          <input
            type="checkbox"
            checked={settings.fallbackToFree}
            onChange={(e) => handleInputChange('fallbackToFree', e.target.checked)}
            className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded bg-transparent"
          />
        </div>
      </div>

      {/* Information */}
      <div className="mb-6 p-4 bg-blue-900/20 border border-blue-600/30 rounded-lg">
        <div className="flex items-start">
          <FontAwesomeIcon icon={faInfoCircle} className="w-5 h-5 text-blue-400 mr-2 mt-0.5 flex-shrink-0" />
          <div>
            <h5 className="text-sm font-medium text-blue-300 mb-1">About FontAwesome Pro</h5>
            <p className="text-sm text-blue-200">
              FontAwesome Pro provides access to over 7,000 additional icons, including solid, regular, 
              light, thin, and duotone styles. This gives your users more options to personalize their 
              collections with unique and professional-looking icons.
            </p>
          </div>
        </div>
      </div>

      {/* Message Display */}
      {message && (
        <div className={`mb-4 p-4 rounded-lg border ${
          message.type === 'success' 
            ? 'bg-green-900/20 border-green-600/30 text-green-200'
            : message.type === 'error'
            ? 'bg-red-900/20 border-red-600/30 text-red-200'
            : 'bg-blue-900/20 border-blue-600/30 text-blue-200'
        }`}>
          {message.text}
        </div>
      )}

      {/* Actions */}
      <div className="flex justify-end space-x-3">
        <button
          onClick={handleSave}
          disabled={isSaving}
          className="lg-button lg-button-primary disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {isSaving ? 'Saving...' : 'Save Settings'}
        </button>
      </div>
    </div>
  );
};

export default FontAwesomeSettings;
