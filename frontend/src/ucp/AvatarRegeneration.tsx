import React, { useState, useEffect } from 'react';
import { useAppSelector } from '../utils/hooks';
import UserAvatar from './UserAvatar';
import avatarService from '../services/avatarService';

interface AvatarRegenerationProps {
  onAvatarUpdate?: (avatar: string) => void;
  className?: string;
}

const AvatarRegeneration: React.FC<AvatarRegenerationProps> = ({ onAvatarUpdate, className = '' }) => {
  const { user } = useAppSelector((state) => state.auth);
  const [salt, setSalt] = useState('');
  const [previewAvatar, setPreviewAvatar] = useState<string | null>(null);
  const [isRegenerating, setIsRegenerating] = useState(false);
  const [error, setError] = useState('');

  // Generate preview when salt changes
  useEffect(() => {
    if (user?.id) {
      const preview = avatarService.previewAvatar(user.id, salt);
      setPreviewAvatar(preview);
    }
  }, [salt, user?.id]);

  const handleRegenerate = async () => {
    if (!user?.id) return;

    setIsRegenerating(true);
    setError('');

    try {
      const response = await avatarService.regenerateAvatar(salt || undefined);
      
      // Update the user's avatar in the Redux store
      if (onAvatarUpdate) {
        onAvatarUpdate(response.avatar);
      }

      // Show success feedback
      setError('');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to regenerate avatar');
    } finally {
      setIsRegenerating(false);
    }
  };

  const handleReset = () => {
    setSalt('');
    setError('');
  };

  if (!user) {
    return null;
  }

  return (
    <div className={`space-y-4 ${className}`}>
      <div>
        <h3 className="text-lg font-medium text-white mb-3">Avatar Customization</h3>
        <p className="text-sm text-white/60 mb-4">
          Add a custom salt to generate a unique avatar. Try different words or phrases to find one you like!
        </p>
      </div>

      {/* Current Avatar Display */}
      <div className="flex items-center space-x-4">
        <div className="text-center">
          <p className="text-sm text-white/80 mb-2">Current Avatar</p>
          <UserAvatar
            userId={user.id}
            username={user.username}
            avatar={user.avatar}
            size="lg"
            className="mx-auto"
          />
        </div>

        {/* Preview Avatar */}
        <div className="text-center">
          <p className="text-sm text-white/80 mb-2">Preview</p>
          <div className="w-16 h-16 rounded-full overflow-hidden bg-gray-100 flex items-center justify-center mx-auto">
            {previewAvatar && (
              <img
                src={`data:image/svg+xml,${encodeURIComponent(previewAvatar)}`}
                alt="Avatar preview"
                className="w-full h-full"
                style={{ imageRendering: 'pixelated' }}
              />
            )}
          </div>
        </div>
      </div>

      {/* Salt Input */}
      <div>
        <label htmlFor="avatar-salt" className="block text-sm font-medium text-white/80 mb-2">
          Custom Salt (optional)
        </label>
        <div className="flex space-x-2">
          <input
            type="text"
            id="avatar-salt"
            value={salt}
            onChange={(e) => setSalt(e.target.value)}
            placeholder="Enter a word or phrase..."
            className="lg-input flex-1"
            maxLength={50}
          />
          <button
            onClick={handleReset}
            className="lg-button"
            disabled={!salt}
          >
            Clear
          </button>
        </div>
        <p className="text-xs text-white/60 mt-1">
          This will be combined with your user ID to generate a unique avatar pattern.
        </p>
      </div>

      {/* Error Display */}
      {error && (
        <div className="text-red-400 text-sm">
          {error}
        </div>
      )}

      {/* Action Buttons */}
      <div className="flex space-x-3">
        <button
          onClick={handleRegenerate}
          disabled={isRegenerating}
          className="lg-button lg-button-primary"
        >
          {isRegenerating ? 'Regenerating...' : 'Save New Avatar'}
        </button>
      </div>
    </div>
  );
};

export default AvatarRegeneration; 