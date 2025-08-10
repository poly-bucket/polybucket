import { useAppSelector } from '../utils/hooks';

export const useSimplePermissions = () => {
  const { user } = useAppSelector((state) => state.auth);

  const isAdmin = (): boolean => {
    if (!user || !user.roles || !Array.isArray(user.roles)) {
      return false;
    }
    return user.roles.includes('Admin');
  };

  const isModerator = (): boolean => {
    if (!user || !user.roles || !Array.isArray(user.roles)) {
      return false;
    }
    const hasModeratorRole = user.roles.includes('Moderator');
    const hasAdminRole = isAdmin();
    return hasModeratorRole || hasAdminRole;
  };

  const isUser = (): boolean => {
    if (!user || !user.roles || !Array.isArray(user.roles)) {
      return false;
    }
    return user.roles.includes('User');
  };

  return {
    isAdmin,
    isModerator,
    isUser
  };
}; 