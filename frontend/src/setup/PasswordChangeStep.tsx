import React, { useState } from 'react';
import { EyeIcon, EyeSlashIcon } from '@heroicons/react/24/outline';
import { useAppSelector } from '../utils/hooks';
import { ApiClientFactory } from '../api/clientFactory';
import { ChangePasswordCommand } from '../api/client';
import PasswordStrengthIndicator from './PasswordStrengthIndicator';

interface PasswordChangeStepProps {
  onComplete: (data: any) => void;
  onBack: () => void;
  data: any;
  isLoading: boolean;
  isFirstStep: boolean;
  isLastStep: boolean;
}

const PasswordChangeStep: React.FC<PasswordChangeStepProps> = ({
  onComplete,
  onBack,
  isLoading,
  isFirstStep,
  isLastStep
}) => {
  const { user } = useAppSelector((state) => state.auth);
  const [formData, setFormData] = useState({
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  });
  const [showPasswords, setShowPasswords] = useState({
    current: false,
    new: false,
    confirm: false
  });
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [isSkipping, setIsSkipping] = useState(false);

  const handleInputChange = (field: string, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    // Clear error when user starts typing
    if (errors[field]) {
      setErrors(prev => {
        const newErrors = { ...prev };
        delete newErrors[field];
        return newErrors;
      });
    }
  };

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.currentPassword) {
      newErrors.currentPassword = 'Current password is required';
    }

    if (!formData.newPassword) {
      newErrors.newPassword = 'New password is required';
    } else if (formData.newPassword.length < 8) {
      newErrors.newPassword = 'Password must be at least 8 characters long';
    } else if (!/[A-Z]/.test(formData.newPassword)) {
      newErrors.newPassword = 'Password must contain at least one uppercase letter';
    } else if (!/[a-z]/.test(formData.newPassword)) {
      newErrors.newPassword = 'Password must contain at least one lowercase letter';
    } else if (!/\d/.test(formData.newPassword)) {
      newErrors.newPassword = 'Password must contain at least one number';
    } else if (!/[!@#$%^&*(),.?":{}|<>]/.test(formData.newPassword)) {
      newErrors.newPassword = 'Password must contain at least one special character';
    }

    if (!formData.confirmPassword) {
      newErrors.confirmPassword = 'Please confirm your new password';
    } else if (formData.newPassword !== formData.confirmPassword) {
      newErrors.confirmPassword = 'Passwords do not match';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const isFormValid = (): boolean => {
    return formData.currentPassword.trim() !== '' &&
           formData.newPassword.trim() !== '' &&
           formData.confirmPassword.trim() !== '' &&
           formData.newPassword === formData.confirmPassword &&
           formData.newPassword.length >= 8 &&
           /[A-Z]/.test(formData.newPassword) &&
           /[a-z]/.test(formData.newPassword) &&
           /\d/.test(formData.newPassword) &&
           /[!@#$%^&*(),.?":{}|<>]/.test(formData.newPassword);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    try {
      const client = ApiClientFactory.getApiClient();
      const command = new ChangePasswordCommand({
        currentPassword: formData.currentPassword,
        newPassword: formData.newPassword,
        confirmPassword: formData.confirmPassword
      });

      const response = await client.changePassword_ChangePassword(command);

      if (response && response.success) {
        onComplete({ 
          passwordChanged: true
        });
      } else {
        setErrors({ submit: response?.message || 'Failed to change password' });
      }
    } catch (error: any) {
      setErrors({ submit: error?.message || 'An error occurred while changing password' });
    }
  };

  const handleSkip = async () => {
    setIsSkipping(true);
    try {
      const client = ApiClientFactory.getApiClient();
      const response = await client.changePassword_SkipPasswordChange();

      if (response && response.success) {
        onComplete({ 
          passwordSkipped: true
        });
      } else {
        setErrors({ submit: response?.message || 'Failed to skip password change' });
      }
    } catch (error: any) {
      setErrors({ submit: error?.message || 'An error occurred while skipping password change' });
    } finally {
      setIsSkipping(false);
    }
  };

  const togglePasswordVisibility = (field: 'current' | 'new' | 'confirm') => {
    setShowPasswords(prev => ({ ...prev, [field]: !prev[field] }));
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div>
        <label htmlFor="currentPassword" className="block text-sm font-medium text-white mb-2">
          Current Password
        </label>
        <div className="relative">
          <input
            id="currentPassword"
            name="currentPassword"
            type={showPasswords.current ? 'text' : 'password'}
            required
            value={formData.currentPassword}
            onChange={(e) => handleInputChange('currentPassword', e.target.value)}
            className={`lg-input pr-10 ${
              errors.currentPassword ? 'border-red-400' : ''
            }`}
            placeholder="Enter your current password"
          />
          <button
            type="button"
            className="absolute inset-y-0 right-0 pr-3 flex items-center"
            onClick={() => togglePasswordVisibility('current')}
          >
            {showPasswords.current ? (
              <EyeSlashIcon className="h-5 w-5 text-gray-400 hover:text-white transition-colors" />
            ) : (
              <EyeIcon className="h-5 w-5 text-gray-400 hover:text-white transition-colors" />
            )}
          </button>
        </div>
        {errors.currentPassword && (
          <p className="mt-2 text-sm text-red-400">{errors.currentPassword}</p>
        )}
      </div>

      <div>
        <label htmlFor="newPassword" className="block text-sm font-medium text-white mb-2">
          New Password
        </label>
        <div className="relative">
          <input
            id="newPassword"
            name="newPassword"
            type={showPasswords.new ? 'text' : 'password'}
            required
            value={formData.newPassword}
            onChange={(e) => handleInputChange('newPassword', e.target.value)}
            className={`lg-input pr-10 ${
              errors.newPassword ? 'border-red-400' : ''
            }`}
            placeholder="Enter your new password"
          />
          <button
            type="button"
            className="absolute inset-y-0 right-0 pr-3 flex items-center"
            onClick={() => togglePasswordVisibility('new')}
          >
            {showPasswords.new ? (
              <EyeSlashIcon className="h-5 w-5 text-gray-400 hover:text-white transition-colors" />
            ) : (
              <EyeIcon className="h-5 w-5 text-gray-400 hover:text-white transition-colors" />
            )}
          </button>
        </div>
        {formData.newPassword && <PasswordStrengthIndicator password={formData.newPassword} />}
        {errors.newPassword && (
          <p className="mt-2 text-sm text-red-400">{errors.newPassword}</p>
        )}
      </div>

      <div>
        <label htmlFor="confirmPassword" className="block text-sm font-medium text-white mb-2">
          Confirm New Password
        </label>
        <div className="relative">
          <input
            id="confirmPassword"
            name="confirmPassword"
            type={showPasswords.confirm ? 'text' : 'password'}
            required
            value={formData.confirmPassword}
            onChange={(e) => handleInputChange('confirmPassword', e.target.value)}
            className={`lg-input pr-10 ${
              errors.confirmPassword ? 'border-red-400' : ''
            }`}
            placeholder="Confirm your new password"
          />
          <button
            type="button"
            className="absolute inset-y-0 right-0 pr-3 flex items-center"
            onClick={() => togglePasswordVisibility('confirm')}
          >
            {showPasswords.confirm ? (
              <EyeSlashIcon className="h-5 w-5 text-gray-400 hover:text-white transition-colors" />
            ) : (
              <EyeIcon className="h-5 w-5 text-gray-400 hover:text-white transition-colors" />
            )}
          </button>
        </div>
        {errors.confirmPassword && (
          <p className="mt-2 text-sm text-red-400">{errors.confirmPassword}</p>
        )}
      </div>

      {errors.submit && (
        <div className="lg-badge lg-badge-error w-full justify-center">
          {errors.submit}
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
            disabled={isLoading || isSkipping}
            className="lg-button disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isSkipping ? 'Skipping...' : 'Skip This Step'}
          </button>
          <button
            type="submit"
            disabled={isLoading || isSkipping || !isFormValid()}
            className="lg-button lg-button-primary disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isLoading ? (
              <div className="flex items-center">
                <div className="lg-spinner w-4 h-4 mr-2"></div>
                Changing Password...
              </div>
            ) : (
              'Change Password & Continue'
            )}
          </button>
        </div>
      </div>
    </form>
  );
};

export default PasswordChangeStep; 