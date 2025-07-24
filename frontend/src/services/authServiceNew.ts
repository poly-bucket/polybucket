import { defaultAxiosClient } from '../api/axiosAdapter';
import { LoginCommandClient, LoginCommand } from '../api/client';

// For testing purposes
const MOCK_MODE = false;
const MOCK_ADMIN_USER = {
  id: '1',
  username: 'admin',
  email: 'admin@example.com',
  accessToken: 'fake-jwt-token',
  refreshToken: 'fake-refresh-token',
  roles: ['Admin']
};

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  id: string;
  username: string;
  email: string;
  accessToken: string;
  refreshToken: string;
  roles: string[];
  requiresPasswordChange?: boolean;
  requiresFirstTimeSetup?: boolean;
  setupStep?: string;
}

// Mock implementation for testing
const mockUsers: AuthResponse[] = [];

const login = async (credentials: LoginRequest): Promise<AuthResponse> => {
  console.log('=== LOGIN FUNCTION CALLED ===');
  console.log('MOCK_MODE:', MOCK_MODE);
  
  if (MOCK_MODE) {
    console.log('MOCK: login', credentials);
    return MOCK_ADMIN_USER;
  }
  
  try {
    console.log('Sending login credentials:', credentials);
    
    // Use the existing working client with axios adapter
    const loginClient = new LoginCommandClient(undefined, defaultAxiosClient);
    const loginCommand = new LoginCommand({
      email: credentials.email,
      password: credentials.password,
      userAgent: navigator.userAgent
    });
    const fileResponse = await loginClient.login(loginCommand);
    
    console.log('Login response received!');
    console.log('Login response:', fileResponse);
    
    // Convert FileResponse blob to JSON
    const responseText = await fileResponse.data.text();
    const responseData = JSON.parse(responseText);
    
    console.log('Parsed response data:', responseData);
    
    let authData: AuthResponse;
    
    // The backend returns a simple format: { token: "...", requiresPasswordChange: boolean, etc. }
    if (responseData && typeof responseData === 'object') {
      if (responseData.token) {
        console.log('Processing login response with token');
        authData = {
          id: '', // Will be extracted from /api/auth/me endpoint
          username: credentials.email.split('@')[0], // Temporary username
          email: credentials.email,
          accessToken: responseData.token,
          refreshToken: '', // No refresh token in this response
          roles: [], // Will be populated from /api/auth/me endpoint
          requiresPasswordChange: responseData.requiresPasswordChange || false,
          requiresFirstTimeSetup: responseData.requiresFirstTimeSetup || false,
          setupStep: responseData.setupStep || undefined
        };
        
        // Store the token in localStorage
        localStorage.setItem('accessToken', responseData.token);
        
        return authData;
      } else {
        throw new Error('Invalid response format: missing token');
      }
    } else {
      throw new Error('Invalid response format');
    }
  } catch (error: any) {
    console.error('Login error:', error);
    
    if (error.response?.status === 401) {
      throw new Error('Invalid credentials');
    } else if (error.response?.status === 400) {
      throw new Error('Invalid request format');
    } else {
      throw new Error('Login failed. Please try again.');
    }
  }
};

const logout = (): void => {
  console.log('=== LOGOUT FUNCTION CALLED ===');
  
  // Clear all auth-related data
  localStorage.removeItem('user');
  localStorage.removeItem('accessToken');
  localStorage.removeItem('refreshToken');
  
  console.log('User logged out successfully');
};

const getCurrentUser = (): AuthResponse | null => {
  console.log('=== GET CURRENT USER FUNCTION CALLED ===');
  
  const userStr = localStorage.getItem('user');
  if (!userStr) {
    console.log('No user found in localStorage');
    return null;
  }
  
  try {
    const user = JSON.parse(userStr);
    console.log('Current user:', user);
    return user;
  } catch (error) {
    console.error('Error parsing user from localStorage:', error);
    localStorage.removeItem('user');
    return null;
  }
};

const isAuthenticated = (): boolean => {
  console.log('=== IS AUTHENTICATED FUNCTION CALLED ===');
  
  const user = getCurrentUser();
  const hasToken = !!localStorage.getItem('accessToken');
  
  console.log('User exists:', !!user);
  console.log('Has token:', hasToken);
  
  return !!(user && hasToken);
};

const refreshToken = async (): Promise<AuthResponse> => {
  console.log('=== REFRESH TOKEN FUNCTION CALLED ===');
  
  const refreshToken = localStorage.getItem('refreshToken');
  if (!refreshToken) {
    throw new Error('No refresh token available');
  }
  
  try {
    // Note: Refresh token endpoint is not yet implemented in the generated client
    // This would use ApiClientFactory.getRefreshTokenClient() when available
    throw new Error('Refresh token not yet implemented with generated client');
    
  } catch (error: any) {
    console.error('Refresh token error:', error);
    
    if (error.response?.status === 401) {
      // Refresh token is invalid, user needs to login again
      logout();
      throw new Error('Session expired. Please login again.');
    } else {
      throw new Error('Failed to refresh token. Please login again.');
    }
  }
};

export const authService = {
  login,
  logout,
  getCurrentUser,
  isAuthenticated,
  refreshToken
};

export default authService; 