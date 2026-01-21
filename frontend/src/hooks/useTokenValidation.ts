import { useEffect, useCallback } from 'react';
import { useLocation } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../utils/hooks';
import { clearUser } from '../store/slices/authSlice';
import { isTokenExpired } from '../utils/jwtUtils';
import { handleAuthFailure } from '../utils/axiosConfig';

const TOKEN_CHECK_INTERVAL = 30000;

export const useTokenValidation = () => {
  const dispatch = useAppDispatch();
  const location = useLocation();
  const { user } = useAppSelector((state) => state.auth);

  const checkTokenValidity = useCallback(() => {
    if (!user || !user.accessToken) {
      return;
    }

    if (isTokenExpired(user.accessToken)) {
      console.log('Token expired, clearing authentication state');
      dispatch(clearUser());
      handleAuthFailure();
    }
  }, [user, dispatch]);

  useEffect(() => {
    checkTokenValidity();

    const intervalId = setInterval(() => {
      checkTokenValidity();
    }, TOKEN_CHECK_INTERVAL);

    return () => {
      clearInterval(intervalId);
    };
  }, [checkTokenValidity]);

  useEffect(() => {
    checkTokenValidity();
  }, [location.pathname, checkTokenValidity]);
};
