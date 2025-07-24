import React from 'react';
import Avatar from './Avatar';

interface UserAvatarProps {
  userId: string;
  username: string;
  avatar?: string;
  profilePictureUrl?: string;
  size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl' | '2xl';
  className?: string;
}

const UserAvatar: React.FC<UserAvatarProps> = ({ 
  userId,
  username, 
  avatar,
  profilePictureUrl, 
  size = 'md',
  className = '' 
}) => {
  // If user has a profile picture, show it (this takes precedence over avatar)
  if (profilePictureUrl) {
    return (
      <img
        src={profilePictureUrl}
        alt={`${username}'s profile picture`}
        className={`w-${size === 'xs' ? '6' : size === 'sm' ? '8' : size === 'md' ? '10' : size === 'lg' ? '12' : size === 'xl' ? '16' : '20'} h-${size === 'xs' ? '6' : size === 'sm' ? '8' : size === 'md' ? '10' : size === 'lg' ? '12' : size === 'xl' ? '16' : '20'} rounded-full object-cover ${className}`}
      />
    );
  }

  // Use the Avatar component with user ID and stored avatar
  return (
    <Avatar
      id={userId}
      avatarSvg={avatar}
      size={size}
      className={className}
      title={`${username}'s avatar`}
    />
  );
};

export default UserAvatar; 