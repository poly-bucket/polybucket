import React, { useEffect, useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { RootState } from '../store';
import { setInitialized, setUser, clearUser } from '../store/slices/authSlice';
import { refreshUserToken } from '../store/thunks/authThunks';
import { handleAuthFailure } from '../utils/axiosConfig';
import { extractUserFromJWT, isTokenExpired } from '../utils/jwtUtils';

interface AppInitializerProps {
  children: React.ReactNode;
}

const TOKEN_CHECK_INTERVAL = 30000;

const AppInitializer: React.FC<AppInitializerProps> = ({ children }) => {
  const dispatch = useDispatch();
  const { isInitialized, user } = useSelector((state: RootState) => state.auth);

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
    const initializeApp = async () => {
      try {
        // Check if user data exists in localStorage
        const storedUser = localStorage.getItem('user');
        const accessToken = localStorage.getItem('accessToken');
        const refreshToken = localStorage.getItem('refreshToken');
        
        if (storedUser && accessToken) {
          try {
            const userData = JSON.parse(storedUser);
            
            // Validate the access token
            if (isTokenExpired(accessToken)) {
              console.log('Access token is expired, attempting to refresh...');
              
              // If we have a refresh token, try to refresh
              if (refreshToken) {
                // Import the refresh function dynamically to avoid circular dependencies
                const { refreshUserToken } = await import('../store/thunks/authThunks');
                const result = await dispatch(refreshUserToken(refreshToken));
                
                if (refreshUserToken.fulfilled.match(result)) {
                  console.log('Token refreshed successfully');
                  // The user state will be updated by the thunk
                } else {
                  console.log('Token refresh failed, clearing authentication');
                  handleAuthFailure();
                }
              } else {
                console.log('No refresh token available, clearing authentication');
                handleAuthFailure();
              }
            } else {
              console.log('Valid access token found, restoring user session');
              // Token is valid, restore user state
              dispatch(setUser(userData));
            }
          } catch (error) {
            console.error('Error parsing stored user data:', error);
            handleAuthFailure();
          }
        } else {
          console.log('No stored authentication data found');
          // Clear any stale data
          localStorage.removeItem('user');
          localStorage.removeItem('accessToken');
          localStorage.removeItem('refreshToken');
        }
      } catch (error) {
        console.error('Error during app initialization:', error);
        handleAuthFailure();
      } finally {
        dispatch(setInitialized());
      }
    };

    if (!isInitialized) {
      initializeApp();
    }
  }, [dispatch, isInitialized]);

  useEffect(() => {
    if (!isInitialized || !user) {
      return;
    }

    checkTokenValidity();

    const intervalId = setInterval(() => {
      checkTokenValidity();
    }, TOKEN_CHECK_INTERVAL);

    return () => {
      clearInterval(intervalId);
    };
  }, [isInitialized, user, checkTokenValidity]);

  if (!isInitialized) {
    return <div>Loading...</div>;
  }

  return <>{children}</>;
};

export default AppInitializer; 