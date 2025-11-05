import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import NavigationBar from '../components/common/NavigationBar';
import UserAvatar from './UserAvatar';
import ModelGrid from '../models/ModelGrid';
import LayoutControls from '../components/common/LayoutControls';
import CollectionCard from '../collections/CollectionCard';
import { 
  Person, 
  Settings, 
  Collections, 
  Print, 
  Palette, 
  ThumbUp, 
  Comment, 
  Download, 
  CalendarToday, 
  LocationOn, 
  Language, 
  Link,
  Lock,
  ErrorOutline
} from '@mui/icons-material';
import api from '../utils/axiosConfig';
import { CircularProgress, Typography, TextField, InputAdornment, IconButton } from '@mui/material';
import { Search as SearchIcon, Clear as ClearIcon, NavigateBefore as NavigateBeforeIcon, NavigateNext as NavigateNextIcon } from '@mui/icons-material';

interface UserProfile {
  id: string;
  username: string;
  firstName?: string;
  lastName?: string;
  bio?: string;
  avatar?: string;
  profilePictureUrl?: string;
  country?: string;
  roleName: string;
  createdAt: string;
  lastLoginAt?: string;
  isBanned: boolean;
  bannedAt?: string;
  banReason?: string;
  banExpiresAt?: string;
  totalModels: number;
  totalCollections: number;
  totalLikes: number;
  totalDownloads: number;
  totalFollowers: number;
  totalFollowing: number;
  isProfilePublic: boolean;
  showEmail: boolean;
  showLastLogin: boolean;
  showStatistics: boolean;
  websiteUrl?: string;
  twitterUrl?: string;
  instagramUrl?: string;
  youtubeUrl?: string;
  message?: string; // Added for private profile message
}

interface UserModel {
  id: string;
  name: string;
  description: string;
  thumbnailUrl?: string;
  downloads: number;
  likes: number;
  createdAt: string;
  updatedAt: string;
  isPublic: boolean;
  isFeatured: boolean;
  license?: string;
  aIGenerated: boolean;
  wIP: boolean;
  nSFW: boolean;
}

interface UserCollection {
  id: string;
  name: string;
  description?: string;
  avatar?: string;
  visibility: 'Public' | 'Private' | 'Unlisted';
  createdAt: string;
  updatedAt: string;
  ownerId: string;
  owner?: {
    id: string;
    username: string;
  };
  collectionModels?: Array<{
    collectionId: string;
    modelId: string;
    model?: {
      id: string;
      name: string;
      description?: string;
      thumbnailUrl?: string;
    };
    addedAt: string;
  }>;
}

interface UserPrinter {
  id: string;
  name: string;
  manufacturer: string;
  model: string;
  type: string;
  description?: string;
  createdAt: string;
  updatedAt: string;
  isDefault: boolean;
  isActive: boolean;
}

interface UserFilament {
  id: string;
  name: string;
  manufacturer: string;
  type: string;
  color: string;
  diameter: string;
  createdAt: string;
  updatedAt: string;
  isActive: boolean;
}

