import React, { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { RootState } from '../store';
import { setInitialized } from '../store/slices/authSlice';
import { isAuthenticated, handleAuthFailure } from '../utils/axiosConfig';
import { extractUserFromJWT, isTokenExpired } from '../utils/jwtUtils';

interface AppInitializerProps {
  children: React.ReactNode;
}

const AppInitializer: React.FC<AppInitializerProps> = ({ children }) => {
  const dispatch = useDispatch();
  const { isInitialized, user } = useSelector((state: RootState) => state.auth);

  useEffect(() => {
    const initializeApp = async () => {
      try {
        // Check if user is authenticated
        if (isAuthenticated()) {
          console.log('User is authenticated, app initialized');
        } else {
                  // Check if there's stored user data in localStorage
        const storedUser = localStorage.getItem('user');
        const storedAccessToken = localStorage.getItem('accessToken');
        
        if (storedUser && storedAccessToken) {
          try {
            const userData = JSON.parse(storedUser);
            
            // Check if the stored token is expired
            if (isTokenExpired(storedAccessToken)) {
              console.log('Stored access token is expired, clearing...');
              handleAuthFailure();
              return;
            }
            
            // Extract user information from the stored JWT
            const decodedUser = extractUserFromJWT(storedAccessToken);
            console.log('Decoded user from stored JWT:', decodedUser);
            
            if (decodedUser) {
              // Update the stored user data with information from JWT
              const updatedUserData = {
                ...userData,
                id: decodedUser.id,
                username: decodedUser.username,
                email: decodedUser.email,
                roles: [decodedUser.role]
              };
              
              // Update localStorage with the corrected user data
              localStorage.setItem('user', JSON.stringify(updatedUserData));
              console.log('Updated stored user data with JWT information');
            } else {
              console.log('Failed to decode stored JWT, clearing...');
              handleAuthFailure();
            }
          } catch (error) {
            console.log('Failed to parse stored user data, clearing...');
            handleAuthFailure();
          }
        } else {
          console.log('No stored user data or access token found');
        }
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

  if (!isInitialized) {
    return <div>Loading...</div>;
  }

  return <>{children}</>;
};

export default AppInitializer; 