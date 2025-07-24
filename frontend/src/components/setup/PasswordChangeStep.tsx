import React, { useState } from 'react';
import { EyeIcon, EyeSlashIcon } from '@heroicons/react/24/outline';
import { useAppSelector } from '../../utils/hooks';

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
      setErrors(prev => ({ ...prev, [field]: '' }));
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
    }

    if (!formData.confirmPassword) {
      newErrors.confirmPassword = 'Please confirm your new password';
    } else if (formData.newPassword !== formData.confirmPassword) {
      newErrors.confirmPassword = 'Passwords do not match';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    try {
      const response = await fetch('http://localhost:11666/api/ChangePassword/change', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${user?.accessToken}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          currentPassword: formData.currentPassword,
          newPassword: formData.newPassword,
          confirmPassword: formData.confirmPassword
        })
      });

      if (response.ok) {
        onComplete({ passwordChanged: true });
      } else {
        const errorData = await response.json();
        setErrors({ submit: errorData.message || 'Failed to change password' });
      }
    } catch (error) {
      setErrors({ submit: 'An error occurred while changing password' });
    }
  };

  const handleSkip = async () => {
    setIsSkipping(true);
    try {
      const response = await fetch('http://localhost:11666/api/ChangePassword/skip', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${user?.accessToken}`,
          'Content-Type': 'application/json'
        }
      });

      if (response.ok) {
        onComplete({ passwordSkipped: true });
      } else {
        const errorData = await response.json();
        setErrors({ submit: errorData.message || 'Failed to skip password change' });
      }
    } catch (error) {
      setErrors({ submit: 'An error occurred while skipping password change' });
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
            disabled={isLoading || isSkipping}
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