import { createAsyncThunk } from '@reduxjs/toolkit';
import api from '../../utils/axiosConfig';
import { ApiClient, LoginCommand, LoginCommandResponse } from '../../api/client';
import { extractUserFromJWT } from '../../utils/jwtUtils';

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

export const loginUser = createAsyncThunk(
  'auth/login',
  async (credentials: LoginRequest, { rejectWithValue, dispatch }: { rejectWithValue: (value: string) => any; dispatch: any }) => {
    try {
      console.log('=== LOGIN THUNK CALLED ===');
      console.log('Sending login credentials:', credentials);
      
      const apiClient = new ApiClient(undefined, api);
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
      const response = await fetch(`${API_URL}/register`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(userData),
      });
      
      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Registration failed');
      }
      
      const authData = await response.json();
      return authData;
    } catch (error: any) {
      return rejectWithValue(error.message || 'Registration failed');
    }
  }
);

export const refreshUserToken = createAsyncThunk(
  'auth/refreshToken',
  async (refreshToken: string, { rejectWithValue }: { rejectWithValue: (value: string) => any }) => {
    try {
      const response = await fetch(`${API_URL}/refresh-token`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ refreshToken }),
      });
      
      if (!response.ok) {
        throw new Error('Failed to refresh token');
      }
      
      const responseData = await response.json();
      
      // Handle the new response format from the backend
      if (responseData.authentication) {
        return {
          accessToken: responseData.authentication.accessToken,
          refreshToken: responseData.authentication.refreshToken
        };
      } else if (responseData.accessToken) {
        return {
          accessToken: responseData.accessToken,
          refreshToken: responseData.refreshToken || refreshToken
        };
      } else {
        throw new Error('Invalid refresh token response format');
      }
    } catch (error: any) {
      return rejectWithValue('Failed to refresh token');
    }
  }
);

export const logoutUser = createAsyncThunk(
  'auth/logout',
  async (_: void, { rejectWithValue }: { rejectWithValue: (value: string) => any }) => {
    try {
      console.log('=== LOGOUT THUNK CALLED ===');
      
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