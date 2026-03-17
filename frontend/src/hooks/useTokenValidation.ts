import { useEffect, useCallback } from 'react';
import { useLocation } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../utils/hooks';
import { refreshUserToken } from '../store/thunks/authThunks';
import { isTokenExpired } from '../utils/jwtUtils';
import { handleAuthFailure } from '../utils/axiosConfig';

const TOKEN_CHECK_INTERVAL = 30000;

export const useTokenValidation = () => {
  const dispatch = useAppDispatch();
  const location = useLocation();
  const { user } = useAppSelector((state) => state.auth);

  const checkTokenValidity = useCallback(async () => {
    if (!user || !user.accessToken) {
      return;
    }

    if (isTokenExpired(user.accessToken)) {
      if (user.refreshToken) {
        const result = await dispatch(refreshUserToken(user.refreshToken));
        if (!refreshUserToken.fulfilled.match(result)) {
          handleAuthFailure();
        }
      } else {
        handleAuthFailure();
      }
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
