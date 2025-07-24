import React from 'react';
import { CheckCircleIcon } from '@heroicons/react/24/outline';

interface SetupCompleteProps {
  onComplete: () => void;
}

const SetupComplete: React.FC<SetupCompleteProps> = ({ onComplete }) => {
  return (
    <div className="lg-container min-h-screen flex flex-col justify-center py-12 sm:px-6 lg:px-8">
      <div className="sm:mx-auto sm:w-full sm:max-w-md">
        <div className="text-center">
          <CheckCircleIcon className="mx-auto h-16 w-16 text-green-400" />
          <h2 className="mt-6 text-center text-3xl font-extrabold text-white">
            Setup Complete!
          </h2>
          <p className="mt-2 text-center text-sm text-gray-300">
            Your PolyBucket site has been configured successfully.
          </p>
        </div>
      </div>

      <div className="mt-8 sm:mx-auto sm:w-full sm:max-w-md">
        <div className="lg-card p-8">
          <div className="text-center space-y-4">
            <p className="text-sm text-gray-300">
              You can now start using your PolyBucket site. All settings can be modified later in the admin panel.
            </p>
            
            <button
              onClick={onComplete}
              className="lg-button lg-button-primary w-full"
            >
              Go to Dashboard
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default SetupComplete; 