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
          <div className="mx-auto h-16 w-16 relative">
            <CheckCircleIcon className="h-16 w-16 text-green-400 animate-scale-in" />
            <svg
              className="absolute inset-0 h-16 w-16 text-green-400"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              strokeWidth={2}
            >
              <circle
                cx="12"
                cy="12"
                r="10"
                className="stroke-green-400 opacity-20"
                style={{
                  strokeDasharray: '62.83',
                  strokeDashoffset: '62.83',
                  animation: 'drawCircle 0.6s ease-out 0.2s forwards'
                }}
              />
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M9 12.75 11.25 15 15 9.75"
                className="stroke-green-400"
                style={{
                  strokeDasharray: '10',
                  strokeDashoffset: '10',
                  animation: 'drawCheck 0.4s ease-out 0.8s forwards'
                }}
              />
            </svg>
          </div>
          <style>{`
            @keyframes scale-in {
              0% {
                transform: scale(0);
                opacity: 0;
              }
              50% {
                transform: scale(1.1);
              }
              100% {
                transform: scale(1);
                opacity: 1;
              }
            }
            @keyframes drawCircle {
              to {
                stroke-dashoffset: 0;
              }
            }
            @keyframes drawCheck {
              to {
                stroke-dashoffset: 0;
              }
            }
            @keyframes fade-in {
              from {
                opacity: 0;
                transform: translateY(-10px);
              }
              to {
                opacity: 1;
                transform: translateY(0);
              }
            }
            .animate-scale-in {
              animation: scale-in 0.5s ease-out forwards;
            }
            .animate-fade-in {
              animation: fade-in 0.5s ease-out 1s forwards;
              opacity: 0;
            }
            .animate-fade-in-delay {
              animation: fade-in 0.5s ease-out 1.2s forwards;
              opacity: 0;
            }
          `}</style>
          <h2 className="mt-6 text-center text-3xl font-extrabold text-white animate-fade-in">
            Setup Complete!
          </h2>
          <p className="mt-2 text-center text-sm text-gray-300 animate-fade-in-delay">
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