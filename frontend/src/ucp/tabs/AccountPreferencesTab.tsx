import React, { useState, useEffect } from 'react';
import { useAppSelector } from '../../utils/hooks';
import api from '../../utils/axiosConfig';
import { minidenticon } from 'minidenticons';
import { Edit as EditIcon, Check as CheckIcon, Close as CloseIcon } from '@mui/icons-material';

const AccountPreferencesTab: React.FC = () => {
  const { user } = useAppSelector((state) => state.auth);
  const [profile, setProfile] = useState({
    username: '',
    email: '',
    firstName: '',
    lastName: '',
    bio: '',
    country: '',
    websiteUrl: '',
    twitterUrl: '',
    instagramUrl: '',
    youtubeUrl: '',
    isProfilePublic: true,
    showEmail: false,
    showLastLogin: false,
    showStatistics: true,
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
  const [editingField, setEditingField] = useState<string | null>(null);
  const [editValues, setEditValues] = useState<Record<string, string>>({});

  useEffect(() => {
    if (user) {
      setProfile(prev => ({
        ...prev,
        username: user.username || '',
        email: user.email || '',
        firstName: user.firstName || '',
        lastName: user.lastName || '',
        bio: user.bio || '',
        country: user.country || '',
        websiteUrl: user.websiteUrl || '',
        twitterUrl: user.twitterUrl || '',
        instagramUrl: user.instagramUrl || '',
        youtubeUrl: user.youtubeUrl || '',
        isProfilePublic: user.isProfilePublic ?? true,
        showEmail: user.showEmail ?? false,
        showLastLogin: user.showLastLogin ?? false,
        showStatistics: user.showStatistics ?? true,
        avatar: user.avatar || ''
      }));
    }
  }, [user]);

  const handleProfileChange = (field: string, value: any) => {
    setProfile(prev => ({ ...prev, [field]: value }));
  };

  const startEditing = (field: string) => {
    setEditingField(field);
    setEditValues({ [field]: profile[field as keyof typeof profile] as string || '' });
  };

  const cancelEditing = () => {
    setEditingField(null);
    setEditValues({});
  };

  const saveField = (field: string) => {
    const value = editValues[field] || '';
    handleProfileChange(field, value);
    setEditingField(null);
    setEditValues({});
  };

  const handleEditValueChange = (field: string, value: string) => {
    setEditValues(prev => ({ ...prev, [field]: value }));
  };

  const getDisplayValue = (field: string): string => {
    const value = profile[field as keyof typeof profile] as string;
    return value && value.trim() ? value : 'n/a';
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
      await api.put('/api/users/profile/update', {
        bio: profile.bio,
        country: profile.country,
        websiteUrl: profile.websiteUrl || null,
        twitterUrl: profile.twitterUrl || null,
        instagramUrl: profile.instagramUrl || null,
        youtubeUrl: profile.youtubeUrl || null,
        isProfilePublic: profile.isProfilePublic,
        showEmail: profile.showEmail,
        showLastLogin: profile.showLastLogin,
        showStatistics: profile.showStatistics
      });
      setSnackbar({ open: true, message: 'Profile updated successfully', severity: 'success' });
    } catch (error) {
      console.error('Error updating profile:', error);
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
    <div className="space-y-4">
      <h2 className="text-xl font-bold text-white mb-3">Basic Information</h2>
      
      {/* Profile Information */}
      <div className="lg-card p-4">
        <div className="space-y-0">
          {/* Avatar Row */}
          <div className="flex items-center py-2 border-b border-white/10">
            <span className="text-sm text-white/70 w-32 flex-shrink-0">Avatar</span>
            <div className="flex items-center space-x-2 ml-auto">
              <div className="w-10 h-10 rounded-full overflow-hidden border-2 border-white/20">
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
                className="lg-button lg-button-secondary text-xs cursor-pointer py-1 px-2"
              >
                Change
              </label>
            </div>
          </div>

          {/* Username Row - Read-only */}
          <div className="flex items-center py-2 border-b border-white/10">
            <span className="text-sm text-white/70 w-32 flex-shrink-0">Username</span>
            <span className="text-sm text-white/50 ml-auto">@{profile.username}</span>
          </div>

          {/* Email Row - Read-only */}
          <div className="flex items-center py-2 border-b border-white/10">
            <span className="text-sm text-white/70 w-32 flex-shrink-0">Email</span>
            <span className="text-sm text-white/50 ml-auto">{profile.email}</span>
          </div>

          {/* Bio Row - Editable */}
          <div className="flex items-center py-2 border-b border-white/10">
            <span className="text-sm text-white/70 w-32 flex-shrink-0">Bio</span>
            {editingField === 'bio' ? (
              <div className="flex items-center gap-2 ml-auto w-64 max-w-md">
                <input
                  type="text"
                  value={editValues.bio || ''}
                  onChange={(e) => handleEditValueChange('bio', e.target.value)}
                  placeholder="Tell us about yourself..."
                  className="lg-input text-sm py-1.5 flex-1"
                  maxLength={500}
                  autoFocus
                />
                <button
                  onClick={() => saveField('bio')}
                  className="text-green-400 hover:text-green-300 transition-colors p-1"
                  title="Save"
                >
                  <CheckIcon style={{ fontSize: '18px' }} />
                </button>
                <button
                  onClick={cancelEditing}
                  className="text-red-400 hover:text-red-300 transition-colors p-1"
                  title="Cancel"
                >
                  <CloseIcon style={{ fontSize: '18px' }} />
                </button>
              </div>
            ) : (
              <div className="flex items-center gap-2 ml-auto">
                <span className="text-sm text-white">{getDisplayValue('bio')}</span>
                <button
                  onClick={() => startEditing('bio')}
                  className="text-white/40 hover:text-white/70 transition-colors p-1"
                  title="Edit"
                >
                  <EditIcon style={{ fontSize: '16px' }} />
                </button>
              </div>
            )}
          </div>

          {/* Country Row - Editable */}
          <div className="flex items-center py-2 border-b border-white/10">
            <span className="text-sm text-white/70 w-32 flex-shrink-0">Country</span>
            {editingField === 'country' ? (
              <div className="flex items-center gap-2 ml-auto w-64 max-w-md">
                <input
                  type="text"
                  value={editValues.country || ''}
                  onChange={(e) => handleEditValueChange('country', e.target.value)}
                  placeholder="Your country"
                  className="lg-input text-sm py-1.5 flex-1"
                  maxLength={100}
                  autoFocus
                />
                <button
                  onClick={() => saveField('country')}
                  className="text-green-400 hover:text-green-300 transition-colors p-1"
                  title="Save"
                >
                  <CheckIcon style={{ fontSize: '18px' }} />
                </button>
                <button
                  onClick={cancelEditing}
                  className="text-red-400 hover:text-red-300 transition-colors p-1"
                  title="Cancel"
                >
                  <CloseIcon style={{ fontSize: '18px' }} />
                </button>
              </div>
            ) : (
              <div className="flex items-center gap-2 ml-auto">
                <span className="text-sm text-white">{getDisplayValue('country')}</span>
                <button
                  onClick={() => startEditing('country')}
                  className="text-white/40 hover:text-white/70 transition-colors p-1"
                  title="Edit"
                >
                  <EditIcon style={{ fontSize: '16px' }} />
                </button>
              </div>
            )}
          </div>

          {/* Website URL Row - Editable */}
          <div className="flex items-center py-2 border-b border-white/10">
            <span className="text-sm text-white/70 w-32 flex-shrink-0">Website</span>
            {editingField === 'websiteUrl' ? (
              <div className="flex items-center gap-2 ml-auto w-64 max-w-md">
                <input
                  type="url"
                  value={editValues.websiteUrl || ''}
                  onChange={(e) => handleEditValueChange('websiteUrl', e.target.value)}
                  placeholder="https://example.com"
                  className="lg-input text-sm py-1.5 flex-1"
                  maxLength={500}
                  autoFocus
                />
                <button
                  onClick={() => saveField('websiteUrl')}
                  className="text-green-400 hover:text-green-300 transition-colors p-1"
                  title="Save"
                >
                  <CheckIcon style={{ fontSize: '18px' }} />
                </button>
                <button
                  onClick={cancelEditing}
                  className="text-red-400 hover:text-red-300 transition-colors p-1"
                  title="Cancel"
                >
                  <CloseIcon style={{ fontSize: '18px' }} />
                </button>
              </div>
            ) : (
              <div className="flex items-center gap-2 ml-auto">
                <span className="text-sm text-white">{getDisplayValue('websiteUrl')}</span>
                <button
                  onClick={() => startEditing('websiteUrl')}
                  className="text-white/40 hover:text-white/70 transition-colors p-1"
                  title="Edit"
                >
                  <EditIcon style={{ fontSize: '16px' }} />
                </button>
              </div>
            )}
          </div>

          {/* Twitter URL Row - Editable */}
          <div className="flex items-center py-2 border-b border-white/10">
            <span className="text-sm text-white/70 w-32 flex-shrink-0">Twitter</span>
            {editingField === 'twitterUrl' ? (
              <div className="flex items-center gap-2 ml-auto w-64 max-w-md">
                <input
                  type="url"
                  value={editValues.twitterUrl || ''}
                  onChange={(e) => handleEditValueChange('twitterUrl', e.target.value)}
                  placeholder="https://twitter.com/username"
                  className="lg-input text-sm py-1.5 flex-1"
                  maxLength={500}
                  autoFocus
                />
                <button
                  onClick={() => saveField('twitterUrl')}
                  className="text-green-400 hover:text-green-300 transition-colors p-1"
                  title="Save"
                >
                  <CheckIcon style={{ fontSize: '18px' }} />
                </button>
                <button
                  onClick={cancelEditing}
                  className="text-red-400 hover:text-red-300 transition-colors p-1"
                  title="Cancel"
                >
                  <CloseIcon style={{ fontSize: '18px' }} />
                </button>
              </div>
            ) : (
              <div className="flex items-center gap-2 ml-auto">
                <span className="text-sm text-white">{getDisplayValue('twitterUrl')}</span>
                <button
                  onClick={() => startEditing('twitterUrl')}
                  className="text-white/40 hover:text-white/70 transition-colors p-1"
                  title="Edit"
                >
                  <EditIcon style={{ fontSize: '16px' }} />
                </button>
              </div>
            )}
          </div>

          {/* Instagram URL Row - Editable */}
          <div className="flex items-center py-2 border-b border-white/10">
            <span className="text-sm text-white/70 w-32 flex-shrink-0">Instagram</span>
            {editingField === 'instagramUrl' ? (
              <div className="flex items-center gap-2 ml-auto w-64 max-w-md">
                <input
                  type="url"
                  value={editValues.instagramUrl || ''}
                  onChange={(e) => handleEditValueChange('instagramUrl', e.target.value)}
                  placeholder="https://instagram.com/username"
                  className="lg-input text-sm py-1.5 flex-1"
                  maxLength={500}
                  autoFocus
                />
                <button
                  onClick={() => saveField('instagramUrl')}
                  className="text-green-400 hover:text-green-300 transition-colors p-1"
                  title="Save"
                >
                  <CheckIcon style={{ fontSize: '18px' }} />
                </button>
                <button
                  onClick={cancelEditing}
                  className="text-red-400 hover:text-red-300 transition-colors p-1"
                  title="Cancel"
                >
                  <CloseIcon style={{ fontSize: '18px' }} />
                </button>
              </div>
            ) : (
              <div className="flex items-center gap-2 ml-auto">
                <span className="text-sm text-white">{getDisplayValue('instagramUrl')}</span>
                <button
                  onClick={() => startEditing('instagramUrl')}
                  className="text-white/40 hover:text-white/70 transition-colors p-1"
                  title="Edit"
                >
                  <EditIcon style={{ fontSize: '16px' }} />
                </button>
              </div>
            )}
          </div>

          {/* YouTube URL Row - Editable */}
          <div className="flex items-center py-2">
            <span className="text-sm text-white/70 w-32 flex-shrink-0">YouTube</span>
            {editingField === 'youtubeUrl' ? (
              <div className="flex items-center gap-2 ml-auto w-64 max-w-md">
                <input
                  type="url"
                  value={editValues.youtubeUrl || ''}
                  onChange={(e) => handleEditValueChange('youtubeUrl', e.target.value)}
                  placeholder="https://youtube.com/username"
                  className="lg-input text-sm py-1.5 flex-1"
                  maxLength={500}
                  autoFocus
                />
                <button
                  onClick={() => saveField('youtubeUrl')}
                  className="text-green-400 hover:text-green-300 transition-colors p-1"
                  title="Save"
                >
                  <CheckIcon style={{ fontSize: '18px' }} />
                </button>
                <button
                  onClick={cancelEditing}
                  className="text-red-400 hover:text-red-300 transition-colors p-1"
                  title="Cancel"
                >
                  <CloseIcon style={{ fontSize: '18px' }} />
                </button>
              </div>
            ) : (
              <div className="flex items-center gap-2 ml-auto">
                <span className="text-sm text-white">{getDisplayValue('youtubeUrl')}</span>
                <button
                  onClick={() => startEditing('youtubeUrl')}
                  className="text-white/40 hover:text-white/70 transition-colors p-1"
                  title="Edit"
                >
                  <EditIcon style={{ fontSize: '16px' }} />
                </button>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Privacy Settings */}
      <div className="lg-card p-4">
        <h3 className="text-base font-medium text-white mb-3">Privacy Settings</h3>
        
        <div className="space-y-0">
          <div className="flex items-center py-2 border-b border-white/10">
            <div className="flex-1">
              <span className="text-sm text-white font-medium">Public Profile</span>
              <p className="text-xs text-white/60">Allow others to view your profile</p>
            </div>
            <input
              type="checkbox"
              checked={profile.isProfilePublic}
              onChange={(e) => handleProfileChange('isProfilePublic', e.target.checked)}
              className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent cursor-pointer"
            />
          </div>
          
          <div className="flex items-center py-2 border-b border-white/10">
            <div className="flex-1">
              <span className="text-sm text-white font-medium">Show Email</span>
              <p className="text-xs text-white/60">Display your email on your profile</p>
            </div>
            <input
              type="checkbox"
              checked={profile.showEmail}
              onChange={(e) => handleProfileChange('showEmail', e.target.checked)}
              className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent cursor-pointer"
            />
          </div>
          
          <div className="flex items-center py-2 border-b border-white/10">
            <div className="flex-1">
              <span className="text-sm text-white font-medium">Show Last Login</span>
              <p className="text-xs text-white/60">Display when you last logged in</p>
            </div>
            <input
              type="checkbox"
              checked={profile.showLastLogin}
              onChange={(e) => handleProfileChange('showLastLogin', e.target.checked)}
              className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent cursor-pointer"
            />
          </div>
          
          <div className="flex items-center py-2">
            <div className="flex-1">
              <span className="text-sm text-white font-medium">Show Statistics</span>
              <p className="text-xs text-white/60">Display your model and collection statistics</p>
            </div>
            <input
              type="checkbox"
              checked={profile.showStatistics}
              onChange={(e) => handleProfileChange('showStatistics', e.target.checked)}
              className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent cursor-pointer"
            />
          </div>
        </div>
      </div>

      {/* Email Notifications */}
      <div className="lg-card p-4">
        <h3 className="text-base font-medium text-white mb-3">Email Notifications</h3>
        
        <div className="space-y-2.5">
          <div className="flex items-center justify-between py-1.5">
            <div className="flex-1">
              <div className="text-sm text-white font-medium">New Followers</div>
              <div className="text-xs text-white/60">Get notified when someone follows you</div>
            </div>
            <input
              type="checkbox"
              checked={profile.emailNotifications.newFollowers}
              onChange={(e) => handleNotificationChange('newFollowers', e.target.checked)}
              className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent cursor-pointer"
            />
          </div>
          
          <div className="flex items-center justify-between py-1.5">
            <div className="flex-1">
              <div className="text-sm text-white font-medium">Collection Updates</div>
              <div className="text-xs text-white/60">Get notified about collection changes</div>
            </div>
            <input
              type="checkbox"
              checked={profile.emailNotifications.collectionUpdates}
              onChange={(e) => handleNotificationChange('collectionUpdates', e.target.checked)}
              className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent cursor-pointer"
            />
          </div>
          
          <div className="flex items-center justify-between py-1.5">
            <div className="flex-1">
              <div className="text-sm text-white font-medium">Model Comments</div>
              <div className="text-xs text-white/60">Get notified about comments on your models</div>
            </div>
            <input
              type="checkbox"
              checked={profile.emailNotifications.modelComments}
              onChange={(e) => handleNotificationChange('modelComments', e.target.checked)}
              className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent cursor-pointer"
            />
          </div>
          
          <div className="flex items-center justify-between py-1.5">
            <div className="flex-1">
              <div className="text-sm text-white font-medium">System Announcements</div>
              <div className="text-xs text-white/60">Get notified about system updates and maintenance</div>
            </div>
            <input
              type="checkbox"
              checked={profile.emailNotifications.systemAnnouncements}
              onChange={(e) => handleNotificationChange('systemAnnouncements', e.target.checked)}
              className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-white/20 rounded bg-transparent cursor-pointer"
            />
          </div>
        </div>
      </div>

      {/* Save Button */}
      <div className="flex justify-end pt-2">
        <button
          onClick={saveProfile}
          disabled={isSaving}
          className="lg-button lg-button-primary text-sm py-2 px-4"
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
