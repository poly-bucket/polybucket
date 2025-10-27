import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { useAppSelector } from '../../utils/hooks';
import UserAvatar from '../../ucp/UserAvatar';
import UserMenu from '../UserMenu';
import SearchBar, { SearchType } from './SearchBar';

interface NavigationBarProps {
  title?: string;
  icon?: React.ReactNode;
  description?: string;
  showSearch?: boolean;
  onSearch?: (query: string, searchTypes?: SearchType[]) => void;
  onClearSearch?: () => void;
  searchPlaceholder?: string;
  showUploadButton?: boolean;
  searchQuery?: string;
  searchTags?: string[];
  onSearchTagRemove?: (tag: string) => void;
  onSearchTagAdd?: (tag: string) => void;
  isSearching?: boolean;
  showHomeLink?: boolean;
  selectedSearchTypes?: SearchType[];
  onSearchTypeChange?: (searchTypes: SearchType[]) => void;
}

const NavigationBar: React.FC<NavigationBarProps> = ({
  title = "Polybucket",
  icon,
  description,
  showSearch = true,
  onSearch,
  onClearSearch,
  searchPlaceholder = "Search for models...",
  showUploadButton = true,
  searchQuery = "",
  searchTags = [],
  onSearchTagRemove,
  onSearchTagAdd,
  isSearching = false,
  showHomeLink = false,
  selectedSearchTypes = ['models'],
  onSearchTypeChange
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

  return (
    <>
      <header className="lg-nav">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-1">
          <div className="flex justify-between items-center">
            <div className="flex items-center gap-1.5">
              {showHomeLink && (
                <Link
                  to="/dashboard"
                  className="flex items-center text-white/60 hover:text-white transition-colors duration-200"
                >
                  <svg className="w-3.5 h-3.5 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
                  </svg>
                  <span className="text-xs">Dashboard</span>
                </Link>
              )}
              
              {showHomeLink && (
                <div className="w-px h-3 bg-white/20 mx-1"></div>
              )}
              
              <div className="flex items-center gap-2">
                {icon && (
                  <div className="flex items-center justify-center w-5 h-5 text-white/80">
                    {icon}
                  </div>
                )}
                <h1 className="text-lg font-semibold text-white">
                  {title}
                </h1>
              </div>
              
              {description && (
                <span className="text-gray-300 text-xs ml-2 max-w-md truncate">
                  {description}
                </span>
              )}
            </div>
            
            {/* Search Bar */}
            {showSearch && (
              <SearchBar
                onSearch={onSearch}
                onClearSearch={onClearSearch}
                searchPlaceholder={searchPlaceholder}
                searchQuery={searchQuery}
                searchTags={searchTags}
                onSearchTagRemove={onSearchTagRemove}
                onSearchTagAdd={onSearchTagAdd}
                isSearching={isSearching}
                selectedSearchTypes={selectedSearchTypes}
                onSearchTypeChange={onSearchTypeChange}
              />
            )}

            {/* User Menu */}
            <div className="flex items-center space-x-2">
              {showUploadButton && (
                <Link
                  to="/upload-model"
                  className="lg-button lg-button-primary flex items-center space-x-1.5 px-2.5 py-1 text-sm"
                >
                  <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                  </svg>
                  <span>Upload Model</span>
                </Link>
              )}
              
              <button
                onClick={handleUserMenuOpen}
                className="flex items-center space-x-1 hover:bg-white/10 rounded p-1 transition-colors duration-200"
              >
                <UserAvatar 
                  userId={user?.id || ''}
                  username={user?.username || ''} 
                  avatar={user?.avatar}
                  profilePictureUrl={user?.profilePictureUrl}
                />
                <svg className="w-3 h-3 text-white/60" fill="none" stroke="currentColor" viewBox="0 0 24 24">
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