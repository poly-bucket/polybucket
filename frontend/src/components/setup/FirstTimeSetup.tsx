import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppSelector } from '../../utils/hooks';
import api from '../../utils/axiosConfig';
import PasswordChangeStep from './PasswordChangeStep';
import TwoFactorAuthStep from './TwoFactorAuthStep';
import SiteSettingsStep from './SiteSettingsStep';
import EmailSettingsStep from './EmailSettingsStep';
import ModerationSettingsStep from './ModerationSettingsStep';
import SetupComplete from './SetupComplete';

interface SetupStep {
  id: string;
  title: string;
  description: string;
  component: React.ComponentType<any>;
}

const setupSteps: SetupStep[] = [
  {
    id: 'password',
    title: 'Change Admin Password',
    description: 'For security, please change your default admin password',
    component: PasswordChangeStep
  },
  {
    id: 'twoFactor',
    title: 'Two-Factor Authentication',
    description: 'Set up two-factor authentication for enhanced security',
    component: TwoFactorAuthStep
  },
  {
    id: 'site',
    title: 'Site Configuration',
    description: 'Configure your site settings and preferences',
    component: SiteSettingsStep
  },
  {
    id: 'email',
    title: 'Email Configuration',
    description: 'Set up email notifications and verification',
    component: EmailSettingsStep
  },
  {
    id: 'moderation',
    title: 'Moderation Settings',
    description: 'Configure content moderation and approval settings',
    component: ModerationSettingsStep
  }
];

const FirstTimeSetup: React.FC = () => {
  const [currentStep, setCurrentStep] = useState(0);
  const [setupData, setSetupData] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [isComplete, setIsComplete] = useState(false);
  const [setupStatus, setSetupStatus] = useState<any>(null);
  const [isLoadingStatus, setIsLoadingStatus] = useState(true);
  const navigate = useNavigate();
  const { user } = useAppSelector((state) => state.auth);

  useEffect(() => {
    if (!user) {
      navigate('/login');
      return;
    }

    // Check current setup status
    const checkSetupStatus = async () => {
      try {
        const response = await api.get('/SystemSetup/status');

        if (response.status === 200) {
          const status = response.data;
          setSetupStatus(status);
          
          // Determine starting step based on completed steps
          let startingStep = 0;
          if (status.isAdminConfigured) {
            startingStep = 1; // Start at 2FA setup
          }
          if (status.isTwoFactorConfigured) {
            startingStep = 2; // Start at site configuration
          }
          if (status.isSiteConfigured) {
            startingStep = 3; // Start at email configuration
          }
          if (status.isEmailConfigured) {
            startingStep = 4; // Start at moderation settings
          }
          if (status.isModerationConfigured) {
            // All steps completed, redirect to dashboard
            navigate('/dashboard');
            return;
          }
          
          setCurrentStep(startingStep);
        }
      } catch (error) {
        console.error('Error checking setup status:', error);
      } finally {
        setIsLoadingStatus(false);
      }
    };

    checkSetupStatus();
  }, [user, navigate]);

  const handleStepComplete = (stepData: any) => {
    setSetupData((prev: any) => ({ ...prev, ...stepData }));
    
    if (currentStep < setupSteps.length - 1) {
      setCurrentStep(currentStep + 1);
    } else {
      handleSetupComplete();
    }
  };

  const handleStepBack = () => {
    if (currentStep > 0) {
      setCurrentStep(currentStep - 1);
    }
  };

  const handleSetupComplete = async () => {
    setIsLoading(true);
    try {
      const response = await api.post('/SystemSetup/complete', setupData);

      if (response.status === 200) {
        setIsComplete(true);
      } else {
        console.error('Failed to complete setup');
      }
    } catch (error) {
      console.error('Error completing setup:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleCompleteRedirect = () => {
    navigate('/dashboard');
  };

  if (isLoadingStatus) {
    return (
      <div className="lg-container min-h-screen flex flex-col justify-center py-12 sm:px-6 lg:px-8">
        <div className="sm:mx-auto sm:w-full sm:max-w-md">
          <div className="text-center">
            <div className="lg-spinner mx-auto"></div>
            <h2 className="mt-4 text-lg font-medium text-white">Checking setup status...</h2>
          </div>
        </div>
      </div>
    );
  }

  if (isComplete) {
    return <SetupComplete onComplete={handleCompleteRedirect} />;
  }

  const currentStepData = setupSteps[currentStep];
  const StepComponent = currentStepData.component;

  return (
    <div className="lg-container min-h-screen flex flex-col justify-center py-12 sm:px-6 lg:px-8">
      <div className="sm:mx-auto sm:w-full sm:max-w-4xl">
        <div className="text-center mb-8">
          <h2 className="text-3xl font-extrabold text-white mb-2">
            Welcome to PolyBucket Setup
          </h2>
          <p className="text-gray-300">
            Let's configure your site step by step
          </p>
        </div>

        <div className="lg-card p-8">
          {/* Progress indicator */}
          <div className="mb-8">
            <div className="flex items-center justify-center mb-6">
              <div className="flex items-center space-x-4 max-w-2xl">
                {setupSteps.map((step, index) => (
                  <div key={step.id} className="flex items-center">
                    <div className={`flex-shrink-0 w-10 h-10 rounded-full flex items-center justify-center text-sm font-medium transition-all duration-300 ${
                      index <= currentStep 
                        ? 'bg-blue-600 text-white shadow-lg' 
                        : 'bg-gray-700 text-gray-400 border border-gray-600'
                    }`}>
                      {index + 1}
                    </div>
                    {index < setupSteps.length - 1 && (
                      <div className={`w-16 h-1 mx-2 transition-all duration-300 ${
                        index < currentStep ? 'bg-blue-600' : 'bg-gray-700'
                      }`} />
                    )}
                  </div>
                ))}
              </div>
            </div>
            
            <div className="text-center">
              <h3 className="text-xl font-semibold text-white mb-2">
                {currentStepData.title}
              </h3>
              <p className="text-gray-300">
                {currentStepData.description}
              </p>
            </div>
          </div>

          {/* Step content */}
          <div className="bg-gray-800/50 rounded-lg p-6 border border-gray-700">
            <StepComponent
              onComplete={handleStepComplete}
              onBack={handleStepBack}
              data={setupData}
              isLoading={isLoading}
              isFirstStep={currentStep === 0}
              isLastStep={currentStep === setupSteps.length - 1}
            />
          </div>

          {/* Navigation buttons */}
          <div className="flex justify-between mt-6">
            <button
              onClick={handleStepBack}
              disabled={currentStep === 0}
              className="lg-button disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Previous
            </button>
            
            <div className="text-sm text-gray-400">
              Step {currentStep + 1} of {setupSteps.length}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default FirstTimeSetup; 