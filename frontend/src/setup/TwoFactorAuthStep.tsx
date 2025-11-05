import React, { useState, useRef, useEffect } from 'react';
import { useAppSelector } from '../utils/hooks';
import { twoFactorAuthService } from '../services/twoFactorAuthService';
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
  const [verificationCode, setVerificationCode] = useState(['', '', '', '', '', '']);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [isGenerating, setIsGenerating] = useState(false);
  const [isVerifying, setIsVerifying] = useState(false);
  const [showQRSetup, setShowQRSetup] = useState(false);
  const [hasAutoSubmitted, setHasAutoSubmitted] = useState(false);
  const inputRefs = useRef<(HTMLInputElement | null)[]>([]);

  // Auto-submit when all 6 digits are filled (only once)
  useEffect(() => {
    const code = verificationCode.join('');
    if (code.length === 6 && !isVerifying && !hasAutoSubmitted) {
      // Set the flag immediately to prevent multiple triggers
      setHasAutoSubmitted(true);
      
      // Increased delay to ensure UI has fully updated and prevent race conditions
      const timer = setTimeout(() => {
        // Double-check that we're still in a valid state before submitting
        if (!isVerifying) {
          handleVerifyCode();
        }
      }, 500);
      return () => clearTimeout(timer);
    }
  }, [verificationCode, isVerifying, hasAutoSubmitted]);

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
      console.error('2FA initialization error:', error);
      const errorMessage = error.response?.data?.message || error.message || 'Failed to setup 2FA';
      // Handle concurrency errors with user-friendly message
      if (errorMessage.includes('modified by another operation') || errorMessage.includes('Concurrent modification')) {
        setErrors({ setup: 'The 2FA setup was modified by another operation. Please try again.' });
      } else {
        setErrors({ setup: errorMessage });
      }
    } finally {
      setIsGenerating(false);
    }
  };

  const handleVerifyCode = async () => {
    // Prevent multiple simultaneous requests
    if (isVerifying) {
      return;
    }

    const code = verificationCode.join('');
    if (code.length !== 6) {
      setErrors({ verification: 'Please enter the complete 6-digit verification code' });
      return;
    }

    setIsVerifying(true);
    try {
      const response = await twoFactorAuthService.enable(code);
      if (response.success) {
        onComplete({ 
          twoFactorEnabled: true,
          backupCodes: response.backupCodes || []
        });
      } else {
        setErrors({ verification: response.message || 'Invalid verification code' });
        // Reset auto-submit flag on error so user can manually retry
        setHasAutoSubmitted(false);
      }
    } catch (error: any) {
      const errorMessage = error.response?.data?.message || 'An error occurred while verifying the code';
      // Handle concurrency errors with user-friendly message
      if (errorMessage.includes('modified by another operation') || errorMessage.includes('Concurrent modification')) {
        setErrors({ verification: 'The 2FA setup was modified by another operation. Please try again.' });
      } else {
        setErrors({ verification: errorMessage });
      }
      // Reset auto-submit flag on error so user can manually retry
      setHasAutoSubmitted(false);
    } finally {
      setIsVerifying(false);
    }
  };

  const handleCodeChange = (index: number, value: string) => {
    // Only allow numeric input
    if (!/^\d*$/.test(value)) {
      return;
    }

    const newCode = [...verificationCode];
    newCode[index] = value;
    setVerificationCode(newCode);

    // Clear verification error when user starts typing
    if (errors.verification) {
      setErrors(prev => {
        const newErrors = { ...prev };
        delete newErrors.verification;
        return newErrors;
      });
    }

    // Auto-advance to next input
    if (value && index < 5) {
      inputRefs.current[index + 1]?.focus();
    }
  };

  const handleKeyDown = (index: number, e: React.KeyboardEvent<HTMLInputElement>) => {
    // Handle backspace to go to previous input
    if (e.key === 'Backspace' && !verificationCode[index] && index > 0) {
      inputRefs.current[index - 1]?.focus();
    }
  };

  const handleSkip = () => {
    onComplete({ twoFactorEnabled: false });
  };

  const handleBackToOptions = () => {
    setShowQRSetup(false);
    setQrCode('');
    setVerificationCode(['', '', '', '', '', '']);
    setErrors({});
    setHasAutoSubmitted(false);
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

          <div className="text-center">
            <h4 className="text-md font-medium text-white mb-4">
              Two-Factor Verification
            </h4>
            <p className="text-gray-300 text-sm mb-6">
              Enter the two-factor authentication code provided by the authenticator app
            </p>
            
            <div className="flex justify-center space-x-2 mb-4">
              {verificationCode.map((digit, index) => (
                <input
                  key={index}
                  ref={(el) => {
                    inputRefs.current[index] = el;
                  }}
                  type="text"
                  value={digit}
                  onChange={(e) => handleCodeChange(index, e.target.value)}
                  onKeyDown={(e) => handleKeyDown(index, e)}
                  className="w-12 h-12 text-center text-lg font-semibold bg-gray-700 border border-gray-600 rounded-lg focus:border-blue-500 focus:ring-2 focus:ring-blue-500 focus:outline-none text-white"
                  maxLength={1}
                  inputMode="numeric"
                  pattern="[0-9]*"
                />
              ))}
            </div>

            {errors.verification && (
              <p className="mt-2 text-sm text-red-400">{errors.verification}</p>
            )}

            {isVerifying && (
              <div className="flex items-center justify-center mt-4">
                <div className="lg-spinner w-4 h-4 mr-2"></div>
                <span className="text-gray-300">Verifying...</span>
              </div>
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
              disabled={isLoading || isVerifying || verificationCode.join('').length !== 6}
              className="lg-button lg-button-primary disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Verify & Continue
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default TwoFactorAuthStep; 