const UserProfile: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user: currentUser } = useAuth();
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [activeTab, setActiveTab] = useState('models');
  

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  // Data states
  const [models, setModels] = useState<UserModel[]>([]);
  const [collections, setCollections] = useState<UserCollection[]>([]);
  
  // Pagination and search states
  const [modelsPage, setModelsPage] = useState(1);
  const [collectionsPage, setCollectionsPage] = useState(1);
  const [modelsSearch, setModelsSearch] = useState('');
  const [collectionsSearch, setCollectionsSearch] = useState('');
  const [modelsTotalPages, setModelsTotalPages] = useState(1);
  const [collectionsTotalPages, setCollectionsTotalPages] = useState(1);
  const [modelsTotalCount, setModelsTotalCount] = useState(0);
  const [collectionsTotalCount, setCollectionsTotalCount] = useState(0);
  const [modelsLoading, setModelsLoading] = useState(false);
  const [collectionsLoading, setCollectionsLoading] = useState(false);
  

  const [comments, setComments] = useState<any[]>([]);

  // Fetch functions
  const fetchUserModels = async (userId: string, page: number = 1, search: string = '') => {
    try {
      setModelsLoading(true);
      const response = await api.get(`/models/user/${userId}/public`, {
        params: {
          page,
          pageSize: 12,
          searchQuery: search || undefined
        }
      });
      if (response.data) {
        // Handle both cases: wrapped in models property or direct array
        const modelsData = response.data.models || response.data;
        if (Array.isArray(modelsData)) {
          setModels(modelsData);
          setModelsTotalCount(response.data.totalCount || modelsData.length);
          setModelsTotalPages(response.data.totalPages || 1);
        } else {
          setModels([]);
          setModelsTotalCount(0);
          setModelsTotalPages(1);
        }
      }
    } catch (error) {
      console.error('Error fetching user models:', error);
    } finally {
      setModelsLoading(false);
    }
  };

  const fetchUserCollections = async (userId: string, page: number = 1, search: string = '') => {
    try {
      setCollectionsLoading(true);
      const response = await api.get(`/collections/user/${userId}`, {
        params: {
          page,
          pageSize: 12,
          searchQuery: search || undefined
        }
      });
      if (response.data) {
        const collectionsData = response.data.collections || response.data;
        if (Array.isArray(collectionsData)) {
          setCollections(collectionsData);
          setCollectionsTotalCount(response.data.totalCount || collectionsData.length);
          setCollectionsTotalPages(response.data.totalPages || 1);
        } else {
          setCollections([]);
          setCollectionsTotalCount(0);
          setCollectionsTotalPages(1);
        }
      }
    } catch (error) {
      console.error('Error fetching user collections:', error);
    } finally {
      setCollectionsLoading(false);
    }
  };





  // const fetchLikedModels = async (userId: string) => {
  //   try {
  //     const response = await api.get(`/users/${userId}/liked-models`);
  //     if (response.data) {
  //       setLikedModels(response.data.models || []);
  //     }
  //   } catch (error) {
  //     console.error('Error fetching liked models:', error);
  //   }
  // };

  useEffect(() => {
    const fetchProfile = async () => {
      if (!id) return;
      
      setLoading(true);
      setError(null);
      
      try {
        const response = await api.get(`/users/profile/${id}`);
        
        if (response?.data) {
          // Check if this is a private profile response
          if (response.data.message === "This profile is private") {
            setProfile({
              id: response.data.id,
              username: response.data.username,
              isProfilePublic: false,
              message: response.data.message
            } as any);
            setLoading(false);
            return;
          }
          
          setProfile(response.data);
          
          // Fetch additional data only if profile is public
          if (response.data.isProfilePublic) {
            await Promise.all([
              fetchUserModels(response.data.id),
              fetchUserCollections(response.data.id)
            ]);
          }
        }
      } catch (error: any) {
        console.error('Error fetching profile:', error);
        if (error.response?.status === 404) {
          setError('User profile not found');
        } else {
          setError('Failed to load user profile');
        }
      } finally {
        setLoading(false);
      }
    };

    fetchProfile();
  }, [id]);



  const loadTabData = async (tab: string) => {
    try {
      switch (tab) {
        case 'models':
          if (profile && models.length === 0) {
            await fetchUserModels(profile.id);
          }
          break;
        case 'collections':
          if (profile && collections.length === 0) {
            await fetchUserCollections(profile.id);
          }
          break;
        // case 'printers':
        //   if (profile && printers.length === 0) {
        //     await fetchUserPrinters(profile.id);
        //   }
        //   break;
        // case 'filaments':
        //   if (profile && filaments.length === 0) {
        //     await fetchUserFilaments(profile.id);
        //   }
        //   break;
        // case 'liked':
        //   if (profile && likedModels.length === 0) {
        //     await fetchLikedModels(profile.id);
        //   }
        //   break;
        case 'comments':
          if (profile && comments.length === 0) {
            // await fetchUserComments(profile.id);
          }
          break;
      }
    } catch (err) {
      console.error(`Error loading ${tab} data:`, err);
    }
  };

  const handleTabChange = async (tab: string) => {
    setActiveTab(tab);
    // Reset pagination and search when switching tabs
    if (tab === 'models') {
      setModelsPage(1);
      setModelsSearch('');
    } else if (tab === 'collections') {
      setCollectionsPage(1);
      setCollectionsSearch('');
    }
    await loadTabData(tab);
  };

  // Search and pagination handlers
  const handleModelsSearch = async (search: string) => {
    setModelsSearch(search);
    setModelsPage(1);
    // Debounce the search to avoid too many API calls
    const timeoutId = setTimeout(() => {
      if (profile) {
        fetchUserModels(profile.id, 1, search);
      }
    }, 300);
    return () => clearTimeout(timeoutId);
  };

  const handleCollectionsSearch = async (search: string) => {
    setCollectionsSearch(search);
    setCollectionsPage(1);
    // Debounce the search to avoid too many API calls
    const timeoutId = setTimeout(() => {
      if (profile) {
        fetchUserCollections(profile.id, 1, search);
      }
    }, 300);
    return () => clearTimeout(timeoutId);
  };

  const handleModelsPageChange = async (page: number) => {
    setModelsPage(page);
    if (profile) {
      await fetchUserModels(profile.id, page, modelsSearch);
    }
  };

  const handleCollectionsPageChange = async (page: number) => {
    setCollectionsPage(page);
    if (profile) {
      await fetchUserCollections(profile.id, page, collectionsSearch);
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString();
  };

  const formatNumber = (num: number) => {
    if (num >= 1000000) return (num / 1000000).toFixed(1) + 'M';
    if (num >= 1000) return (num / 1000).toFixed(1) + 'K';
    return num.toString();
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center min-h-screen">
        <CircularProgress />
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex justify-center items-center min-h-screen">
        <div className="text-center">
          <ErrorOutline className="text-red-500 text-6xl mb-4" />
          <Typography variant="h5" color="error" gutterBottom>
            {error}
          </Typography>
        </div>
      </div>
    );
  }

  if (!profile) {
    return (
      <div className="flex justify-center items-center min-h-screen">
        <Typography variant="h5">Profile not found</Typography>
      </div>
    );
  }

  // Check if profile is private
  if (!profile.isProfilePublic) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="max-w-2xl mx-auto text-center">
          <Lock className="text-gray-400 text-6xl mb-4 mx-auto" />
          <Typography variant="h4" gutterBottom>
            Profile is Private
          </Typography>
          <Typography variant="body1" color="textSecondary" paragraph>
            {profile.message || "This user has chosen to keep their profile private."}
          </Typography>
          <Typography variant="body2" color="textSecondary">
            You can only view profiles that are set to public.
          </Typography>
        </div>
      </div>
    );
  }

  const isOwnProfile = currentUser?.username === profile.username;

  return (
    <div className="lg-container min-h-screen flex flex-col">
      <NavigationBar
        title={`${profile.firstName && profile.lastName ? `${profile.firstName} ${profile.lastName}` : profile.username}'s Profile`}
        icon={<Person className="w-6 h-6" />}
        showSearch={true}
        showUploadButton={isOwnProfile}
        showHomeLink={true}
      />

      {/* Main Content - Padding for fixed navbar */}
      <div className="flex-1 pt-20">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
        {/* Profile Header */}
        <div className="lg-card p-4 mb-4">
          <div className="flex flex-col sm:flex-row gap-4">
            {/* Avatar */}
            <div className="flex-shrink-0">
              <UserAvatar
                userId={profile.id}
                username={profile.username}
                avatar={profile.avatar}
                profilePictureUrl={profile.profilePictureUrl}
                size="lg"
                className="w-20 h-20 mx-auto sm:mx-0"
              />
            </div>

            {/* Profile Details */}
            <div className="flex-1 min-w-0">
              <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-2 mb-2">
                <div>
                  <h1 className="text-xl font-bold text-white">
                    {profile.firstName && profile.lastName ? `${profile.firstName} ${profile.lastName}` : profile.username}
                  </h1>
                  {profile.firstName && profile.lastName && (
                    <p className="text-sm text-white/70">@{profile.username}</p>
                  )}
                </div>
                {isOwnProfile && (
                  <button
                    onClick={() => navigate('/settings')}
                    className="lg-button lg-button-secondary text-sm py-1 px-3"
                  >
                    <Settings className="w-4 h-4 mr-1" />
                    Edit Profile
                  </button>
                )}
              </div>

              {profile.bio && (
                <p className="text-sm text-white/90 mb-3 line-clamp-2">{profile.bio}</p>
              )}

              {/* Profile Stats - Inline */}
              {profile.showStatistics && (
                <div className="flex flex-wrap items-center gap-x-4 gap-y-1 mb-3 text-sm">
                  <div className="flex items-center gap-1">
                    <span className="font-semibold text-white">{formatNumber(profile.totalModels)}</span>
                    <span className="text-white/60">Models</span>
                  </div>
                  <div className="flex items-center gap-1">
                    <span className="font-semibold text-white">{formatNumber(profile.totalCollections)}</span>
                    <span className="text-white/60">Collections</span>
                  </div>
                  <div className="flex items-center gap-1">
                    <span className="font-semibold text-white">{formatNumber(profile.totalLikes)}</span>
                    <span className="text-white/60">Likes</span>
                  </div>
                  <div className="flex items-center gap-1">
                    <span className="font-semibold text-white">{formatNumber(profile.totalDownloads)}</span>
                    <span className="text-white/60">Downloads</span>
                  </div>
                </div>
              )}

              {/* Additional Info and Social Links - Inline */}
              <div className="flex flex-wrap items-center gap-x-4 gap-y-1 text-xs text-white/60">
                {profile.country && (
                  <div className="flex items-center gap-1">
                    <LocationOn className="w-3 h-3" />
                    {profile.country}
                  </div>
                )}
                <div className="flex items-center gap-1">
                  <CalendarToday className="w-3 h-3" />
                  Member since {formatDate(profile.createdAt)}
                </div>
                {profile.roleName && (
                  <div className="flex items-center gap-1">
                    <Person className="w-3 h-3" />
                    {profile.roleName}
                  </div>
                )}
                {(profile.websiteUrl || profile.twitterUrl || profile.instagramUrl || profile.youtubeUrl) && (
                  <div className="flex items-center gap-2 ml-auto">
                    {profile.websiteUrl && (
                      <a
                        href={profile.websiteUrl}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="text-blue-400 hover:text-blue-300 transition-colors"
                      >
                        <Link className="w-4 h-4" />
                      </a>
                    )}
                    {profile.twitterUrl && (
                      <a
                        href={profile.twitterUrl}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="text-blue-400 hover:text-blue-300 transition-colors"
                      >
                        <Language className="w-4 h-4" />
                      </a>
                    )}
                    {profile.instagramUrl && (
                      <a
                        href={profile.instagramUrl}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="text-blue-400 hover:text-blue-300 transition-colors"
                      >
                        <Language className="w-4 h-4" />
                      </a>
                    )}
                    {profile.youtubeUrl && (
                      <a
                        href={profile.youtubeUrl}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="text-blue-400 hover:text-blue-300 transition-colors"
                      >
                        <Language className="w-4 h-4" />
                      </a>
                    )}
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>

        {/* Navigation Tabs */}
        <div className="lg-tabs mb-4">
          <nav className="flex space-x-4 overflow-x-auto">
              <button
                onClick={() => handleTabChange('models')}
                className={`lg-tab-button ${
                  activeTab === 'models' ? 'active' : ''
                }`}
              >
                <Person className="w-5 h-5" />
                Models ({profile.totalModels})
              </button>
              <button
                onClick={() => handleTabChange('collections')}
                className={`lg-tab-button ${
                  activeTab === 'collections' ? 'active' : ''
                }`}
              >
                <Collections className="w-5 h-5" />
                Collections ({profile.totalCollections})
              </button>
              {/* <button
                onClick={() => handleTabChange('printers')}
                className={`lg-tab-button ${
                  activeTab === 'printers' ? 'active' : ''
                }`}
              >
                <Print className="w-5 h-5" />
                Printers & Filaments
              </button> */}
              {/* <button
                onClick={() => handleTabChange('liked')}
                className={`lg-tab-button ${
                  activeTab === 'liked' ? 'active' : ''
                }`}
              >
                <ThumbUp className="w-5 h-5" />
                Liked Models
              </button>
              <button
                onClick={() => handleTabChange('comments')}
                className={`lg-tab-button ${
                  activeTab === 'comments' ? 'active' : ''
                }`}
              >
                <Comment className="w-5 h-5" />
                Comments
              </button> */}
            </nav>
        </div>

        {/* Tab Content */}
        <div className="min-h-96">
          {activeTab === 'models' && (
            <div>
              {/* Search Bar and Layout Controls - Combined Row */}
              <div className="flex flex-col sm:flex-row gap-3 mb-4">
                <div className="flex-1">
                  <TextField
                    fullWidth
                    variant="outlined"
                    placeholder="Search models..."
                    value={modelsSearch}
                    onChange={(e) => handleModelsSearch(e.target.value)}
                    size="small"
                    InputProps={{
                      startAdornment: (
                        <InputAdornment position="start">
                          <SearchIcon className="text-white/60" style={{ fontSize: '18px' }} />
                        </InputAdornment>
                      ),
                      endAdornment: modelsSearch && (
                        <InputAdornment position="end">
                          <IconButton
                            size="small"
                            onClick={() => handleModelsSearch('')}
                            className="text-white/60 hover:text-white"
                          >
                            <ClearIcon style={{ fontSize: '18px' }} />
                          </IconButton>
                        </InputAdornment>
                      ),
                      className: "text-white",
                      sx: {
                        '& .MuiOutlinedInput-root': {
                          '& fieldset': {
                            borderColor: 'rgba(255, 255, 255, 0.3)',
                          },
                          '&:hover fieldset': {
                            borderColor: 'rgba(255, 255, 255, 0.5)',
                          },
                          '&.Mui-focused fieldset': {
                            borderColor: 'rgba(255, 255, 255, 0.8)',
                          },
                        },
                        '& .MuiInputBase-input': {
                          color: 'white',
                          padding: '8px 14px',
                        },
                      }
                    }}
                  />
                </div>
                <div className="flex-shrink-0">
                  <LayoutControls compact={true} />
                </div>
              </div>

              {/* Models Grid */}
              {modelsLoading ? (
                <div className="flex justify-center items-center py-8">
                  <CircularProgress />
                </div>
              ) : models.length > 0 ? (
                <>
                  <ModelGrid
                    models={models.map(model => ({
                      id: model.id,
                      name: model.name,
                      description: model.description,
                      thumbnailUrl: model.thumbnailUrl,
                      author: { username: profile.username },
                      comments: [],
                      // Add other required properties
                    } as any))}
                    onModelClick={(model) => navigate(`/models/${model.id}`)}
                  />
                  
                  {/* Pagination */}
                  {modelsTotalPages > 1 && (
                    <div className="flex justify-center items-center mt-6 space-x-2">
                      <IconButton
                        onClick={() => handleModelsPageChange(modelsPage - 1)}
                        disabled={modelsPage <= 1}
                        className="text-white/60 hover:text-white disabled:text-white/30"
                        size="small"
                      >
                        <NavigateBeforeIcon />
                      </IconButton>
                      
                      <span className="text-white/60 text-sm">
                        Page {modelsPage} of {modelsTotalPages} ({modelsTotalCount} total)
                      </span>
                      
                      <IconButton
                        onClick={() => handleModelsPageChange(modelsPage + 1)}
                        disabled={modelsPage >= modelsTotalPages}
                        className="text-white/60 hover:text-white disabled:text-white/30"
                        size="small"
                      >
                        <NavigateNextIcon />
                      </IconButton>
                    </div>
                  )}
                </>
              ) : (
                <div className="lg-card p-8 text-center">
                  <p className="text-white/60">
                    {modelsSearch ? `No models found matching "${modelsSearch}"` : 'No models found'}
                  </p>
                </div>
              )}
            </div>
          )}

          {activeTab === 'collections' && (
            <div>
              {/* Search Bar */}
              <div className="mb-4">
                <TextField
                  fullWidth
                  variant="outlined"
                  placeholder="Search collections..."
                  value={collectionsSearch}
                  onChange={(e) => handleCollectionsSearch(e.target.value)}
                  size="small"
                  InputProps={{
                    startAdornment: (
                      <InputAdornment position="start">
                        <SearchIcon className="text-white/60" style={{ fontSize: '18px' }} />
                      </InputAdornment>
                    ),
                    endAdornment: collectionsSearch && (
                      <InputAdornment position="end">
                        <IconButton
                          size="small"
                          onClick={() => handleCollectionsSearch('')}
                          className="text-white/60 hover:text-white"
                        >
                          <ClearIcon style={{ fontSize: '18px' }} />
                        </IconButton>
                      </InputAdornment>
                    ),
                    className: "text-white",
                    sx: {
                      '& .MuiOutlinedInput-root': {
                        '& fieldset': {
                          borderColor: 'rgba(255, 255, 255, 0.3)',
                        },
                        '&:hover fieldset': {
                          borderColor: 'rgba(255, 255, 255, 0.5)',
                        },
                        '&.Mui-focused fieldset': {
                          borderColor: 'rgba(255, 255, 255, 0.8)',
                        },
                      },
                      '& .MuiInputBase-input': {
                        color: 'white',
                        padding: '8px 14px',
                      },
                    }
                  }}
                />
              </div>

              {/* Collections Grid */}
              {collectionsLoading ? (
                <div className="flex justify-center items-center py-8">
                  <CircularProgress />
                </div>
              ) : collections.length > 0 ? (
                <>
                  <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
                    {collections.map((collection) => (
                      <CollectionCard
                        key={collection.id}
                        collection={collection as any}
                        showOwner={false}
                      />
                    ))}
                  </div>
                  
                  {/* Pagination */}
                  {collectionsTotalPages > 1 && (
                    <div className="flex justify-center items-center mt-6 space-x-2">
                      <IconButton
                        onClick={() => handleCollectionsPageChange(collectionsPage - 1)}
                        disabled={collectionsPage <= 1}
                        className="text-white/60 hover:text-white disabled:text-white/30"
                        size="small"
                      >
                        <NavigateBeforeIcon />
                      </IconButton>
                      
                      <span className="text-white/60 text-sm">
                        Page {collectionsPage} of {collectionsTotalPages} ({collectionsTotalCount} total)
                      </span>
                      
                      <IconButton
                        onClick={() => handleCollectionsPageChange(collectionsPage + 1)}
                        disabled={collectionsPage >= collectionsTotalPages}
                        className="text-white/60 hover:text-white disabled:text-white/30"
                        size="small"
                      >
                        <NavigateNextIcon />
                      </IconButton>
                    </div>
                  )}
                </>
              ) : (
                <div className="lg-card p-8 text-center">
                  <p className="text-white/60">
                    {collectionsSearch ? `No collections found matching "${collectionsSearch}"` : 'No collections found'}
                  </p>
                </div>
              )}
            </div>
          )}

          {activeTab === 'printers' && (
            <div>
              {/* Printers */}
              <div className="mb-6">
                <h3 className="text-lg font-semibold text-white mb-3">Printers</h3>
                {/* {printers.length > 0 ? (
                  <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
                    {printers.map((printer) => (
                      <div key={printer.id} className="lg-card p-3">
                        <h4 className="font-semibold text-white text-sm mb-1">{printer.name}</h4>
                        <p className="text-white/80 text-xs mb-1">{printer.manufacturer} {printer.model}</p>
                        <p className="text-white/60 text-xs">{printer.type}</p>
                        {printer.isDefault && (
                          <span className="inline-block bg-blue-500 text-white text-xs px-2 py-1 rounded mt-2">
                            Default
                          </span>
                        )}
                      </div>
                    ))}
                  </div>
                ) : (
                  <p className="text-white/60 text-sm">No printers found</p>
                )} */}
              </div>

              {/* Filaments */}
              <div>
                <h3 className="text-lg font-semibold text-white mb-3">Filaments</h3>
                {/* {filaments.length > 0 ? (
                  <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
                    {filaments.map((filament) => (
                      <div key={filament.id} className="lg-card p-3">
                        <h4 className="font-semibold text-white text-sm mb-1">{filament.name}</h4>
                        <p className="text-white/80 text-xs mb-1">{filament.manufacturer} {filament.type}</p>
                        <p className="text-white/60 text-xs">{filament.color} - {filament.diameter}mm</p>
                      </div>
                    ))}
                  </div>
                ) : (
                  <p className="text-white/60 text-sm">No filaments found</p>
                )} */}
              </div>
            </div>
          )}

          {activeTab === 'liked' && (
            <div>
              {/* {likedModels.length > 0 ? (
                <ModelGrid
                  models={likedModels.map(model => ({
                    id: model.id,
                    name: model.name,
                    description: model.description,
                    thumbnailUrl: model.thumbnailUrl,
                    author: { username: profile.username },
                    comments: [],
                    // Add other required properties
                  } as any))}
                  onModelClick={(model) => navigate(`/models/${model.id}`)}
                />
              ) : (
                <div className="lg-card p-8 text-center">
                  <p className="text-white/60">No liked models found</p>
                </div>
              )} */}
            </div>
          )}

          {activeTab === 'comments' && (
            <div>
              {comments.length > 0 ? (
                <div className="space-y-3">
                  {comments.map((comment) => (
                    <div key={comment.id} className="lg-card p-3">
                      <div className="flex items-start gap-3">
                        <UserAvatar
                          userId={comment.user.id}
                          username={comment.user.username}
                          avatar={comment.user.avatar}
                          profilePictureUrl={comment.user.profilePictureUrl}
                          size="sm"
                        />
                        <div className="flex-1">
                          <div className="flex items-center gap-2 mb-1">
                            <span className="font-semibold text-white text-sm">{comment.user.username}</span>
                            <span className="text-white/40">•</span>
                            <span className="text-white/60 text-xs">{formatDate(comment.createdAt)}</span>
                            {comment.isEdited && (
                              <span className="text-white/40 text-xs">(edited)</span>
                            )}
                          </div>
                          <p className="text-white/90 text-sm mb-1">{comment.content}</p>
                          {comment.model && (
                            <div className="flex items-center gap-2 text-xs">
                              <span className="text-white/60">on</span>
                              <button
                                onClick={() => navigate(`/models/${comment.model.id}`)}
                                className="text-blue-400 hover:text-blue-300 transition-colors"
                              >
                                {comment.model.name}
                              </button>
                            </div>
                          )}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <div className="lg-card p-8 text-center">
                  <p className="text-white/60">No comments found</p>
                </div>
              )}
            </div>
          )}
        </div>
      </div>
      </div>
    </div>
  );
};

export default UserProfile;
