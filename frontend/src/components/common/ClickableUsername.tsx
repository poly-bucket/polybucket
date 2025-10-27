import React from 'react';
import { useNavigate } from 'react-router-dom';
import UserAvatar from '../../ucp/UserAvatar';

interface ClickableUsernameProps {
  userId: string;
  username: string;
  avatar?: string;
  profilePictureUrl?: string;
  size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl' | '2xl';
  showAvatar?: boolean;
  className?: string;
  children?: React.ReactNode;
}

const ClickableUsername: React.FC<ClickableUsernameProps> = ({
  userId,
  username,
  avatar,
  profilePictureUrl,
  size = 'sm',
  showAvatar = false,
  className = '',
  children
}) => {
  const navigate = useNavigate();

  const handleClick = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    // Use ID-based route for better security
    if (userId) {
      navigate(`/profile/${userId}`);
    } else {
      // Fallback to username route for backward compatibility (though this shouldn't happen)
      navigate(`/user/${username}`);
    }
  };

  return (
    <button
      onClick={handleClick}
      className={`inline-flex items-center gap-2 hover:text-blue-400 transition-colors duration-200 ${className}`}
      title={`View ${username}'s profile`}
    >
      {showAvatar && (
        <UserAvatar
          userId={userId}
          username={username}
          avatar={avatar}
          profilePictureUrl={profilePictureUrl}
          size={size}
        />
      )}
      <span className="font-medium">{children || username}</span>
    </button>
  );
};

export default ClickableUsername;
