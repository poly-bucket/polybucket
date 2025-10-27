import React, { useState } from 'react';
import { useAppSelector } from '../utils/hooks';
import { PrivacySettings } from '../services/api.client';

interface SiteSettingsStepProps {
  onComplete: (data: any) => void;
  onBack: () => void;
  data: any;
  isLoading: boolean;
  isFirstStep: boolean;
  isLastStep: boolean;
}

const SiteSettingsStep: React.FC<SiteSettingsStepProps> = ({
  onComplete,
  onBack,
  data,
  isLoading,
  isFirstStep,
  isLastStep
}) => {
  const { user } = useAppSelector((state) => state.auth);
  const [formData, setFormData] = useState({
    siteName: data.siteName || 'PolyBucket',
    siteDescription: data.siteDescription || '3D Model Repository',
    contactEmail: data.contactEmail || '',
    allowPublicBrowsing: data.allowPublicBrowsing !== false,
    requireLoginForUpload: data.requireLoginForUpload !== false,
    allowUserRegistration: data.allowUserRegistration !== false,
    requireEmailVerification: data.requireEmailVerification || false,
    maxFileSizeBytes: data.maxFileSizeBytes || 100 * 1024 * 1024,
    allowedFileTypes: data.allowedFileTypes || '.stl,.obj,.fbx,.3ds,.glb,.gltf,.ply,.step,.stp,.iges,.igs,.brep,.png,.jpg,.jpeg,.gif',
    maxFilesPerUpload: data.maxFilesPerUpload || 5,
    enableFileCompression: data.enableFileCompression !== false,
    autoGeneratePreviews: data.autoGeneratePreviews !== false,
    defaultModelPrivacy: data.defaultModelPrivacy || PrivacySettings.Public,
    autoApproveModels: data.autoApproveModels || false,
    requireModeration: data.requireModeration !== false,
    defaultUserRole: data.defaultUserRole || 'User',
    allowCustomRoles: data.allowCustomRoles || false,
    maxFailedLoginAttempts: data.maxFailedLoginAttempts || 5,
    lockoutDurationMinutes: data.lockoutDurationMinutes || 15,
    requireStrongPasswords: data.requireStrongPasswords !== false,
    passwordMinLength: data.passwordMinLength || 8,
    defaultTheme: data.defaultTheme || 'dark',
    defaultLanguage: data.defaultLanguage || 'en',
    showAdvancedOptions: data.showAdvancedOptions || false,
    enableFederation: data.enableFederation || false,
    instanceName: data.instanceName || '',
    instanceDescription: data.instanceDescription || '',
    adminContact: data.adminContact || ''
  });
  const [errors, setErrors] = useState<Record<string, string>>({});

  const handleInputChange = (field: string, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    if (errors[field]) {
      setErrors(prev => ({ ...prev, [field]: '' }));
    }
  };

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.siteName.trim()) {
      newErrors.siteName = 'Site name is required';
    }

    if (!formData.contactEmail.trim()) {
      newErrors.contactEmail = 'Contact email is required';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.contactEmail)) {
      newErrors.contactEmail = 'Please enter a valid email address';
    }

    if (formData.maxFileSizeBytes < 1 * 1024 * 1024 || formData.maxFileSizeBytes > 1024 * 1024 * 1024) {
      newErrors.maxFileSizeBytes = 'Max file size must be between 1MB and 1GB';
    }

    if (formData.maxFilesPerUpload < 1 || formData.maxFilesPerUpload > 20) {
      newErrors.maxFilesPerUpload = 'Max files per upload must be between 1 and 20';
    }

    if (formData.maxFailedLoginAttempts < 1 || formData.maxFailedLoginAttempts > 10) {
      newErrors.maxFailedLoginAttempts = 'Max failed login attempts must be between 1 and 10';
    }

    if (formData.lockoutDurationMinutes < 5 || formData.lockoutDurationMinutes > 60) {
      newErrors.lockoutDurationMinutes = 'Lockout duration must be between 5 and 60 minutes';
    }

    if (formData.passwordMinLength < 6 || formData.passwordMinLength > 20) {
      newErrors.passwordMinLength = 'Password minimum length must be between 6 and 20 characters';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    // Transform the data to match backend expectations
    const transformedData = {
      SiteName: formData.siteName,
      SiteDescription: formData.siteDescription,
      ContactEmail: formData.contactEmail,
      AllowPublicBrowsing: formData.allowPublicBrowsing,
      RequireLoginForUpload: formData.requireLoginForUpload,
      AllowUserRegistration: formData.allowUserRegistration,
      RequireEmailVerification: formData.requireEmailVerification,
      MaxFileSizeBytes: formData.maxFileSizeBytes,
      AllowedFileTypes: formData.allowedFileTypes,
      MaxFilesPerUpload: formData.maxFilesPerUpload,
      EnableFileCompression: formData.enableFileCompression,
      AutoGeneratePreviews: formData.autoGeneratePreviews,
      DefaultModelPrivacy: formData.defaultModelPrivacy,
      AutoApproveModels: formData.autoApproveModels,
      RequireModeration: formData.requireModeration,
      DefaultUserRole: formData.defaultUserRole,
      AllowCustomRoles: formData.allowCustomRoles,
      MaxFailedLoginAttempts: formData.maxFailedLoginAttempts,
      LockoutDurationMinutes: formData.lockoutDurationMinutes,
      RequireStrongPasswords: formData.requireStrongPasswords,
      PasswordMinLength: formData.passwordMinLength,
      DefaultTheme: formData.defaultTheme,
      DefaultLanguage: formData.defaultLanguage,
      ShowAdvancedOptions: formData.showAdvancedOptions,
      EnableFederation: formData.enableFederation,
      InstanceName: formData.instanceName,
      InstanceDescription: formData.instanceDescription,
      AdminContact: formData.adminContact || null
    };

    try {
      const response = await fetch('http://localhost:11666/api/SystemSetup/site-settings', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${user?.accessToken}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(transformedData)
      });

      if (response.ok) {
        onComplete(formData);
      } else {
        const errorData = await response.json();
        setErrors({ submit: errorData.message || 'Failed to save site settings' });
      }
    } catch (error) {
      setErrors({ submit: 'An error occurred while saving site settings' });
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
        {/* Basic Site Settings */}
        <div className="sm:col-span-2">
          <h3 className="text-lg font-medium text-white mb-4">Basic Site Settings</h3>
        </div>

        <div>
          <label htmlFor="siteName" className="block text-sm font-medium text-white mb-2">
            Site Name *
          </label>
          <input
            type="text"
            id="siteName"
            value={formData.siteName}
            onChange={(e) => handleInputChange('siteName', e.target.value)}
            className={`lg-input ${
              errors.siteName ? 'border-red-400' : ''
            }`}
            placeholder="Enter site name"
          />
          {errors.siteName && <p className="mt-1 text-sm text-red-400">{errors.siteName}</p>}
        </div>

        <div>
          <label htmlFor="contactEmail" className="block text-sm font-medium text-white mb-2">
            Contact Email *
          </label>
          <input
            type="email"
            id="contactEmail"
            value={formData.contactEmail}
            onChange={(e) => handleInputChange('contactEmail', e.target.value)}
            className={`lg-input ${
              errors.contactEmail ? 'border-red-400' : ''
            }`}
            placeholder="admin@example.com"
          />
          {errors.contactEmail && <p className="mt-1 text-sm text-red-400">{errors.contactEmail}</p>}
        </div>

        <div className="sm:col-span-2">
          <label htmlFor="siteDescription" className="block text-sm font-medium text-white mb-2">
            Site Description
          </label>
          <textarea
            id="siteDescription"
            rows={3}
            value={formData.siteDescription}
            onChange={(e) => handleInputChange('siteDescription', e.target.value)}
            className="lg-input resize-none"
            placeholder="Describe your site"
          />
        </div>

        {/* Access Control */}
        <div className="sm:col-span-2">
          <h3 className="text-lg font-medium text-white mb-4">Access Control</h3>
        </div>

        <div className="flex items-center">
          <input
            type="checkbox"
            id="allowPublicBrowsing"
            checked={formData.allowPublicBrowsing}
            onChange={(e) => handleInputChange('allowPublicBrowsing', e.target.checked)}
            className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-600 rounded bg-gray-700"
          />
          <label htmlFor="allowPublicBrowsing" className="ml-2 block text-sm text-white">
            Allow public browsing
          </label>
        </div>

        <div className="flex items-center">
          <input
            type="checkbox"
            id="requireLoginForUpload"
            checked={formData.requireLoginForUpload}
            onChange={(e) => handleInputChange('requireLoginForUpload', e.target.checked)}
            className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-600 rounded bg-gray-700"
          />
          <label htmlFor="requireLoginForUpload" className="ml-2 block text-sm text-white">
            Require login for uploads
          </label>
        </div>

        <div className="flex items-center">
          <input
            type="checkbox"
            id="allowUserRegistration"
            checked={formData.allowUserRegistration}
            onChange={(e) => handleInputChange('allowUserRegistration', e.target.checked)}
            className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-600 rounded bg-gray-700"
          />
          <label htmlFor="allowUserRegistration" className="ml-2 block text-sm text-white">
            Allow user registration
          </label>
        </div>

        <div className="flex items-center">
          <input
            type="checkbox"
            id="requireEmailVerification"
            checked={formData.requireEmailVerification}
            onChange={(e) => handleInputChange('requireEmailVerification', e.target.checked)}
            className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-600 rounded bg-gray-700"
          />
          <label htmlFor="requireEmailVerification" className="ml-2 block text-sm text-white">
            Require email verification
          </label>
        </div>

        {/* File Upload Settings */}
        <div className="sm:col-span-2">
          <h3 className="text-lg font-medium text-white mb-4">File Upload Settings</h3>
        </div>

        <div>
          <label htmlFor="maxFileSizeBytes" className="block text-sm font-medium text-white mb-2">
            Max File Size (MB)
          </label>
          <input
            type="number"
            id="maxFileSizeBytes"
            value={Math.round(formData.maxFileSizeBytes / (1024 * 1024))}
            onChange={(e) => handleInputChange('maxFileSizeBytes', parseInt(e.target.value) * 1024 * 1024)}
            min="1"
            max="1024"
            className={`lg-input ${
              errors.maxFileSizeBytes ? 'border-red-400' : ''
            }`}
          />
          {errors.maxFileSizeBytes && <p className="mt-1 text-sm text-red-400">{errors.maxFileSizeBytes}</p>}
        </div>

        <div>
          <label htmlFor="maxFilesPerUpload" className="block text-sm font-medium text-white mb-2">
            Max Files per Upload
          </label>
          <input
            type="number"
            id="maxFilesPerUpload"
            value={formData.maxFilesPerUpload}
            onChange={(e) => handleInputChange('maxFilesPerUpload', parseInt(e.target.value))}
            min="1"
            max="20"
            className={`lg-input ${
              errors.maxFilesPerUpload ? 'border-red-400' : ''
            }`}
          />
          {errors.maxFilesPerUpload && <p className="mt-1 text-sm text-red-400">{errors.maxFilesPerUpload}</p>}
        </div>

        <div className="sm:col-span-2">
          <label htmlFor="allowedFileTypes" className="block text-sm font-medium text-white mb-2">
            Allowed File Types
          </label>
          <input
            type="text"
            id="allowedFileTypes"
            value={formData.allowedFileTypes}
            onChange={(e) => handleInputChange('allowedFileTypes', e.target.value)}
            placeholder=".stl,.obj,.fbx,.3ds,.glb,.gltf,.ply,.step,.stp,.iges,.igs,.brep,.png,.jpg,.jpeg,.gif"
            className="lg-input"
          />
        </div>

        <div className="flex items-center">
          <input
            type="checkbox"
            id="enableFileCompression"
            checked={formData.enableFileCompression}
            onChange={(e) => handleInputChange('enableFileCompression', e.target.checked)}
            className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-600 rounded bg-gray-700"
          />
          <label htmlFor="enableFileCompression" className="ml-2 block text-sm text-white">
            Enable file compression
          </label>
        </div>

        <div className="flex items-center">
          <input
            type="checkbox"
            id="autoGeneratePreviews"
            checked={formData.autoGeneratePreviews}
            onChange={(e) => handleInputChange('autoGeneratePreviews', e.target.checked)}
            className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-600 rounded bg-gray-700"
          />
          <label htmlFor="autoGeneratePreviews" className="ml-2 block text-sm text-white">
            Auto-generate previews
          </label>
        </div>
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
          className="lg-button"
        >
          Back
        </button>
        <button
          type="submit"
          disabled={isLoading}
          className="lg-button lg-button-primary disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {isLoading ? (
            <div className="flex items-center">
              <div className="lg-spinner w-4 h-4 mr-2"></div>
              Saving...
            </div>
          ) : (
            'Save & Continue'
          )}
        </button>
      </div>
    </form>
  );
};

export default SiteSettingsStep; 