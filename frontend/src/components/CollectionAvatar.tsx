import React from 'react';
import Avatar from './Avatar';

interface CollectionAvatarProps {
  collectionId: string;
  collectionName: string;
  avatar?: string;
  size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl' | '2xl';
  className?: string;
}

const CollectionAvatar: React.FC<CollectionAvatarProps> = ({ 
  collectionId,
  collectionName, 
  avatar,
  size = 'md',
  className = '' 
}) => {
  return (
    <Avatar
      id={collectionId}
      avatarSvg={avatar}
      size={size}
      className={className}
      title={`${collectionName}'s avatar`}
    />
  );
};

export default CollectionAvatar; 