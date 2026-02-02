import { createAsyncThunk } from '@reduxjs/toolkit';
import api from '../../utils/axiosConfig';
import { ApiClient, LoginCommand, LoginCommandResponse } from '../../api/client';
import { extractUserFromJWT } from '../../utils/jwtUtils';
import { API_CONFIG } from '../../api/config';
import {  } from '../../api/client';
import { RegisterCommand, RefreshTokenCommand, RegisterCommandResponse, RefreshTokenCommandResponse } from '../../api/client';

export interface LoginRequest {
  email: string;
  password: string;
  twoFactorToken?: string;
  backupCode?: string;
}

export interface AuthResponse {
  id: string;
  username: string;
  email: string;
  accessToken: string;
  refreshToken: string;
  roles: string[];
  avatar?: string;
  requiresPasswordChange?: boolean;
  requiresFirstTimeSetup?: boolean;
  setupStep?: string;
  requiresTwoFactor?: boolean;
  twoFactorToken?: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
  country?: string;
  isAdmin?: boolean;
}

const API_URL = import.meta.env.VITE_API_URL ? `${import.meta.env.VITE_API_URL}/api/auth` : 'http://localhost:11666/api/auth';

const sharedHttpClient = new AxiosHttpClient(API_CONFIG.baseUrl);

export const loginUser = createAsyncThunk(
  'auth/login',
  async (credentials: LoginRequest, { rejectWithValue, dispatch }: { rejectWithValue: (value: string) => any; dispatch: any }) => {
    try {
      const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:11666';
      const apiClient = new ApiClient(baseUrl, api);
      const loginCommand = new LoginCommand({
        emailOrUsername: credentials.email,
        password: credentials.password,
        userAgent: navigator.userAgent
      });
      
      // Ensure the email field is properly set (workaround for potential constructor issue)
      loginCommand.email = credentials.email;
      
      // Add 2FA fields if provided (these will be sent as additional properties)
      if (credentials.twoFactorToken) {
        (loginCommand as any).twoFactorToken = credentials.twoFactorToken;
      }
      if (credentials.backupCode) {
        (loginCommand as any).backupCode = credentials.backupCode;
      }
      
      // Debug: Check what's actually in the LoginCommand object
      console.log('LoginCommand object:', loginCommand);
      console.log('LoginCommand.email:', loginCommand.email);
      console.log('LoginCommand.toJSON():', loginCommand.toJSON());
      
      const response = await apiClient.login_Login(loginCommand);
      
      console.log('Login response received!');
      console.log('Response:', response);
      
      let authData: AuthResponse;
      
      if (response && response.requiresTwoFactor) {
        console.log('2FA required for login');
        
        // Return a special response indicating 2FA is required
        authData = {
          id: '', // No user ID yet since not fully authenticated
          username: credentials.email.split('@')[0],
          email: credentials.email,
          accessToken: '', // No token yet
          refreshToken: '',
          roles: [],
          requiresTwoFactor: true,
          twoFactorToken: response.twoFactorToken || undefined
        };
        
        console.log('Auth data for 2FA requirement:', authData);
        
        return authData;
      } else if (response && response.token) {
        console.log('Processing login response with token');
        
        // Extract user information from JWT token
        const decodedUser = extractUserFromJWT(response.token);
        console.log('Decoded user from JWT:', decodedUser);
        
        if (!decodedUser) {
          console.error('Failed to decode JWT token');
          throw new Error('Invalid authentication token received');
        }
        
        authData = {
          id: decodedUser.id || '',
          username: decodedUser.username || credentials.email.split('@')[0],
          email: decodedUser.email || credentials.email,
          accessToken: response.token,
          refreshToken: response.refreshToken || '',
          roles: decodedUser.role ? [decodedUser.role] : [],
          requiresPasswordChange: response.requiresPasswordChange || false,
          requiresFirstTimeSetup: response.requiresFirstTimeSetup || false,
          setupStep: response.setupStep || undefined,
          requiresTwoFactor: false,
          twoFactorToken: undefined
        };
        
        console.log('Auth data populated from JWT:', authData);
        
        return authData;
      } else {
        throw new Error('Invalid response format: missing token');
      }
    } catch (error: any) {
      console.error('Login error:', error);
      
      // Handle different types of errors
      if (error.status === 401) {
        // Handle ApiException from the API client
        const errorMessage = error.result?.message || error.result?.detail || error.message || 'Invalid email or password';
        console.log('401 error message:', errorMessage);
        return rejectWithValue(errorMessage);
      } else if (error.status === 400) {
        const errorMessage = error.result?.message || error.result?.detail || error.message || 'Invalid request';
        console.log('400 error message:', errorMessage);
        return rejectWithValue(errorMessage);
      } else if (error.status === 500) {
        return rejectWithValue('Server error. Please try again later.');
      } else if (error.response?.status === 401) {
        // Handle axios errors
        const errorMessage = error.response.data?.message || error.response.data?.detail || 'Invalid email or password';
        console.log('401 axios error message:', errorMessage);
        return rejectWithValue(errorMessage);
      } else if (error.response?.status === 400) {
        const errorMessage = error.response.data?.message || error.response.data?.detail || 'Invalid request';
        console.log('400 axios error message:', errorMessage);
        return rejectWithValue(errorMessage);
      } else if (error.response?.status === 500) {
        return rejectWithValue('Server error. Please try again later.');
      } else if (error.message) {
        // Handle other errors with messages
        console.log('Other error message:', error.message);
        return rejectWithValue(error.message);
      } else {
        return rejectWithValue('An unexpected error occurred');
      }
    }
  }
);



