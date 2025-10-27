import React, { useState, useEffect } from 'react';
import { useAppSelector } from '../../utils/hooks';
import api from '../../utils/axiosConfig';
import { minidenticon } from 'minidenticons';

const AccountPreferencesTab: React.FC = () => {
  const { user } = useAppSelector((state) => state.auth);
  const [profile, setProfile] = useState({
    username: '',
    email: '',
    firstName: '',
    lastName: '',
    bio: '',
    avatar: '',
    timezone: 'UTC',
    language: 'en',
    emailNotifications: {
      newFollowers: true,
      collectionUpdates: true,
      modelComments: true,
      systemAnnouncements: true
    }
  });
  const [isSaving, setIsSaving] = useState(false);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' as 'success' | 'error' });

  useEffect(() => {
    if (user) {
      setProfile(prev => ({
        ...prev,
        username: user.username || '',
        email: user.email || '',
        firstName: user.firstName || '',
        lastName: user.lastName || '',
        bio: user.bio || '',
        avatar: user.avatar || ''
      }));
    }
  }, [user]);

  const handleProfileChange = (field: string, value: any) => {
    setProfile(prev => ({ ...prev, [field]: value }));
  };

  const handleNotificationChange = (key: string, value: boolean) => {
    setProfile(prev => ({
      ...prev,
      emailNotifications: {
        ...prev.emailNotifications,
        [key]: value
      }
    }));
  };

  const saveProfile = async () => {
    setIsSaving(true);
    try {
      // Mock API call - replace with actual endpoint
      await new Promise(resolve => setTimeout(resolve, 1000));
      setSnackbar({ open: true, message: 'Profile updated successfully', severity: 'success' });
    } catch (error) {
      setSnackbar({ open: true, message: 'Failed to update profile', severity: 'error' });
    } finally {
      setIsSaving(false);
    }
  };

  const uploadAvatar = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      try {
        // Mock avatar upload - replace with actual endpoint
        await new Promise(resolve => setTimeout(resolve, 1000));
        const avatarUrl = URL.createObjectURL(file);
        setProfile(prev => ({ ...prev, avatar: avatarUrl }));
        setSnackbar({ open: true, message: 'Avatar updated successfully', severity: 'success' });
      } catch (error) {
        setSnackbar({ open: true, message: 'Failed to upload avatar', severity: 'error' });
      }
    }
  };

  const getAvatarUrl = () => {
    if (profile.avatar) {
      return profile.avatar;
    }
    if (profile.username) {
      return `data:image/svg+xml;utf8,${encodeURIComponent(minidenticon(profile.username, 80))}`;
    }
    return '';
  };

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold text-white">Basic Information</h2>
      
      {/* Profile Information */}
      <div className="lg-card p-6">
        <div className="space-y-4">
          {/* Avatar Row */}
          <div className="flex items-center justify-between py-3 border-b border-white/10">
            <span className="text-white/80">Avatar</span>
            <div className="flex items-center space-x-3">
              <div className="w-12 h-12 rounded-full overflow-hidden border-2 border-white/20">
                <img
                  src={getAvatarUrl()}
                  alt="Avatar"
                  className="w-full h-full object-cover"
                />
              </div>
              <input
                type="file"
                accept="image/*"
                onChange={uploadAvatar}
                className="hidden"
                id="avatar-upload"
              />
              <label
                htmlFor="avatar-upload"
                className="lg-button lg-button-secondary text-sm cursor-pointer"
              >
                Change
              </label>
            </div>
            <svg className="w-5 h-5 text-white/40" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
            </svg>
          </div>

          {/* Display Name Row */}
          <div className="flex items-center justify-between py-3 border-b border-white/10">
            <span className="text-white/80">Display Name</span>
            <span className="text-white">{profile.firstName && profile.lastName ? `${profile.firstName} ${profile.lastName}` : profile.username}</span>
            <svg className="w-5 h-5 text-white/40" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
            </svg>
          </div>

          {/* Username Row */}
          <div className="flex items-center justify-between py-3 border-b border-white/10">
            <span className="text-white/80">Username</span>
            <span className="text-white">@{profile.username}</span>
            <svg className="w-5 h-5 text-white/40" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
            </svg>
          </div>

          {/* Email Row */}
          <div className="flex items-center justify-between py-3 border-b border-white/10">
            <span className="text-white/80">Email</span>
            <div className="flex items-center space-x-2">
              <svg className="w-4 h-4 text-white/40" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
              </svg>
              <span className="text-white">{profile.email}</span>
            </div>
            <svg className="w-5 h-5 text-white/40" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
            </svg>
          </div>

          {/* Gender Row */}
          <div className="flex items-center justify-between py-3 border-b border-white/10">
            <span className="text-white/80">Gender</span>
            <div className="flex items-center space-x-2">
              <svg className="w-4 h-4 text-white/40" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
              </svg>
              <span className="text-white">Unknown</span>
            </div>
            <svg className="w-5 h-5 text-white/40" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
            </svg>
          </div>

          {/* Region Row */}
          <div className="flex items-center justify-between py-3 border-b border-white/10">
            <span className="text-white/80">Region</span>
            <div className="flex items-center space-x-2">
              <svg className="w-4 h-4 text-white/40" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
              </svg>
              <span className="text-white">--</span>
            </div>
            <svg className="w-5 h-5 text-white/40" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
            </svg>
          </div>

          {/* Bio Row */}
          <div className="flex items-center justify-between py-3">
            <span className="text-white/80">Bio</span>
            <span className="text-white">{profile.bio || ''}</span>
            <svg className="w-5 h-5 text-white/40" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
            </svg>
          </div>
        </div>
      </div>

      {/* Preferences */}
      <div className="lg-card p-6">
        <h3 className="text-lg font-medium text-white mb-4">Preferences</h3>
        
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-white/80 mb-2">Timezone</label>
            <select
              value={profile.timezone}
              onChange={(e) => handleProfileChange('timezone', e.target.value)}
              className="lg-input"
            >
              <option value="UTC">UTC</option>
              <option value="America/New_York">Eastern Time</option>
              <option value="America/Chicago">Central Time</option>
              <option value="America/Denver">Mountain Time</option>
              <option value="America/Los_Angeles">Pacific Time</option>
              <option value="Europe/London">London</option>
              <option value="Europe/Paris">Paris</option>
              <option value="Asia/Tokyo">Tokyo</option>
            </select>
          </div>
          
          <div>
            <label className="block text-sm font-medium text-white/80 mb-2">Language</label>
            <select
              value={profile.language}
              onChange={(e) => handleProfileChange('language', e.target.value)}
              className="lg-input"
            >
              <option value="en">English</option>
              <option value="es">Spanish</option>
              <option value="fr">French</option>
              <option value="de">German</option>
              <option value="ja">Japanese</option>
              <option value="zh">Chinese</option>
            </select>
          </div>
        </div>
      </div>

      {/* Email Notifications */}
      <div className="lg-card p-6">
        <h3 className="text-lg font-medium text-white mb-4">Email Notifications</h3>
        
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <div>
              <div className="text-white font-medium">New Followers</div>
              <div className="text-sm text-white/60">Get notified when someone follows you</div>
            </div>
            <input
              type="checkbox"
              checked={profile.emailNotifications.newFollowers}
              onChange={(e) => handleNotificationChange('newFollowers', e.target.checked)}
              className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
            />
          </div>
          
          <div className="flex items-center justify-between">
            <div>
              <div className="text-white font-medium">Collection Updates</div>
              <div className="text-sm text-white/60">Get notified about collection changes</div>
            </div>
            <input
              type="checkbox"
              checked={profile.emailNotifications.collectionUpdates}
              onChange={(e) => handleNotificationChange('collectionUpdates', e.target.checked)}
              className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
            />
          </div>
          
          <div className="flex items-center justify-between">
            <div>
              <div className="text-white font-medium">Model Comments</div>
              <div className="text-sm text-white/60">Get notified about comments on your models</div>
            </div>
            <input
              type="checkbox"
              checked={profile.emailNotifications.modelComments}
              onChange={(e) => handleNotificationChange('modelComments', e.target.checked)}
              className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
            />
          </div>
          
          <div className="flex items-center justify-between">
            <div>
              <div className="text-white font-medium">System Announcements</div>
              <div className="text-sm text-white/60">Get notified about system updates and maintenance</div>
            </div>
            <input
              type="checkbox"
              checked={profile.emailNotifications.systemAnnouncements}
              onChange={(e) => handleNotificationChange('systemAnnouncements', e.target.checked)}
              className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent"
            />
          </div>
        </div>
      </div>

      {/* Save Button */}
      <div className="flex justify-end">
        <button
          onClick={saveProfile}
          disabled={isSaving}
          className="lg-button lg-button-primary"
        >
          {isSaving ? 'Saving...' : 'Save Changes'}
        </button>
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

export default AccountPreferencesTab;
