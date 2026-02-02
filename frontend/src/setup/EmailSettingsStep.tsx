import React, { useState } from 'react';
import { useAppSelector } from '../utils/hooks';

interface EmailSettingsStepProps {
  onComplete: (data: any) => void;
  onBack: () => void;
}

const EmailSettingsStep: React.FC<EmailSettingsStepProps> = ({ onComplete, onBack }) => {
  const { user } = useAppSelector((state) => state.auth);
  const [emailSettings, setEmailSettings] = useState({
    smtpServer: '',
    smtpPort: 587,
    smtpUsername: '',
    smtpPassword: '',
    fromEmail: '',
    fromName: ''
  });
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError(null);

    try {
      const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:11666';
      const response = await fetch(`${apiUrl}/api/SystemSetup/email-settings`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${user?.accessToken}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(emailSettings)
      });

      if (response.ok) {
        onComplete({ emailSettings });
      } else {
        const errorData = await response.json();
        setError(errorData.message || 'Failed to save email settings');
      }
    } catch (error) {
      setError('Failed to save email settings');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="space-y-6">
      <div className="text-center">
        <h3 className="text-lg font-medium text-gray-900 mb-2">
          Email Configuration
        </h3>
        <p className="text-sm text-gray-600">
          Email settings can be configured later in the admin panel.
        </p>
      </div>

      <div className="flex justify-between">
        <button
          type="button"
          onClick={onBack}
          className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50"
        >
          Back
        </button>
        <button
          type="button"
          onClick={() => onComplete({ emailConfigured: true })}
          disabled={isLoading}
          className="px-4 py-2 text-sm font-medium text-white bg-blue-600 border border-transparent rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50"
        >
          {isLoading ? 'Saving...' : 'Skip & Continue'}
        </button>
      </div>
    </div>
  );
};

export default EmailSettingsStep; 