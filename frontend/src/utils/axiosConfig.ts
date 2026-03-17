import axios, { InternalAxiosRequestConfig, AxiosError, AxiosResponse } from 'axios';
import store from '../store';
import { refreshUserToken } from '../store/thunks/authThunks';
import { clearUser } from '../store/slices/authSlice';
import { getApiConfig } from '../api/config';

const API_BASE_URL = getApiConfig().baseUrl;

// Utility function to check if user is authenticated
export const isAuthenticated = (): boolean => {
  const state = store.getState();
  const user = state.auth.user;
  return !!(user && user.accessToken && user.refreshToken);
};

// Utility function to check if a request URL is an authentication endpoint
const isAuthenticationEndpoint = (url: string | undefined): boolean => {
  if (!url) return false;
  
  const authEndpoints = [
    '/auth/login',
    '/auth/register',
    '/auth/refresh',
    '/auth/logout',
    '/auth/forgot-password',
    '/auth/reset-password',
    '/auth/verify-email'
  ];
  
  return authEndpoints.some(endpoint => url.includes(endpoint));
};

// Utility function to handle authentication failure
export const handleAuthFailure = (): void => {
  console.log('Authentication failed, logging out user');
  store.dispatch(clearUser());
  localStorage.removeItem('user');
  localStorage.removeItem('accessToken');
  localStorage.removeItem('refreshToken');
  localStorage.removeItem('persist:auth');
  window.location.href = '/login';
};

// Create an instance of axios
const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json'
  }
});

// Add a request interceptor
api.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    // Get the token from Redux store
    const state = store.getState();
    const user = state.auth.user;
    
    // Only add Authorization header if user is authenticated and has a valid token
    if (user?.accessToken && user.accessToken.trim() !== '') {
      // Add the token to the authorization header
      config.headers = config.headers || {};
      config.headers.Authorization = `Bearer ${user.accessToken}`;
    }
    return config;
  },
  (error: AxiosError) => {
    return Promise.reject(error);
  }
);

// Add a response interceptor
api.interceptors.response.use(
  (response: AxiosResponse) => {
    return response;
  },
  async (error: AxiosError) => {
    if (!error.config) {
      return Promise.reject(error);
    }

    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };
    
    // Handle 403 Forbidden - don't try to refresh token or logout
    if (error.response?.status === 403) {
      console.warn('Permission denied (403 Forbidden)', error.config.url);
      // Just reject the promise without logging out
      return Promise.reject(error);
    }
    
    // Skip token refresh logic for authentication endpoints
    // These endpoints handle their own 401 errors (e.g., invalid credentials)
    if (isAuthenticationEndpoint(error.config.url || '')) {
      console.log('Authentication endpoint detected, skipping token refresh logic');
      return Promise.reject(error);
    }
    
    // If the error is 401 and we haven't already tried to refresh the token
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      
      try {
        // Get current user from store
        const state = store.getState();
        const user = state.auth.user;
        
        if (user?.refreshToken) {
          console.log('Attempting to refresh token...');
          // Try to refresh the token
          const result = await store.dispatch(refreshUserToken(user.refreshToken));
          
          if (refreshUserToken.fulfilled.match(result)) {
            console.log('Token refreshed successfully');
            // Get the updated user with new token
            const updatedState = store.getState();
            const updatedUser = updatedState.auth.user;
            
            if (updatedUser && updatedUser.accessToken) {
              // Add the new token to the original request
              originalRequest.headers.Authorization = `Bearer ${updatedUser.accessToken}`;
              return api(originalRequest);
            }
          } else {
            console.log('Token refresh failed, logging out user');
            handleAuthFailure();
          }
        } else {
          console.log('No refresh token available, logging out user');
          handleAuthFailure();
        }
      } catch (refreshError) {
        // If refreshing fails, logout user
        console.error('Token refresh failed, logging out:', refreshError);
        handleAuthFailure();
        return Promise.reject(refreshError);
      }
    }
    
    return Promise.reject(error);
  }
);

export default api; 