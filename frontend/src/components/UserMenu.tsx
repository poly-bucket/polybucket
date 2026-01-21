import React, { useState, useRef, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../store';
import { logoutUser } from '../store/thunks/authThunks';
import { clearUser } from '../store/slices/authSlice';
import { useSimplePermissions } from '../hooks/useSimplePermissions';
import { isTokenExpired } from '../utils/jwtUtils';
import UserAvatar from '../ucp/UserAvatar';

interface UserMenuProps {
  isOpen: boolean;
  onClose: () => void;
  anchorEl: HTMLElement | null;
}

// User Info Header Component
const UserInfoHeader: React.FC = () => {
  const { user } = useAppSelector((state) => state.auth);
  const { isAdmin, isModerator } = useSimplePermissions();

  return (
    <div className="px-4 py-3 border-b border-gray-100">
      <div className="flex items-center">
        <UserAvatar 
          userId={user?.id || ''}
          username={user?.username || ''} 
          avatar={user?.avatar}
          profilePictureUrl={user?.profilePictureUrl}
          size="md"
          className="mr-3"
        />
        <div className="flex-1 min-w-0">
          <p className="text-sm font-medium text-gray-900 truncate">
            {user?.username}
          </p>
          <p className="text-xs text-gray-500 truncate">
            {user?.email}
          </p>
          <div className="flex items-center mt-1">
            {isAdmin() && (
              <span className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-red-100 text-red-800">
                Admin
              </span>
            )}
            {isModerator() && !isAdmin() && (
              <span className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-yellow-100 text-yellow-800">
                Moderator
              </span>
            )}
            {!isModerator() && (
              <span className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-blue-100 text-blue-800">
                User
              </span>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

// Menu Item Component
interface MenuItemProps {
  icon: React.ReactNode;
  label: string;
  onClick: () => void;
  variant?: 'default' | 'danger';
}

const MenuItem: React.FC<MenuItemProps> = ({ icon, label, onClick, variant = 'default' }) => {
  const baseClasses = "w-full text-left px-4 py-2 text-sm flex items-center transition-colors duration-150";
  const variantClasses = variant === 'danger' 
    ? "text-red-600 hover:bg-red-50" 
    : "text-gray-700 hover:bg-gray-50";

  return (
    <button
      onClick={onClick}
      className={`${baseClasses} ${variantClasses}`}
    >
      <span className={`w-4 h-4 mr-3 ${variant === 'danger' ? 'text-red-400' : 'text-gray-400'}`}>
        {icon}
      </span>
      {label}
    </button>
  );
};

// User Settings Menu Items Component
const UserSettingsMenuItems: React.FC<{ onNavigate: (path: string) => void }> = ({ onNavigate }) => {
  const { user } = useAppSelector((state) => state.auth);
  const { isAdmin, isModerator } = useSimplePermissions();

  return (
    <>
      {/* My Profile */}
      <MenuItem
        icon={
          <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
          </svg>
        }
        label="My Profile"
        onClick={() => onNavigate(`/profile/${user?.id}`)}
      />

      {/* User Settings */}
      <MenuItem
        icon={
          <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
            </svg>
          }
        label="User Control Panel"
        onClick={() => onNavigate('/settings')}
      />

      {/* My Collections */}
      <MenuItem
        icon={
          <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
          </svg>
        }
        label="My Collections"
        onClick={() => onNavigate('/my-collections')}
      />

      {/* My Models */}
      <MenuItem
        icon={
          <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
          </svg>
        }
        label="My Models"
        onClick={() => onNavigate('/my-models')}
      />

      {/* Moderation Dashboard - Only for moderators and admins */}
      {isModerator() && (
        <MenuItem
          icon={
            <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          }
          label="Moderation Control Panel"
          onClick={() => onNavigate('/moderation-dashboard')}
        />
      )}

      {/* Admin Settings - Only for admins */}
      {isAdmin() && (
        <MenuItem
          icon={
            <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
            </svg>
          }
          label="Admin Control Panel"
          onClick={() => onNavigate('/admin')}
        />
      )}
    </>
  );
};

// Logout Menu Item Component
const LogoutMenuItem: React.FC<{ onLogout: () => void }> = ({ onLogout }) => (
  <MenuItem
    icon={
      <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
      </svg>
    }
    label="Logout"
    onClick={onLogout}
    variant="danger"
  />
);

// Main UserMenu Component
const UserMenu: React.FC<UserMenuProps> = ({ isOpen, onClose, anchorEl }) => {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { user } = useAppSelector((state) => state.auth);
  const menuRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(event.target as Node) && !anchorEl?.contains(event.target as Node)) {
        onClose();
      }
    };

    if (isOpen) {
      document.addEventListener('mousedown', handleClickOutside);
    }

    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [isOpen, onClose, anchorEl]);

  const handleLogout = async () => {
    onClose();
    
    const tokenExpired = user?.accessToken ? isTokenExpired(user.accessToken) : true;
    
    if (tokenExpired) {
      dispatch(clearUser());
      localStorage.removeItem('user');
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      navigate('/login');
    } else {
      try {
        await dispatch(logoutUser());
      } catch (error) {
        console.warn('Logout API call failed, performing local logout:', error);
      } finally {
        dispatch(clearUser());
        localStorage.removeItem('user');
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        navigate('/login');
      }
    }
  };

  const handleNavigate = (path: string) => {
    navigate(path);
    onClose();
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50">
      {/* Backdrop */}
      <div 
        className="absolute inset-0 bg-black" 
        style={{ opacity: 0.59 }}
        onClick={onClose} 
      />
      
      {/* Menu Container */}
      <div 
        ref={menuRef}
        className="absolute right-0 mt-2 w-64 bg-white rounded-lg shadow-lg border border-gray-200 py-2 z-50"
        style={{
          top: anchorEl ? anchorEl.getBoundingClientRect().bottom + 8 : 0,
          right: anchorEl ? window.innerWidth - anchorEl.getBoundingClientRect().right : 16
        }}
      >
        {/* User Info Header */}
        <UserInfoHeader />

        {/* Menu Items */}
        <div className="py-1">
          <UserSettingsMenuItems onNavigate={handleNavigate} />
          
          {/* Page Break Divider */}
          <div className="border-t border-gray-200 my-2 mx-4" />
          
          {/* Logout */}
          <LogoutMenuItem onLogout={handleLogout} />
        </div>
      </div>
    </div>
  );
};

export default UserMenu; 