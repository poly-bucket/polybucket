import { Navigate, useLocation } from 'react-router-dom';
import { useAppSelector } from '../store/hooks';
import { routes } from './routes';

interface ProtectedRouteProps {
  children: React.ReactNode;
}

export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children }) => {
  const { isAuthenticated } = useAppSelector((state) => state.auth);
  const location = useLocation();

  if (!isAuthenticated) {
    return <Navigate to={routes.login} state={{ from: location }} replace />;
  }

  return <>{children}</>;
}; 