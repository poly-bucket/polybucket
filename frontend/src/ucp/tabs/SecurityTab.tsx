import React, { useState } from 'react';
import { Visibility, VisibilityOff, Security, QrCode2 } from '@mui/icons-material';

const SecurityTab: React.FC = () => {
  const [passwordForm, setPasswordForm] = useState({
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
    showCurrentPassword: false,
    showNewPassword: false,
    showConfirmPassword: false
  });
  
  const [twoFactorEnabled, setTwoFactorEnabled] = useState(false);
  const [twoFactorSecret, setTwoFactorSecret] = useState('');
  const [showTwoFactorSetup, setShowTwoFactorSetup] = useState(false);
  const [verificationCode, setVerificationCode] = useState('');
  
  const [isChangingPassword, setIsChangingPassword] = useState(false);
  const [isSettingUp2FA, setIsSettingUp2FA] = useState(false);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' as 'success' | 'error' });

  const handlePasswordChange = (field: string, value: any) => {
    setPasswordForm(prev => ({ ...prev, [field]: value }));
  };

  const togglePasswordVisibility = (field: string) => {
    setPasswordForm(prev => ({ ...prev, [field]: !prev[field as keyof typeof prev] }));
  };

  const changePassword = async () => {
    if (passwordForm.newPassword !== passwordForm.confirmPassword) {
      setSnackbar({ open: true, message: 'New passwords do not match', severity: 'error' });
      return;
    }

    if (passwordForm.newPassword.length < 8) {
      setSnackbar({ open: true, message: 'Password must be at least 8 characters long', severity: 'error' });
      return;
    }

    setIsChangingPassword(true);
    try {
      // Mock API call - replace with actual endpoint
      await new Promise(resolve => setTimeout(resolve, 1000));
      setSnackbar({ open: true, message: 'Password changed successfully', severity: 'success' });
      setPasswordForm({
        currentPassword: '',
        newPassword: '',
        confirmPassword: '',
        showCurrentPassword: false,
        showNewPassword: false,
        showConfirmPassword: false
      });
    } catch (error) {
      setSnackbar({ open: true, message: 'Failed to change password', severity: 'error' });
    } finally {
      setIsChangingPassword(false);
    }
  };

  const setupTwoFactor = async () => {
    setIsSettingUp2FA(true);
    try {
      // Mock API call to generate 2FA secret
      await new Promise(resolve => setTimeout(resolve, 1000));
      setTwoFactorSecret('JBSWY3DPEHPK3PXP'); // Mock secret
      setShowTwoFactorSetup(true);
    } catch (error) {
      setSnackbar({ open: true, message: 'Failed to setup two-factor authentication', severity: 'error' });
    } finally {
      setIsSettingUp2FA(false);
    }
  };

  const verifyTwoFactor = async () => {
    if (verificationCode.length !== 6) {
      setSnackbar({ open: true, message: 'Please enter a 6-digit verification code', severity: 'error' });
      return;
    }

    try {
      // Mock API call to verify 2FA
      await new Promise(resolve => setTimeout(resolve, 1000));
      setTwoFactorEnabled(true);
      setShowTwoFactorSetup(false);
      setSnackbar({ open: true, message: 'Two-factor authentication enabled successfully', severity: 'success' });
    } catch (error) {
      setSnackbar({ open: true, message: 'Invalid verification code', severity: 'error' });
    }
  };

  const disableTwoFactor = async () => {
    try {
      // Mock API call to disable 2FA
      await new Promise(resolve => setTimeout(resolve, 1000));
      setTwoFactorEnabled(false);
      setSnackbar({ open: true, message: 'Two-factor authentication disabled', severity: 'success' });
    } catch (error) {
      setSnackbar({ open: true, message: 'Failed to disable two-factor authentication', severity: 'error' });
    }
  };

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold text-white">Security Settings</h2>
      
      {/* Password Change */}
      <div className="lg-card p-6">
        <h3 className="text-lg font-medium text-white mb-4">Change Password</h3>
        
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-white/80 mb-2">Current Password</label>
            <div className="relative">
              <input
                type={passwordForm.showCurrentPassword ? 'text' : 'password'}
                value={passwordForm.currentPassword}
                onChange={(e) => handlePasswordChange('currentPassword', e.target.value)}
                className="lg-input pr-12"
                placeholder="Enter current password"
              />
              <button
                type="button"
                onClick={() => togglePasswordVisibility('showCurrentPassword')}
                className="absolute right-3 top-1/2 transform -translate-y-1/2 text-white/60 hover:text-white"
              >
                {passwordForm.showCurrentPassword ? <VisibilityOff className="w-5 h-5" /> : <Visibility className="w-5 h-5" />}
              </button>
            </div>
          </div>
          
          <div>
            <label className="block text-sm font-medium text-white/80 mb-2">New Password</label>
            <div className="relative">
              <input
                type={passwordForm.showNewPassword ? 'text' : 'password'}
                value={passwordForm.newPassword}
                onChange={(e) => handlePasswordChange('newPassword', e.target.value)}
                className="lg-input pr-12"
                placeholder="Enter new password"
              />
              <button
                type="button"
                onClick={() => togglePasswordVisibility('showNewPassword')}
                className="absolute right-3 top-1/2 transform -translate-y-1/2 text-white/60 hover:text-white"
              >
                {passwordForm.showNewPassword ? <VisibilityOff className="w-5 h-5" /> : <Visibility className="w-5 h-5" />}
              </button>
            </div>
            <p className="text-sm text-white/60 mt-1">Password must be at least 8 characters long</p>
          </div>
          
          <div>
            <label className="block text-sm font-medium text-white/80 mb-2">Confirm New Password</label>
            <div className="relative">
              <input
                type={passwordForm.showConfirmPassword ? 'text' : 'password'}
                value={passwordForm.confirmPassword}
                onChange={(e) => handlePasswordChange('confirmPassword', e.target.value)}
                className="lg-input pr-12"
                placeholder="Confirm new password"
              />
              <button
                type="button"
                onClick={() => togglePasswordVisibility('showConfirmPassword')}
                className="absolute right-3 top-1/2 transform -translate-y-1/2 text-white/60 hover:text-white"
              >
                {passwordForm.showConfirmPassword ? <VisibilityOff className="w-5 h-5" /> : <Visibility className="w-5 h-5" />}
              </button>
            </div>
          </div>
          
          <div className="flex justify-end">
            <button
              onClick={changePassword}
              disabled={isChangingPassword || !passwordForm.currentPassword || !passwordForm.newPassword || !passwordForm.confirmPassword}
              className="lg-button lg-button-primary"
            >
              {isChangingPassword ? 'Changing...' : 'Change Password'}
            </button>
          </div>
        </div>
      </div>

      {/* Two-Factor Authentication */}
      <div className="lg-card p-6">
        <h3 className="text-lg font-medium text-white mb-4">Two-Factor Authentication</h3>
        
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center space-x-3">
            <Security className="w-6 h-6 text-indigo-400" />
            <div>
              <div className="text-white font-medium">Two-Factor Authentication</div>
              <div className="text-sm text-white/60">
                {twoFactorEnabled ? 'Enabled - Your account is protected with 2FA' : 'Disabled - Add an extra layer of security'}
              </div>
            </div>
          </div>
          
          {!twoFactorEnabled ? (
            <button
              onClick={setupTwoFactor}
              disabled={isSettingUp2FA}
              className="lg-button lg-button-primary"
            >
              {isSettingUp2FA ? 'Setting up...' : 'Enable 2FA'}
            </button>
          ) : (
            <button
              onClick={disableTwoFactor}
              className="lg-button lg-button-secondary"
            >
              Disable 2FA
            </button>
          )}
        </div>

        {twoFactorEnabled && (
          <div className="lg-badge-success p-3">
            <span className="text-sm">
              Two-factor authentication is enabled. You'll need to enter a verification code from your authenticator app when signing in.
            </span>
          </div>
        )}
      </div>

      {/* Two-Factor Setup Dialog */}
      {showTwoFactorSetup && (
        <div className="lg-modal-overlay">
          <div className="lg-modal p-6 max-w-md w-full">
            <h3 className="text-xl font-bold text-white mb-4">Setup Two-Factor Authentication</h3>
            
            <div className="space-y-4">
              <div className="text-center">
                <div className="bg-white p-4 rounded-lg inline-block mb-4">
                  <QrCode2 className="w-32 h-32 text-gray-800" />
                </div>
                <p className="text-white/60 text-sm mb-4">
                  Scan this QR code with your authenticator app (Google Authenticator, Authy, etc.)
                </p>
                <div className="bg-white/10 p-3 rounded-lg">
                  <p className="text-white font-mono text-sm">{twoFactorSecret}</p>
                  <p className="text-white/60 text-xs mt-1">Or enter this code manually</p>
                </div>
              </div>
              
              <div>
                <label className="block text-sm font-medium text-white/80 mb-2">Verification Code</label>
                <input
                  type="text"
                  value={verificationCode}
                  onChange={(e) => setVerificationCode(e.target.value)}
                  className="lg-input text-center text-lg tracking-widest"
                  placeholder="000000"
                  maxLength={6}
                />
              </div>
            </div>
            
            <div className="flex justify-end space-x-3 mt-6">
              <button
                onClick={() => setShowTwoFactorSetup(false)}
                className="lg-button"
              >
                Cancel
              </button>
              <button
                onClick={verifyTwoFactor}
                disabled={verificationCode.length !== 6}
                className="lg-button lg-button-primary"
              >
                Verify & Enable
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Security Recommendations */}
      <div className="lg-card p-6">
        <h3 className="text-lg font-medium text-white mb-4">Security Recommendations</h3>
        
        <div className="space-y-3">
          <div className="flex items-start space-x-3">
            <div className="w-2 h-2 bg-green-400 rounded-full mt-2"></div>
            <div>
              <div className="text-white font-medium">Use a strong password</div>
              <div className="text-sm text-white/60">Include uppercase, lowercase, numbers, and special characters</div>
            </div>
          </div>
          
          <div className="flex items-start space-x-3">
            <div className="w-2 h-2 bg-green-400 rounded-full mt-2"></div>
            <div>
              <div className="text-white font-medium">Enable two-factor authentication</div>
              <div className="text-sm text-white/60">Add an extra layer of security to your account</div>
            </div>
          </div>
          
          <div className="flex items-start space-x-3">
            <div className="w-2 h-2 bg-yellow-400 rounded-full mt-2"></div>
            <div>
              <div className="text-white font-medium">Regular password updates</div>
              <div className="text-sm text-white/60">Change your password every 3-6 months</div>
            </div>
          </div>
          
          <div className="flex items-start space-x-3">
            <div className="w-2 h-2 bg-yellow-400 rounded-full mt-2"></div>
            <div>
              <div className="text-white font-medium">Monitor account activity</div>
              <div className="text-sm text-white/60">Review login history and report suspicious activity</div>
            </div>
          </div>
        </div>
      </div>

      {/* Snackbar */}
      {snackbar.open && (
        <div className={`fixed bottom-4 right-4 p-4 rounded-lg shadow-lg z-50 ${
          snackbar.severity === 'success' ? 'bg-green-600' : 'bg-red-600'
        } text-white`}>
          <div className="flex items-center space-x-2">
            <span>{snackbar.message}</span>
            <button
              onClick={() => setSnackbar(prev => ({ ...prev, open: false }))}
              className="ml-2 text-white/80 hover:text-white"
            >
              ×
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default SecurityTab;
