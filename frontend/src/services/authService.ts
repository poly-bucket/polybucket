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
  firstName?: string;
  lastName?: string;
  isAdmin?: boolean;
}

export interface AuthResponse {
  id: string;
  username: string;
  email: string;
  accessToken: string;
  refreshToken: string;
  roles: string[];
  profilePictureUrl?: string; // Optional: for when users upload profile pictures
}

export interface CheckFirstRunResponse {
  isFirstRun: boolean;
}

export interface SystemSetupStatus {
  isAdminConfigured: boolean;
  isRoleConfigured: boolean;
  isModerationConfigured: boolean;
}

// Mock implementation for testing
let mockFirstRun = true;
const mockUsers: AuthResponse[] = [];

const checkFirstRun = async (): Promise<CheckFirstRunResponse> => {
  if (MOCK_MODE) {
    console.log('MOCK: checkFirstRun', { isFirstRun: mockFirstRun });
    return { isFirstRun: mockFirstRun };
  }
  
  // Use the setup-status endpoint to determine if it's first run
  const setupStatus = await getSetupStatus();
  const isFirstRun = !setupStatus.isAdminConfigured;
  
  return { isFirstRun };
};

const getSetupStatus = async (): Promise<SystemSetupStatus> => {
  if (MOCK_MODE) {
    console.log('MOCK: getSetupStatus');
    return { 
      isAdminConfigured: !mockFirstRun,
      isRoleConfigured: !mockFirstRun,
      isModerationConfigured: !mockFirstRun,
    };
  }
  
  // Create a new instance of axios without interceptors
  const instance = axios.create({
    headers: {
      'Content-Type': 'application/json'
    }
  });
  
  const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:11666';
  const response = await instance.get(`${baseUrl}/api/system-settings/setup-status`);
  const data = response.data;
  
  // Map backend response to frontend interface
  return {
    isAdminConfigured: data.isAdminSetupComplete,
    isRoleConfigured: data.isRoleSetupComplete,
    isModerationConfigured: data.isModerationSetupComplete
  };
};

const isAdminConfigured = async (): Promise<boolean> => {
  if (MOCK_MODE) {
    console.log('MOCK: isAdminConfigured');
    return !mockFirstRun;
  }
  
  // Create a new instance of axios without interceptors
  const instance = axios.create({
    headers: {
      'Content-Type': 'application/json'
    }
  });
  
  const response = await instance.get(`${API_URL}/is-admin-configured`);
  return response.data.data;
};

const isRoleConfigured = async (): Promise<boolean> => {
  if (MOCK_MODE) {
    console.log('MOCK: isRoleConfigured');
    return !mockFirstRun;
  }
  
  // Create a new instance of axios without interceptors
  const instance = axios.create({
    headers: {
      'Content-Type': 'application/json'
    }
  });
  
  const response = await instance.get(`${API_URL}/is-role-configured`);
  return response.data.data;
};

const setAdminConfigured = async (isConfigured: boolean): Promise<boolean> => {
  if (MOCK_MODE) {
    console.log('MOCK: setAdminConfigured', isConfigured);
    mockFirstRun = !isConfigured;
    return true;
  }
  
  const user = getCurrentUser();
  const headers = {
    'Content-Type': 'application/json'
  };
  
  if (user?.accessToken) {
    Object.assign(headers, { Authorization: `Bearer ${user.accessToken}` });
  }
  
  const response = await axios.post(`${API_URL}/set-admin-configured`, isConfigured, { headers });
  return response.data.data;
};

const setRoleConfigured = async (isConfigured: boolean): Promise<boolean> => {
  if (MOCK_MODE) {
    console.log('MOCK: setRoleConfigured', isConfigured);
    mockFirstRun = !isConfigured;
    return true;
  }
  
  const user = getCurrentUser();
  const headers = {
    'Content-Type': 'application/json'
  };
  
  if (user?.accessToken) {
    Object.assign(headers, { Authorization: `Bearer ${user.accessToken}` });
  }
  
  const response = await axios.post(`${API_URL}/set-role-configured`, isConfigured, { headers });
  return response.data.data;
};

