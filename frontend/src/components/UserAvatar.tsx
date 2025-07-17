import React from 'react';
import { minidenticon } from 'minidenticons';

interface UserAvatarProps {
  username: string;
  profilePictureUrl?: string;
  size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl' | '2xl';
  className?: string;
}

const UserAvatar: React.FC<UserAvatarProps> = ({ 
  username, 
  profilePictureUrl, 
  size = 'md',
  className = '' 
}) => {
  // Size mappings for consistent sizing
  const sizeClasses = {
    xs: 'w-6 h-6',      // 24px
    sm: 'w-8 h-8',      // 32px  
    md: 'w-10 h-10',    // 40px
    lg: 'w-12 h-12',    // 48px
    xl: 'w-16 h-16',    // 64px
    '2xl': 'w-20 h-20'  // 80px
  };

  // Size values in pixels for the SVG
  const sizePixels = {
    xs: 24,
    sm: 32,
    md: 40,
    lg: 48,
    xl: 64,
    '2xl': 80
  };

  const sizeClass = sizeClasses[size];
  const pixelSize = sizePixels[size];

  // If user has a profile picture, show it
  if (profilePictureUrl) {
    return (
      <img
        src={profilePictureUrl}
        alt={`${username}'s profile picture`}
        className={`${sizeClass} rounded-full object-cover ${className}`}
      />
    );
  }

  // Generate identicon SVG
  const svgString = minidenticon(username, 50, 50); // 50% saturation and lightness
  const svgDataUrl = `data:image/svg+xml,${encodeURIComponent(svgString)}`;

  return (
    <div 
      className={`${sizeClass} rounded-full overflow-hidden bg-gray-100 flex items-center justify-center ${className}`}
      title={`${username}'s avatar`}
    >
      <img
        src={svgDataUrl}
        alt={`${username}'s generated avatar`}
        className="w-full h-full"
        style={{ imageRendering: 'pixelated' }} // Keeps the pixelated look crisp
      />
    </div>
  );
};

export default UserAvatar; 