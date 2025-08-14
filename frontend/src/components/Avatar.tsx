import React from 'react';
import { minidenticon } from 'minidenticons';

interface AvatarProps {
  id: string;
  avatarSvg?: string;
  size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl' | '2xl';
  className?: string;
  title?: string;
}

const Avatar: React.FC<AvatarProps> = ({ 
  id, 
  avatarSvg, 
  size = 'md',
  className = '',
  title
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

  const sizeClass = sizeClasses[size];

  // If we have a stored avatar SVG, use it
  if (avatarSvg) {
    const svgDataUrl = `data:image/svg+xml,${encodeURIComponent(avatarSvg)}`;
    
    return (
      <div 
        className={`${sizeClass} rounded-full overflow-hidden bg-white flex items-center justify-center ${className}`}
        title={title}
      >
        <img
          src={svgDataUrl}
          alt="Avatar"
          className="w-full h-full"
          style={{ imageRendering: 'pixelated' }}
        />
      </div>
    );
  }

  // Generate identicon SVG from ID
  const svgString = minidenticon(id, 50, 50); // 50% saturation and lightness
  const svgDataUrl = `data:image/svg+xml,${encodeURIComponent(svgString)}`;

  return (
    <div 
      className={`${sizeClass} rounded-full overflow-hidden bg-white flex items-center justify-center ${className}`}
      title={title}
    >
      <img
        src={svgDataUrl}
        alt="Generated avatar"
        className="w-full h-full"
        style={{ imageRendering: 'pixelated' }}
      />
    </div>
  );
};

export default Avatar; 