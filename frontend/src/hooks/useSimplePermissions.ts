import { useAppSelector } from '../utils/hooks';

export const useSimplePermissions = () => {
  const { user } = useAppSelector((state) => state.auth);

  // Debug logging
  console.log('useSimplePermissions - User:', user);
  console.log('useSimplePermissions - User roles:', user?.roles);

  const isAdmin = (): boolean => {
    const hasAdminRole = user?.roles?.includes('Admin') || false;
    console.log('useSimplePermissions - isAdmin():', hasAdminRole);
    return hasAdminRole;
  };

  const isModerator = (): boolean => {
    const hasModeratorRole = user?.roles?.includes('Moderator') || false;
    const hasAdminRole = isAdmin();
    const result = hasModeratorRole || hasAdminRole;
    console.log('useSimplePermissions - isModerator():', result);
    return result;
  };

  const isUser = (): boolean => {
    const hasUserRole = user?.roles?.includes('User') || false;
    console.log('useSimplePermissions - isUser():', hasUserRole);
    return hasUserRole;
  };

  return {
    isAdmin,
    isModerator,
    isUser
  };
}; 