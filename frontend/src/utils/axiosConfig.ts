import axios, { InternalAxiosRequestConfig, AxiosError, AxiosResponse } from 'axios';
import authService from '../services/authService';
import { store } from '../store/store';
import { refreshToken, logout } from '../store/slices/authSlice';

const API_BASE_URL = 'http://localhost:11666/api';

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
    // Get the token from local storage
    const user = authService.getCurrentUser();
    if (user?.accessToken) {
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
    
    // If the error is 401 and we haven't already tried to refresh the token
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      
      try {
        // Try to refresh the token
        await store.dispatch(refreshToken()).unwrap();
        
        // Get the new token
        const user = authService.getCurrentUser();
        if (user && user.accessToken) {
          // Add the new token to the original request
          originalRequest.headers.Authorization = `Bearer ${user.accessToken}`;
          return axios(originalRequest);
        }
      } catch (refreshError) {
        // If refreshing fails, logout user
        console.error('Token refresh failed, logging out:', refreshError);
        store.dispatch(logout());
        return Promise.reject(refreshError);
      }
    }
    
    return Promise.reject(error);
  }
);

export default api; 