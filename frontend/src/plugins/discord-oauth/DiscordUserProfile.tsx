import React, { useState, useEffect } from 'react';
import { useAuth } from '../../context/AuthContext';
import { usePlugin } from '../PluginAPI';

interface DiscordUserProfileProps {
  userId?: string;
  className?: string;
}

interface DiscordUserData {
  id: string;
  username: string;
  discriminator: string;
  globalName?: string;
  avatar?: string;
  email?: string;
  verified: boolean;
  locale?: string;
  mfaEnabled: boolean;
  premiumType: number;
}

const DiscordUserProfile: React.FC<DiscordUserProfileProps> = ({
  userId,
  className = ''
}) => {
  const { user } = useAuth();
  const plugin = usePlugin('discord-oauth-plugin');
  const [userData, setUserData] = useState<DiscordUserData | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string>('');

  useEffect(() => {
    if (userId || user?.provider === 'discord') {
      loadDiscordUserData();
    }
  }, [userId, user]);

  const loadDiscordUserData = async () => {
    try {
      setLoading(true);
      setError('');

      const targetUserId = userId || user?.id;
      if (!targetUserId) {
        setError('No user ID provided');
        return;
      }

      // In a real implementation, this would fetch Discord user data from your backend
      // For now, we'll simulate the data structure
      const mockData: DiscordUserData = {
        id: targetUserId,
        username: 'discorduser',
        discriminator: '1234',
        globalName: 'Discord User',
        avatar: 'avatar_hash',
        email: 'user@example.com',
        verified: true,
        locale: 'en-US',
        mfaEnabled: false,
        premiumType: 0
      };

      setUserData(mockData);
    } catch (err) {
      setError('Failed to load Discord user data');
      console.error('Error loading Discord user data:', err);
    } finally {
      setLoading(false);
    }
  };

  const getAvatarUrl = (userId: string, avatarHash?: string) => {
    if (!avatarHash) {
      return `https://cdn.discordapp.com/embed/avatars/${parseInt(userId) % 5}.png`;
    }
    return `https://cdn.discordapp.com/avatars/${userId}/${avatarHash}.png`;
  };

  const getPremiumBadge = (premiumType: number) => {
    switch (premiumType) {
      case 1:
        return { name: 'Nitro Classic', color: 'bg-indigo-100 text-indigo-800' };
      case 2:
        return { name: 'Nitro', color: 'bg-purple-100 text-purple-800' };
      case 3:
        return { name: 'Nitro Basic', color: 'bg-blue-100 text-blue-800' };
      default:
        return null;
    }
  };

  if (loading) {
    return (
      <div className={`animate-pulse ${className}`}>
        <div className="flex items-center space-x-3">
          <div className="w-12 h-12 bg-gray-300 rounded-full"></div>
          <div className="space-y-2">
            <div className="h-4 bg-gray-300 rounded w-24"></div>
            <div className="h-3 bg-gray-300 rounded w-16"></div>
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className={`text-red-600 text-sm ${className}`}>
        {error}
      </div>
    );
  }

  if (!userData) {
    return null;
  }

  const premiumBadge = getPremiumBadge(userData.premiumType);

  return (
    <div className={`bg-white rounded-lg shadow p-4 ${className}`}>
      <div className="flex items-center space-x-4">
        <div className="relative">
          <img
            src={getAvatarUrl(userData.id, userData.avatar)}
            alt={`${userData.username}'s avatar`}
            className="w-12 h-12 rounded-full"
          />
          {userData.verified && (
            <div className="absolute -bottom-1 -right-1 w-4 h-4 bg-blue-500 rounded-full flex items-center justify-center">
              <svg className="w-2.5 h-2.5 text-white" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
              </svg>
            </div>
          )}
        </div>
        
        <div className="flex-1">
          <div className="flex items-center space-x-2">
            <h3 className="text-lg font-medium text-gray-900">
              {userData.globalName || userData.username}
            </h3>
            {premiumBadge && (
              <span className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${premiumBadge.color}`}>
                {premiumBadge.name}
              </span>
            )}
          </div>
          
          <p className="text-sm text-gray-600">
            @{userData.username}#{userData.discriminator}
          </p>
          
          {userData.email && (
            <p className="text-sm text-gray-500">
              {userData.email}
            </p>
          )}
        </div>
      </div>

      <div className="mt-4 grid grid-cols-2 gap-4 text-sm">
        <div>
          <span className="text-gray-500">User ID:</span>
          <span className="ml-2 font-mono text-gray-900">{userData.id}</span>
        </div>
        
        <div>
          <span className="text-gray-500">Locale:</span>
          <span className="ml-2 text-gray-900">{userData.locale || 'Unknown'}</span>
        </div>
        
        <div>
          <span className="text-gray-500">MFA:</span>
          <span className={`ml-2 ${userData.mfaEnabled ? 'text-green-600' : 'text-gray-500'}`}>
            {userData.mfaEnabled ? 'Enabled' : 'Disabled'}
          </span>
        </div>
        
        <div>
          <span className="text-gray-500">Verified:</span>
          <span className={`ml-2 ${userData.verified ? 'text-green-600' : 'text-red-600'}`}>
            {userData.verified ? 'Yes' : 'No'}
          </span>
        </div>
      </div>

      <div className="mt-4 flex space-x-2">
        <a
          href={`https://discord.com/users/${userData.id}`}
          target="_blank"
          rel="noopener noreferrer"
          className="inline-flex items-center px-3 py-2 border border-gray-300 shadow-sm text-sm leading-4 font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50"
        >
          <svg className="w-4 h-4 mr-2" viewBox="0 0 24 24" fill="currentColor">
            <path d="M20.317 4.37a19.791 19.791 0 0 0-4.885-1.515.074.074 0 0 0-.079.037c-.21.375-.444.864-.608 1.25a18.27 18.27 0 0 0-5.487 0 12.64 12.64 0 0 0-.617-1.25.077.077 0 0 0-.079-.037A19.736 19.736 0 0 0 3.677 4.37a.07.07 0 0 0-.032.027C.533 9.046-.32 13.58.099 18.057a.082.082 0 0 0 .031.057 19.9 19.9 0 0 0 5.993 3.03.078.078 0 0 0 .084-.028 14.09 14.09 0 0 0 1.226-1.994.076.076 0 0 0-.041-.106 13.107 13.107 0 0 1-1.872-.892.077.077 0 0 1-.008-.128 10.2 10.2 0 0 0 .372-.292.074.074 0 0 1 .077-.01c3.928 1.793 8.18 1.793 12.062 0a.074.074 0 0 1 .078.01c.12.098.246.198.373.292a.077.077 0 0 1-.006.127 12.299 12.299 0 0 1-1.873.892.077.077 0 0 0-.041.107c.36.698.772 1.362 1.225 1.993a.076.076 0 0 0 .084.028 19.839 19.839 0 0 0 6.002-3.03.077.077 0 0 0 .032-.054c.5-5.177-.838-9.674-3.549-13.66a.061.061 0 0 0-.031-.03zM8.02 15.33c-1.183 0-2.157-1.085-2.157-2.419 0-1.333.956-2.419 2.157-2.419 1.21 0 2.176 1.096 2.157 2.42 0 1.333-.956 2.418-2.157 2.418zm7.975 0c-1.183 0-2.157-1.085-2.157-2.419 0-1.333.955-2.419 2.157-2.419 1.21 0 2.176 1.096 2.157 2.42 0 1.333-.946 2.418-2.157 2.418z"/>
          </svg>
          View on Discord
        </a>
      </div>
    </div>
  );
};

export default DiscordUserProfile;
