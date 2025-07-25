import React, { useState } from 'react';
import { useAppSelector } from '../../utils/hooks';
import { twoFactorAuthService } from '../../services/twoFactorAuthService';
import QRCode from 'react-qr-code';

interface TwoFactorAuthStepProps {
  onComplete: (data: any) => void;
  onBack: () => void;
  data: any;
  isLoading: boolean;
  isFirstStep: boolean;
  isLastStep: boolean;
}

const TwoFactorAuthStep: React.FC<TwoFactorAuthStepProps> = ({
  onComplete,
  onBack,
  isLoading,
  isFirstStep,
  isLastStep
}) => {
  const { user } = useAppSelector((state) => state.auth);
  const [enable2FA, setEnable2FA] = useState(false);
  const [qrCode, setQrCode] = useState<string>('');
  const [verificationCode, setVerificationCode] = useState('');
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [isGenerating, setIsGenerating] = useState(false);
  const [isVerifying, setIsVerifying] = useState(false);
  const [showQRSetup, setShowQRSetup] = useState(false);

  const handleEnable2FA = async () => {
    if (!enable2FA) {
      onComplete({ twoFactorEnabled: false });
      return;
    }

    setIsGenerating(true);
    try {
      const response = await twoFactorAuthService.initialize();
      setQrCode(response.qrCodeUrl);
      setShowQRSetup(true);
    } catch (error: any) {
      setErrors({ setup: error.response?.data?.message || 'Failed to setup 2FA' });
    } finally {
      setIsGenerating(false);
    }
  };

  const handleVerifyCode = async () => {
    if (!verificationCode.trim()) {
      setErrors({ verification: 'Please enter the verification code' });
      return;
    }

    setIsVerifying(true);
    try {
      const response = await twoFactorAuthService.enable(verificationCode);
      
      if (response.success) {
        onComplete({ 
          twoFactorEnabled: true,
          backupCodes: response.backupCodes || []
        });
      } else {
        setErrors({ verification: response.message || 'Invalid verification code' });
      }
    } catch (error: any) {
      setErrors({ verification: error.response?.data?.message || 'An error occurred while verifying the code' });
    } finally {
      setIsVerifying(false);
    }
  };

  const handleSkip = () => {
    onComplete({ twoFactorEnabled: false });
  };

  const handleBackToOptions = () => {
    setShowQRSetup(false);
    setQrCode('');
    setVerificationCode('');
    setErrors({});
  };

  return (
    <div className="space-y-6">
      <div>
        <h3 className="text-lg font-medium text-white mb-4">
          Two-Factor Authentication Setup
        </h3>
        <p className="text-gray-300 mb-6">
          Enhance your account security by enabling two-factor authentication. This adds an extra layer of protection to your account.
        </p>
      </div>

      {!showQRSetup ? (
        <div className="space-y-4">
          <div className="flex items-center space-x-3">
            <input
              type="checkbox"
              id="enable2FA"
              checked={enable2FA}
              onChange={(e) => setEnable2FA(e.target.checked)}
              className="w-4 h-4 text-blue-600 bg-gray-700 border-gray-600 rounded focus:ring-blue-500 focus:ring-2"
            />
            <label htmlFor="enable2FA" className="text-white">
              Enable Two-Factor Authentication
            </label>
          </div>

          {errors.setup && (
            <div className="lg-badge lg-badge-error w-full justify-center">
              {errors.setup}
            </div>
          )}

          <div className="flex justify-between pt-4">
            <button
              type="button"
              onClick={onBack}
              disabled={isFirstStep}
              className="lg-button disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Back
            </button>
            <div className="flex space-x-3">
              <button
                type="button"
                onClick={handleSkip}
                disabled={isLoading || isGenerating}
                className="lg-button disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Skip This Step
              </button>
              <button
                type="button"
                onClick={handleEnable2FA}
                disabled={isLoading || isGenerating || !enable2FA}
                className="lg-button lg-button-primary disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isGenerating ? (
                  <div className="flex items-center">
                    <div className="lg-spinner w-4 h-4 mr-2"></div>
                    Setting up 2FA...
                  </div>
                ) : (
                  'Continue'
                )}
              </button>
            </div>
          </div>
        </div>
      ) : (
        <div className="space-y-6">
                      <div className="text-center">
              <h4 className="text-md font-medium text-white mb-4">
                Scan QR Code with Your Authenticator App
              </h4>
              <div className="bg-white p-4 rounded-lg inline-block mb-4">
                <QRCode
                  value={qrCode}
                  size={192}
                  level="M"
                  bgColor="#FFFFFF"
                  fgColor="#000000"
                  style={{ width: '100%', height: 'auto' }}
                />
              </div>
              <p className="text-gray-300 text-sm mb-4">
                Use apps like Google Authenticator, Authy, or Microsoft Authenticator to scan this QR code.
              </p>
              <div className="bg-gray-800 p-4 rounded-lg mb-4">
                <p className="text-gray-300 text-sm mb-2">
                  <strong>Manual Entry:</strong> If scanning doesn't work, you can manually enter the setup key in your authenticator app.
                </p>
                <p className="text-gray-400 text-xs break-all">
                  {qrCode}
                </p>
              </div>
            </div>

          <div>
            <label htmlFor="verificationCode" className="block text-sm font-medium text-white mb-2">
              Verification Code
            </label>
            <input
              id="verificationCode"
              type="text"
              value={verificationCode}
              onChange={(e) => {
                setVerificationCode(e.target.value);
                if (errors.verification) {
                  setErrors(prev => {
                    const newErrors = { ...prev };
                    delete newErrors.verification;
                    return newErrors;
                  });
                }
              }}
              className="lg-input"
              placeholder="Enter the 6-digit code from your app"
              maxLength={6}
            />
            {errors.verification && (
              <p className="mt-2 text-sm text-red-400">{errors.verification}</p>
            )}
          </div>

          <div className="flex justify-between pt-4">
            <button
              type="button"
              onClick={handleBackToOptions}
              className="lg-button"
            >
              Back
            </button>
            <button
              type="button"
              onClick={handleVerifyCode}
              disabled={isLoading || isVerifying || !verificationCode.trim()}
              className="lg-button lg-button-primary disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isVerifying ? (
                <div className="flex items-center">
                  <div className="lg-spinner w-4 h-4 mr-2"></div>
                  Verifying...
                </div>
              ) : (
                'Verify & Continue'
              )}
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default TwoFactorAuthStep; 