export const registerUser = createAsyncThunk(
  'auth/register',
  async (userData: RegisterRequest, { rejectWithValue }: { rejectWithValue: (value: string) => any }) => {
    try {
      const registerClient = new RegisterClient(API_CONFIG.baseUrl, sharedHttpClient);
      const registerCommand = new RegisterCommand({
        email: userData.email,
        username: userData.username,
        password: userData.password,
        confirmPassword: userData.confirmPassword,
        firstName: userData.firstName,
        lastName: userData.lastName,
        country: userData.country
      });

      const response: RegisterCommandResponse = await registerClient.register(registerCommand);
      
      if (response.authentication) {
        const decodedUser = extractUserFromJWT(response.authentication.accessToken || '');
        return {
          id: decodedUser?.id || response.authentication.user?.id || '',
          username: userData.username,
          email: userData.email,
          accessToken: response.authentication.accessToken || '',
          refreshToken: response.authentication.refreshToken || '',
          roles: [],
          requiresEmailVerification: response.requiresEmailVerification || false
        };
      }
      
      throw new Error('Registration failed: Invalid response format');
    } catch (error: any) {
      let errorMessage = 'Registration failed';
      
      if (error.message) {
        if (error.message.includes('Network Error') || error.message.includes('Failed to fetch') || error.message.includes('ERR_NETWORK')) {
          errorMessage = 'Network Error: Unable to connect to the server. Please check your connection and ensure the API is running.';
        } else if (error.message.includes('CORS')) {
          errorMessage = 'CORS Error: Cross-origin request blocked. Please check server configuration.';
        } else {
          errorMessage = error.message;
        }
      }
      
      if (error.result?.message) {
        errorMessage = error.result.message;
      } else if (error.result?.detail) {
        errorMessage = error.result.detail;
      }
      
      console.error('Registration error:', error);
      return rejectWithValue(errorMessage);
    }
  }
);

export const refreshUserToken = createAsyncThunk(
  'auth/refreshToken',
  async (refreshToken: string, { rejectWithValue }: { rejectWithValue: (value: string) => any }) => {
    try {
      const refreshTokenClient = new RefreshTokenClient(API_CONFIG.baseUrl, sharedHttpClient);
      const refreshTokenCommand = new RefreshTokenCommand({
        refreshToken: refreshToken
      });

      const response: RefreshTokenCommandResponse = await refreshTokenClient.refreshToken(refreshTokenCommand);
      
      if (response.authentication) {
        return {
          accessToken: response.authentication.accessToken || '',
          refreshToken: response.authentication.refreshToken || refreshToken
        };
      }
      
      throw new Error('Invalid refresh token response format');
    } catch (error: any) {
      const errorMessage = error.result?.message || error.result?.detail || error.message || 'Failed to refresh token';
      return rejectWithValue(errorMessage);
    }
  }
);

export const logoutUser = createAsyncThunk(
  'auth/logout',
  async (_: void, { rejectWithValue }: { rejectWithValue: (value: string) => any }) => {
    try {
      console.log('=== LOGOUT THUNK CALLED ===');
      
      // SKIPPED: No LogoutClient found in generated API client
      // Using direct fetch until LogoutClient is added to backend OpenAPI spec
      const response = await fetch(`${API_URL}/logout`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
      });
      
      if (!response.ok) {
        console.warn('Logout request failed, but continuing with local logout');
      }
      
      return true;
    } catch (error: any) {
      console.warn('Logout error, but continuing with local logout:', error);
      return true;
    }
  }
); 