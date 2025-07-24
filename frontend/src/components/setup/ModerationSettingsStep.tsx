import React from 'react';
import { useNavigate } from 'react-router-dom';

interface ModerationSettingsStepProps {
  onComplete: (data: any) => void;
  onBack: () => void;
  data: any;
  isLoading: boolean;
  isFirstStep: boolean;
  isLastStep: boolean;
}

const ModerationSettingsStep: React.FC<ModerationSettingsStepProps> = ({
  onComplete,
  onBack,
  isLoading
}) => {
  const navigate = useNavigate();
  
  const handleContinue = () => {
    onComplete({ moderationConfigured: true });
  };

  const handleModerationSettings = () => {
    navigate('/admin/moderation-settings');
  };

  return (
    <div className="space-y-6">
      <div className="text-center">
        <h3 className="text-lg font-medium text-gray-900 mb-2">
          Moderation Settings
        </h3>
        <p className="text-sm text-gray-600">
          Moderation settings can be configured later in the admin panel.
        </p>
        <button
          type="button"
          onClick={handleModerationSettings}
          className="mt-4 px-4 py-2 text-sm font-medium text-blue-600 bg-white border border-blue-600 rounded-md hover:bg-blue-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
        >
          Configure Moderation Settings
        </button>
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
          onClick={handleContinue}
          disabled={isLoading}
          className="px-4 py-2 text-sm font-medium text-white bg-blue-600 border border-transparent rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 flex items-center gap-2"
        >
          {isLoading ? 'Saving...' : (
            <>
              Dashboard
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
              </svg>
            </>
          )}
        </button>
      </div>
    </div>
  );
};

export default ModerationSettingsStep; 