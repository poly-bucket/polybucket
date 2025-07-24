import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { useAppSelector } from '../../utils/hooks';
import UserAvatar from '../UserAvatar';
import UserMenu from '../UserMenu';

interface NavigationBarProps {
  title?: string;
  showSearch?: boolean;
  onSearch?: (query: string) => void;
  searchPlaceholder?: string;
  showUploadButton?: boolean;
  searchQuery?: string;
  isSearching?: boolean;
  showHomeLink?: boolean;
}

const NavigationBar: React.FC<NavigationBarProps> = ({
  title = "PolyBucket",
  showSearch = true,
  onSearch,
  searchPlaceholder = "Search models...",
  showUploadButton = true,
  searchQuery = "",
  isSearching = false,
  showHomeLink = false
}) => {
  const { user } = useAppSelector((state) => state.auth);
  
  // User menu state
  const [isUserMenuOpen, setIsUserMenuOpen] = useState(false);
  const [userMenuAnchorEl, setUserMenuAnchorEl] = useState<HTMLElement | null>(null);

  const handleUserMenuOpen = (event: React.MouseEvent<HTMLElement>) => {
    setUserMenuAnchorEl(event.currentTarget);
    setIsUserMenuOpen(true);
  };

  const handleUserMenuClose = () => {
    setIsUserMenuOpen(false);
    setUserMenuAnchorEl(null);
  };

  const handleSearch = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter' && onSearch) {
      onSearch((e.target as HTMLInputElement).value);
    }
  };

  return (
    <>
      <header className="lg-nav">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center py-4">
            <div className="flex items-center">
              {showHomeLink && (
                <Link
                  to="/dashboard"
                  className="flex items-center text-white/60 hover:text-white mr-4 transition-colors duration-200"
                >
                  <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
                  </svg>
                  Dashboard
                </Link>
              )}
              <h1 className="text-2xl font-bold text-white">{title}</h1>
              {searchQuery && (
                <span className="ml-4 px-3 py-1 bg-indigo-500/20 text-indigo-300 rounded-full text-sm border border-indigo-500/30">
                  Search: "{searchQuery}"
                </span>
              )}
            </div>
            
            {/* Search Bar */}
            {showSearch && (
              <div className="flex-1 max-w-lg mx-8">
                <div className="relative">
                  <input
                    type="text"
                    placeholder={searchPlaceholder}
                    className="lg-input w-full px-4 py-2"
                    onKeyPress={handleSearch}
                    disabled={isSearching}
                  />
                  {isSearching && (
                    <div className="absolute right-3 top-2.5">
                      <div className="lg-spinner h-5 w-5"></div>
                    </div>
                  )}
                </div>
              </div>
            )}

            {/* User Menu */}
            <div className="flex items-center space-x-4">
              {showUploadButton && (
                <Link
                  to="/upload-model"
                  className="lg-button lg-button-primary flex items-center space-x-2"
                >
                  <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                  </svg>
                  <span>Upload Model</span>
                </Link>
              )}
              
              <button
                onClick={handleUserMenuOpen}
                className="flex items-center space-x-2 hover:bg-white/10 rounded-lg p-2 transition-colors duration-200"
              >
                <UserAvatar 
                  userId={user?.id || ''}
                  username={user?.username || ''} 
                  avatar={user?.avatar}
                  profilePictureUrl={user?.profilePictureUrl}
                />
                <svg className="w-4 h-4 text-white/60" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                </svg>
              </button>
            </div>
          </div>
        </div>
      </header>

      {/* User Menu Dropdown */}
      <UserMenu
        isOpen={isUserMenuOpen}
        onClose={handleUserMenuClose}
        anchorEl={userMenuAnchorEl}
      />
    </>
  );
};

export default NavigationBar; 