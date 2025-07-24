import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL ? `${import.meta.env.VITE_API_URL}/api/auth` : 'http://localhost:11666/api/auth';

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
  emailOrUsername: string;
  password: string;
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
}

// Mock implementation for testing
const mockUsers: AuthResponse[] = [];

const login = async (credentials: LoginRequest): Promise<AuthResponse> => {
  console.log('=== LOGIN FUNCTION CALLED ===');
  console.log('MOCK_MODE:', MOCK_MODE);
  console.log('API_URL:', API_URL);
  
  if (MOCK_MODE) {
    console.log('MOCK: login', credentials);
    return MOCK_ADMIN_USER;
  }
  
  try {
    console.log('Sending login credentials:', { emailOrUsername: credentials.emailOrUsername });
    console.log('Full credentials object:', credentials);
    
    // Create a new instance of axios without interceptors to prevent authorization header
    // from being automatically added to login requests, which can cause 401 errors
    const instance = axios.create({
      headers: {
        'Content-Type': 'application/json'
      }
    });
    
    // Fix capitalization to match backend expectations
    const backendCredentials = {
      Email: credentials.emailOrUsername,
      Password: credentials.password
    };
    
    console.log('Making axios request to:', `${API_URL}/login`);
    console.log('Request payload:', JSON.stringify(backendCredentials, null, 2));
    
    const response = await instance.post(`${API_URL}/login`, backendCredentials);
    console.log('Login response received!');
    console.log('Login response:', response.data);
    console.log('Login response structure:', JSON.stringify(response.data, null, 2));
    console.log('Response status:', response.status);
    
    let authData: AuthResponse;
    
    // The backend returns a simple format: { token: "...", requiresPasswordChange: boolean, etc. }
    if (response.data && typeof response.data === 'object') {
      if (response.data.token) {
        console.log('Processing login response with token');
        authData = {
          id: '', // Will be extracted from /api/auth/me endpoint
          username: credentials.emailOrUsername.split('@')[0], // Temporary username
          email: credentials.emailOrUsername,
          accessToken: response.data.token,
          refreshToken: '', // No refresh token in this response
          roles: [], // Will be populated from /api/auth/me endpoint
          requiresPasswordChange: response.data.requiresPasswordChange || false,
          requiresFirstTimeSetup: response.data.requiresFirstTimeSetup || false,
          setupStep: response.data.setupStep || undefined
        };
      } else if (response.data.accessToken) {
        // Alternative format with accessToken
        authData = {
          id: '',
          username: credentials.emailOrUsername.split('@')[0],
          email: credentials.emailOrUsername,
          accessToken: response.data.accessToken,
          refreshToken: response.data.refreshToken || '',
          roles: [],
          requiresPasswordChange: response.data.requiresPasswordChange || false,
          requiresFirstTimeSetup: response.data.requiresFirstTimeSetup || false,
          setupStep: response.data.setupStep || undefined
        };
      } else {
        console.error('Unrecognized login response format:', response.data);
        throw new Error('Invalid login response format: missing token or accessToken');
      }
    } else {
      throw new Error('Invalid login response format: not an object');
    }
    
    // Store the initial auth data
    localStorage.setItem('user', JSON.stringify(authData));
    
    // Now fetch the complete user information including role
    try {
      console.log('=== FETCHING USER DETAILS ===');
      console.log('Fetching user details from /api/auth/me...');
      console.log('Using token:', authData.accessToken.substring(0, 20) + '...');
      console.log('API URL:', `${API_URL.replace('/auth', '')}/auth/me`);
      
      const userResponse = await instance.get(`${API_URL.replace('/auth', '')}/auth/me`, {
        headers: {
          'Authorization': `Bearer ${authData.accessToken}`
        }
      });
      
      console.log('=== USER DETAILS RESPONSE ===');
      console.log('Response status:', userResponse.status);
      console.log('Response headers:', userResponse.headers);
      
      console.log('User details response:', userResponse.data);
      console.log('User role from /api/auth/me:', userResponse.data.role);
      console.log('User role type:', typeof userResponse.data.role);
      
      // Update auth data with complete user information
      const userData = userResponse.data;
      console.log('Before update - authData.roles:', authData.roles);
      authData.id = userData.id;
      authData.username = userData.username;
      authData.email = userData.email;
      authData.avatar = userData.avatar;
      authData.roles = [userData.role]; // Convert role string to array
      
      console.log('After update - authData.roles:', authData.roles);
      console.log('userData.role value:', userData.role);
      
      // Update localStorage with complete user data
      localStorage.setItem('user', JSON.stringify(authData));
      
      console.log('Updated auth data with user details:', authData);
    } catch (error: any) {
      console.error('Failed to fetch user details from /api/auth/me:', error);
      console.error('Error details:', {
        message: error.message,
        response: error.response?.data,
        status: error.response?.status
      });
      console.warn('Attempting to extract role from JWT token...');
      
      // Try to extract role from JWT token as fallback
      try {
        const tokenParts = authData.accessToken.split('.');
        if (tokenParts.length === 3) {
          const payload = JSON.parse(atob(tokenParts[1]));
          console.log('JWT payload:', payload);
          if (payload.role) {
            console.log('Extracted role from JWT:', payload.role);
            authData.roles = [payload.role];
            localStorage.setItem('user', JSON.stringify(authData));
          }
        }
      } catch (jwtError) {
        console.error('Failed to extract role from JWT:', jwtError);
      }
      
      console.warn('Continuing with basic auth data');
    }
    
    return authData;
  } catch (error: any) {
    console.error('Login error:', error);
    console.error('Error details:', {
      message: error.message,
      response: error.response?.data,
      status: error.response?.status
    });
    throw error;
  }
};

