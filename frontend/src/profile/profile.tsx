import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { useAppSelector } from '../store/hooks';
import { selectCurrentUserId } from '../store/slices/auth-slice';
import { getUserProfile } from './profile.service';
import type { UserProfile } from './profile.service';

interface ProfileStats {
  followers: number;
  following: number;
  likes: number;
  downloads: number;
}

export const Profile = () => {
  const { userId: urlUserId } = useParams<{ userId: string }>();
  const authUserId = useAppSelector(selectCurrentUserId);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [activeTab, setActiveTab] = useState<'models' | 'collections'>('models');

  // Use URL userId if provided, otherwise use the authenticated user's ID
  const targetUserId = urlUserId || authUserId;

  useEffect(() => {
    const fetchProfile = async () => {
      if (!targetUserId) {
        setError('No user ID available');
        setIsLoading(false);
        return;
      }

      try {
        setIsLoading(true);
        setError(null);
        const profileData = await getUserProfile(targetUserId);
        setProfile(profileData);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load profile');
      } finally {
        setIsLoading(false);
      }
    };

    fetchProfile();
  }, [targetUserId]);

  // Mock stats - in a real app, these would come from the API
  const stats: ProfileStats = {
    followers: 4300,
    following: 19,
    likes: 20500,
    downloads: 53500
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="w-8 h-8 border-4 border-green-500 rounded-full border-t-transparent animate-spin"></div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-red-500">{error}</div>
      </div>
    );
  }

  if (!profile) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-gray-500">Profile not found</div>
      </div>
    );
  }

  return (
    <div className="container px-4 py-8 mx-auto">
      {/* Profile Header */}
      <div className="flex items-start gap-8 mb-8">
        <img 
          src={profile.avatarUrl || '/default-avatar.jpg'} 
          alt={profile.displayName || profile.username} 
          className="w-32 h-32 rounded-full"
        />
        <div className="flex-1">
          <div className="flex items-center gap-2 mb-2">
            <h1 className="text-2xl font-bold">{profile.displayName || profile.username}</h1>
          </div>
          <p className="mb-4 text-gray-500">@{profile.username}</p>
          <p className="mb-4">{profile.bio || 'No bio available'}</p>
          
          {/* Stats */}
          <div className="flex gap-6">
            <div>
              <span className="font-bold">{stats.followers}</span>
              <span className="ml-1 text-gray-500">Followers</span>
            </div>
            <div>
              <span className="font-bold">{stats.following}</span>
              <span className="ml-1 text-gray-500">Following</span>
            </div>
            <div>
              <span className="font-bold">{stats.likes}</span>
              <span className="ml-1 text-gray-500">Likes</span>
            </div>
          </div>
        </div>
        {/* Only show follow button if viewing another user's profile */}
        {urlUserId && urlUserId !== authUserId && (
          <button className="btn-primary">Follow</button>
        )}
      </div>

      {/* Tabs */}
      <div className="mb-8 border-b">
        <div className="flex gap-8">
          <button 
            className={`pb-4 px-2 ${activeTab === 'models' ? 'border-b-2 border-green-400 font-bold' : 'text-gray-500'}`}
            onClick={() => setActiveTab('models')}
          >
            Models
          </button>
          <button 
            className={`pb-4 px-2 ${activeTab === 'collections' ? 'border-b-2 border-green-400 font-bold' : 'text-gray-500'}`}
            onClick={() => setActiveTab('collections')}
          >
            Collections
          </button>
        </div>
      </div>

      {/* Content */}
      {activeTab === 'models' && (
        <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
          {/* Models would be mapped here */}
          <div className="text-gray-500">No models yet</div>
        </div>
      )}
      
      {activeTab === 'collections' && (
        <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
          {/* Collections would be mapped here */}
          <div className="text-gray-500">No collections yet</div>
        </div>
      )}
    </div>
  );
}; 