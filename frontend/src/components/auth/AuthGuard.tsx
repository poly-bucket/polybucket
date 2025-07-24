import React from 'react';
import { useNavigate, Outlet } from 'react-router-dom';
import { useAppSelector } from '../../utils/hooks';

const AuthGuard: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useAppSelector((state) => state.auth);

  React.useEffect(() => {
    // If not authenticated, go to login
    if (!user) {
      navigate('/login');
    }
  }, [user, navigate]);

  return <Outlet />;
};

export default AuthGuard; 