const register = async (userData: RegisterRequest): Promise<AuthResponse> => {
  if (MOCK_MODE) {
    console.log('MOCK: register', userData);
    const newUser: AuthResponse = {
      id: Math.random().toString(),
      username: userData.username,
      email: userData.email,
      accessToken: 'fake-jwt-token',
      refreshToken: 'fake-refresh-token',
      roles: userData.isAdmin ? ['Admin'] : ['User']
    };
    mockUsers.push(newUser);
    localStorage.setItem('user', JSON.stringify(newUser));
    return newUser;
  }

  try {
    const response = await axios.post(`${API_URL}/register`, userData);
    
    if (response.data && typeof response.data === 'object') {
      let authData: AuthResponse;
      if (response.data.accessToken) {
        authData = response.data;
        authData.roles = authData.roles || [];
      } else {
        throw new Error('Invalid registration response format: missing accessToken');
      }
      
      localStorage.setItem('user', JSON.stringify(authData));
      return authData;
    } else {
      throw new Error('Invalid registration response format');
    }
  } catch (error) {
    console.error('Registration error:', error);
    throw error;
  }
};

const logout = (): void => {
  localStorage.removeItem('user');
};

const getCurrentUser = (): AuthResponse | null => {
  try {
    const userStr = localStorage.getItem('user');
    if (userStr) {
      return JSON.parse(userStr);
    }
    return null;
  } catch (error) {
    console.error('Error getting current user:', error);
    return null;
  }
};

const refreshToken = async (): Promise<AuthResponse> => {
  const user = getCurrentUser();
  if (!user?.refreshToken) {
    throw new Error('No refresh token available');
  }

  if (MOCK_MODE) {
    console.log('MOCK: refreshToken');
    return user;
  }

  try {
    const response = await axios.post(`${API_URL}/refresh-token`, {
      refreshToken: user.refreshToken
    });

    if (response.data && response.data.accessToken) {
      const updatedUser = { ...user, accessToken: response.data.accessToken };
      localStorage.setItem('user', JSON.stringify(updatedUser));
      return updatedUser;
    } else {
      throw new Error('Invalid refresh token response');
    }
  } catch (error) {
    console.error('Refresh token error:', error);
    logout(); // Logout user if refresh fails
    throw error;
  }
};

const authService = {
  login,
  register,
  logout,
  getCurrentUser,
  refreshToken,
};

export default authService; 