const setModerationConfigured = async (isConfigured: boolean): Promise<boolean> => {
  if (MOCK_MODE) {
    console.log('MOCK: setModerationConfigured', isConfigured);
    return true;
  }
  
  const user = getCurrentUser();
  const headers = {
    'Content-Type': 'application/json'
  };
  
  if (user?.accessToken) {
    Object.assign(headers, { Authorization: `Bearer ${user.accessToken}` });
  }
  
  const response = await axios.post(`${API_URL}/set-moderation-configured`, isConfigured, { headers });
  return response.data.data;
};

const login = async (credentials: LoginRequest): Promise<AuthResponse> => {
  if (MOCK_MODE) {
    console.log('MOCK: login', credentials);
    return MOCK_ADMIN_USER;
  }
  
  try {
    console.log('Sending login credentials:', { emailOrUsername: credentials.emailOrUsername });
    
    // Create a new instance of axios without interceptors to prevent authorization header
    // from being automatically added to login requests, which can cause 401 errors
    const instance = axios.create({
      headers: {
        'Content-Type': 'application/json'
      }
    });
    
    // Fix capitalization to match backend expectations
    const backendCredentials = {
      EmailOrUsername: credentials.emailOrUsername,
      Password: credentials.password
    };
    
    const response = await instance.post(`${API_URL}/login`, backendCredentials);
    console.log('Login response:', response.data);
    console.log('Login response structure:', JSON.stringify(response.data, null, 2));
    
    let authData: AuthResponse;
    
    // Handle different response formats - prioritize the most common format we've seen
    if (response.data && typeof response.data === 'object') {
      if (response.data.succeeded === true && response.data.data) {
        console.log('Processing succeeded=true response with data property');
        // Most common format: { succeeded: true, data: { user: {...}, accessToken: "..." } } 
        const data = response.data.data;
        
        if (data.user && data.accessToken) {
          // This is the actual format from the backend
          console.log('Found user and accessToken in data');
          const user = data.user;
          authData = {
            id: user.id,
            username: user.username,
            email: user.email,
            accessToken: data.accessToken,
            refreshToken: data.refreshToken,
            roles: user.isAdmin ? ['Admin'] : ['User'] // Handle roles
          };
        } else {
          // Fallback for other data structures
          console.log('No user property but has data with accessToken');
          authData = {
            id: data.id || '',
            username: data.username || '',
            email: data.email || '',
            accessToken: data.accessToken,
            refreshToken: data.refreshToken || '',
            roles: data.roles || []
          };
        }
      } else {
        // Other formats
        console.log('Alternative response format detected');
        if (response.data.accessToken) {
          // Direct root level token
          authData = response.data;
          authData.roles = authData.roles || [];
        } else {
          console.error('Unrecognized login response format:', response.data);
          throw new Error('Invalid login response format: missing accessToken');
        }
      }
    } else {
      throw new Error('Invalid login response format: not an object');
    }
    
    // Validate that we have an access token
    if (!authData.accessToken) {
      console.error('No access token found in response:', authData);
      throw new Error('Login successful but no access token received');
    }
    
    // Store user in localStorage
    localStorage.setItem('user', JSON.stringify(authData));
    console.log('Saved auth data to localStorage:', authData);
    
    return authData;
  } catch (error) {
    console.error('Login error:', error);
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
      roles: ['User']
    };
    mockUsers.push(newUser);
    localStorage.setItem('user', JSON.stringify(newUser));
    return newUser;
  }

  // Create a new instance of axios without interceptors
  const instance = axios.create({
    headers: {
      'Content-Type': 'application/json'
    }
  });
  
  try {
    const response = await instance.post(`${API_URL}/register`, userData);
    
    // Handle different response formats
    if (response.data && typeof response.data === 'object') {
      let authData: AuthResponse;
      if (response.data.succeeded && response.data.data) {
        const data = response.data.data;
        authData = {
          id: data.id,
          username: data.username,
          email: data.email,
          accessToken: data.accessToken,
          refreshToken: data.refreshToken,
          roles: data.roles || []
        };
      } else if (response.data.accessToken) {
        authData = response.data;
      } else {
        throw new Error('Invalid registration response format');
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

const adminSetup = async (userData: RegisterRequest): Promise<AuthResponse> => {
  if (MOCK_MODE) {
    console.log('MOCK: adminSetup', userData);
    const newAdmin: AuthResponse = {
      id: Math.random().toString(),
      username: userData.username,
      email: userData.email,
      accessToken: 'fake-jwt-token',
      refreshToken: 'fake-refresh-token',
      roles: ['Admin']
    };
    mockUsers.push(newAdmin);
    localStorage.setItem('user', JSON.stringify(newAdmin));
    return newAdmin;
  }

  // Create a new instance of axios without interceptors
  const instance = axios.create({
    headers: {
      'Content-Type': 'application/json'
    }
  });
  
  try {
    // Call the admin-setup endpoint with ReplaceDefaultAdmin flag
    const adminSetupData = {
      username: userData.username,
      email: userData.email,
      password: userData.password,
      replaceDefaultAdmin: true
    };
    
    const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:11666';
    const response = await instance.post(`${baseUrl}/api/system-settings/admin-setup`, adminSetupData);
    
    // After successful admin setup, login with the new credentials
    const loginData = {
      emailOrUsername: userData.username,
      password: userData.password
    };
    
    const loginResponse = await instance.post(`${API_URL}/login`, loginData);
    
    // Handle login response
    if (loginResponse.data && typeof loginResponse.data === 'object') {
      let authData: AuthResponse;
      if (loginResponse.data.succeeded && loginResponse.data.data) {
        const data = loginResponse.data.data;
        if (data.user) {
          authData = {
            id: data.user.id,
            username: data.user.username,
            email: data.user.email,
            accessToken: data.accessToken,
            refreshToken: data.refreshToken,
            roles: data.user.isAdmin ? ['Admin'] : ['User']
          };
        } else {
          authData = {
            id: data.id || '',
            username: data.username || '',
            email: data.email || '',
            accessToken: data.accessToken,
            refreshToken: data.refreshToken || '',
            roles: data.roles || []
          };
        }
      } else if (loginResponse.data.accessToken) {
        authData = loginResponse.data;
        authData.roles = authData.roles || [];
      } else {
        throw new Error('Invalid login response format after admin setup');
      }
      
      localStorage.setItem('user', JSON.stringify(authData));
      return authData;
    } else {
      throw new Error('Invalid login response format after admin setup');
    }
  } catch (error) {
    console.error('Admin setup error:', error);
    throw error;
  }
};

const logout = (): void => {
  localStorage.removeItem('user');
};

const getCurrentUser = (): AuthResponse | null => {
  try {
    const userStr = localStorage.getItem('user');
    if (!userStr) {
      return null;
    }
    
    const user = JSON.parse(userStr) as AuthResponse;
    
    // Ensure the object is valid
    if (!user || typeof user !== 'object') {
      console.error('Invalid user data in localStorage');
      return null;
    }
    
    // Ensure we have an access token
    if (!user.accessToken) {
      console.error('User data in localStorage missing accessToken');
      return null;
    }
    
    // Ensure roles is never undefined
    user.roles = user.roles || [];
    
    return user;
  } catch (error) {
    console.error('Error retrieving user from localStorage:', error);
    // Clear invalid data
    localStorage.removeItem('user');
    return null;
  }
};

const refreshToken = async (): Promise<AuthResponse> => {
  if (MOCK_MODE) {
    console.log('MOCK: refreshToken');
    const user = getCurrentUser();
    if (!user) {
      throw new Error('No user to refresh');
    }
    return { ...user, accessToken: 'new-fake-jwt-token' };
  }
  
  const user = getCurrentUser();
  if (!user || !user.refreshToken) {
    throw new Error('No refresh token available');
  }

  try {
    const response = await axios.post(`${API_URL}/refresh-token`, {
      refreshToken: user.refreshToken
    });
    
    if (response.data && response.data.accessToken) {
      const updatedUser = {
        ...user,
        accessToken: response.data.accessToken,
        refreshToken: response.data.refreshToken || user.refreshToken
      };
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
  checkFirstRun,
  getSetupStatus,
  isAdminConfigured,
  isRoleConfigured,
  setAdminConfigured,
  setRoleConfigured,
  setModerationConfigured,
  login,
  register,
  adminSetup,
  logout,
  getCurrentUser,
  refreshToken,
};

export default authService; 