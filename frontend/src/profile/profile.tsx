import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { useSelector } from 'react-redux';
import { selectCurrentUserId } from '../store/slices/auth-slice';
import { getUserProfile } from './profile.service';
import type { UserProfile } from './profile.service';

interface ProfileStats {
  followers: number;
  following: number;
  likes: number;
  downloads: number;
}

interface ProfileData {
  username: string;
  displayName: string;
  avatarUrl: string;
  bio: string;
  isVerified: boolean;
  stats: ProfileStats;
  socialLinks?: {
    twitter?: string;
    website?: string;
    instagram?: string;
  };
}

export const Profile = () => {
  const { userId } = useParams();
  const currentUserId = useSelector(selectCurrentUserId);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [userProfile, setUserProfile] = useState<UserProfile | null>(null);
  const [activeTab, setActiveTab] = useState<'models' | 'collections'>('models');

  // Temporary mock data for UI elements we don't have in the API yet
  const mockProfileData: ProfileData = {
    username: userProfile?.username ?? "",
    displayName: userProfile?.username ?? "",
    avatarUrl: "/sample/avatar.jpg",
    bio: "Frontend Developer exploring 3D Printing 🚀",
    isVerified: true,
    stats: {
      followers: 4300,
      following: 19,
      likes: 20500,
      downloads: 53500
    },
    socialLinks: {
      twitter: "https://twitter.com/example",
      website: "https://example.com",
      instagram: "https://instagram.com/example"
    }
  };

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        setIsLoading(true);
        setError(null);
        
        // Use currentUserId from Redux instead of getCurrentUserId()
        const targetUserId = userId || currentUserId;
        const profile = await getUserProfile(targetUserId);
        
        setUserProfile(profile);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load profile');
      } finally {
        setIsLoading(false);
      }
    };

    fetchProfile();
  }, [userId, currentUserId]);

  if (isLoading) {
    return <div className="flex items-center justify-center min-h-screen">
      <div className="w-8 h-8 border-4 border-green-500 rounded-full border-t-transparent animate-spin"></div>
    </div>;
  }

  if (error) {
    return <div className="flex items-center justify-center min-h-screen">
      <div className="text-red-500">{error}</div>
    </div>;
  }

  return (
    <div className="container px-4 py-8 mx-auto">
      {/* Profile Header */}
      <div className="flex items-start gap-8 mb-8">
        <img 
          src={mockProfileData.avatarUrl} 
          alt={mockProfileData.displayName} 
          className="w-32 h-32 rounded-full"
        />
        <div className="flex-1">
          <div className="flex items-center gap-2 mb-2">
            <h1 className="text-2xl font-bold">{mockProfileData.displayName}</h1>
            {mockProfileData.isVerified && (
              <span className="text-green-400">✓</span>
            )}
          </div>
          <p className="mb-4 text-gray-500">@{userProfile?.username}</p>
          <p className="mb-4">{mockProfileData.bio}</p>
          
          {/* Stats */}
          <div className="flex gap-6">
            <div>
              <span className="font-bold">{mockProfileData.stats.followers}</span>
              <span className="ml-1 text-gray-500">Followers</span>
            </div>
            <div>
              <span className="font-bold">{mockProfileData.stats.following}</span>
              <span className="ml-1 text-gray-500">Following</span>
            </div>
            <div>
              <span className="font-bold">{mockProfileData.stats.likes}</span>
              <span className="ml-1 text-gray-500">Likes</span>
            </div>
          </div>

          {/* Social Links */}
          {mockProfileData.socialLinks && (
            <div className="flex gap-4 mt-4">
              {mockProfileData.socialLinks.twitter && (
                <a href={mockProfileData.socialLinks.twitter} className="text-gray-500 hover:text-gray-700">
                  Twitter
                </a>
              )}
              {mockProfileData.socialLinks.website && (
                <a href={mockProfileData.socialLinks.website} className="text-gray-500 hover:text-gray-700">
                  Website
                </a>
              )}
              {mockProfileData.socialLinks.instagram && (
                <a href={mockProfileData.socialLinks.instagram} className="text-gray-500 hover:text-gray-700">
                  Instagram
                </a>
              )}
            </div>
          )}
        </div>
        <button className="btn-primary">Follow</button>
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
        </div>
      )}
      
      {activeTab === 'collections' && (
        <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
          {/* Collections would be mapped here */}
        </div>
      )}
    </div>
  );
}; 