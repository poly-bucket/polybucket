import axios from 'axios';

const API_URL = 'http://localhost:5166/api/auth';

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
}

export interface CheckFirstRunResponse {
  isFirstRun: boolean;
}

export interface SystemSetupStatus {
  id: string;
  isAdminConfigured: boolean;
  isRoleConfigured: boolean;
  isModerationConfigured: boolean;
  requireUploadModeration?: boolean;
  moderatorRoles?: string;
  createdAt: string;
  updatedAt: string;
}

// Mock implementation for testing
let mockFirstRun = true;
const mockUsers: AuthResponse[] = [];

const checkFirstRun = async (): Promise<CheckFirstRunResponse> => {
  if (MOCK_MODE) {
    console.log('MOCK: checkFirstRun', { isFirstRun: mockFirstRun });
    return { isFirstRun: mockFirstRun };
  }
  
  // Create a new instance of axios without interceptors
  const instance = axios.create({
    headers: {
      'Content-Type': 'application/json'
    }
  });
  
  const response = await instance.get(`${API_URL}/check-first-run`);
  return response.data;
};

const getSetupStatus = async (): Promise<SystemSetupStatus> => {
  if (MOCK_MODE) {
    console.log('MOCK: getSetupStatus');
    return { 
      id: '1',
      isAdminConfigured: !mockFirstRun,
      isRoleConfigured: !mockFirstRun,
      isModerationConfigured: !mockFirstRun,
      requireUploadModeration: false,
      moderatorRoles: '',
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString()
    };
  }
  
  // Create a new instance of axios without interceptors
  const instance = axios.create({
    headers: {
      'Content-Type': 'application/json'
    }
  });
  
  const response = await instance.get(`${API_URL}/setup-status`);
  return response.data.data;
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
      id: String(mockUsers.length + 1),
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
    // Create an object that matches the RegisterUserRequest class in the backend
    const backendUserData = {
      Username: userData.username,
      Email: userData.email,
      Password: userData.password,
      FirstName: userData.firstName || 'User',
      LastName: userData.lastName || 'Account',
      IsAdmin: userData.isAdmin || false
    };
    
    console.log('Sending registration data:', backendUserData);
    
    // Create a new instance of axios without interceptors to prevent authorization header
    // from being automatically added to register requests
    const instance = axios.create({
      headers: {
        'Content-Type': 'application/json'
      }
    });
    
    const response = await instance.post(`${API_URL}/register`, backendUserData);
    console.log('Registration response:', response.data);
    console.log('Response structure:', JSON.stringify(response.data, null, 2));
    
    // Create a default AuthResponse object from the response data
    // Handle various response structures that might come from the API
    let authData: AuthResponse;
    
    try {
      // Attempt to extract the needed auth fields from various possible response structures
      if (response.data && typeof response.data === 'object') {
        // Check if response is in expected format
        if (response.data.data && response.data.data.accessToken) {
          authData = response.data.data;
        } else if (response.data.accessToken) {
          authData = response.data;
        } else if (response.data.id && response.data.username) {
          // If the data has user identifiers but not tokens, create a placeholder
          // This allows us to proceed to login in the next step
          authData = {
            id: response.data.id,
            username: response.data.username,
            email: response.data.email || userData.email,
            accessToken: response.data.token || '',
            refreshToken: response.data.refreshToken || '',
            roles: response.data.roles || [userData.isAdmin ? 'Admin' : 'User']
          };
        } else {
          // If we can't extract a proper auth object, create one from the submitted data
          // This is a fallback so we can proceed to the explicit login step
          authData = {
            id: '0', // Temporary ID
            username: userData.username,
            email: userData.email,
            accessToken: '', // Will be obtained in the login step
            refreshToken: '',
            roles: []
          };
        }
      } else {
        // If response.data isn't an object, create minimal auth data from the userData
        authData = {
          id: '0',
          username: userData.username,
          email: userData.email,
          accessToken: '',
          refreshToken: '',
          roles: []
        };
      }
    } catch (parseError) {
      console.error('Error parsing auth data:', parseError);
      // Fallback to minimal auth data
      authData = {
        id: '0',
        username: userData.username,
        email: userData.email,
        accessToken: '',
        refreshToken: '',
        roles: []
      };
    }
    
    console.log('Extracted auth data:', authData);
    
    // Store the auth data in localStorage if it contains an accessToken
    if (authData.accessToken) {
      localStorage.setItem('user', JSON.stringify(authData));
    }
    
    return authData;
  } catch (error) {
    console.error('Registration error:', error);
    if (axios.isAxiosError(error) && error.response) {
      console.error('Error status:', error.response.status);
      console.error('Error data:', error.response.data);
      
      // Extract and format validation errors
      let errorMessage = 'Registration failed';
      let validationErrors = {};
      
      if (error.response.data) {
        // Handle structured error response
        if (error.response.data.message) {
          errorMessage = error.response.data.message;
        }
        
        if (error.response.data.validationErrors) {
          validationErrors = error.response.data.validationErrors;
          console.error('Validation errors:', validationErrors);
        }
      }
      
      // Throw a structured error that can be handled by components
      throw {
        message: errorMessage,
        validationErrors,
        original: error
      };
    }
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
    const user = getCurrentUser();
    if (!user) {
      throw new Error('No user found');
    }
    
    // For testing: just return the same user
    await new Promise(resolve => setTimeout(resolve, 300)); // Simulate network delay
    return user;
  }
  
  const user = getCurrentUser();
  if (!user) {
    throw new Error('No user found');
  }

  try {
    // Fix capitalization to match backend expectations
    const backendRequest = {
      RefreshToken: user.refreshToken
    };
    
    const response = await axios.post(`${API_URL}/refresh-token`, backendRequest);
    console.log('Refresh token response:', response.data);
    
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
          const responseUser = data.user;
          authData = {
            id: responseUser.id,
            username: responseUser.username,
            email: responseUser.email,
            accessToken: data.accessToken,
            refreshToken: data.refreshToken,
            roles: responseUser.isAdmin ? ['Admin'] : ['User']
          };
        } else {
          // Fallback for other data structures
          console.log('No user property but has data with accessToken');
          authData = {
            id: data.id || user.id,
            username: data.username || user.username,
            email: data.email || user.email,
            accessToken: data.accessToken,
            refreshToken: data.refreshToken || user.refreshToken,
            roles: data.roles || user.roles
          };
        }
      } else {
        // Other formats
        console.log('Alternative response format detected');
        if (response.data.accessToken) {
          // Direct token at root
          authData = {
            ...user, // Keep existing user data
            accessToken: response.data.accessToken,
            refreshToken: response.data.refreshToken || user.refreshToken
          };
        } else {
          console.error('Unrecognized refresh token response:', response.data);
          throw new Error('Invalid refresh token response format: missing accessToken');
        }
      }
    } else {
      throw new Error('Invalid refresh token response format: not an object');
    }
    
    // Validate we have a token
    if (!authData.accessToken) {
      console.error('No access token in refresh response:', authData);
      throw new Error('Token refresh successful but no access token received');
    }
    
    // Store updated auth data
    localStorage.setItem('user', JSON.stringify(authData));
    console.log('Updated auth data in localStorage after refresh');
    
    return authData;
  } catch (error) {
    console.error('Error refreshing token:', error);
    throw error;
  }
};

const authService = {
  checkFirstRun,
  login,
  register,
  logout,
  getCurrentUser,
  refreshToken,
  getSetupStatus,
  isAdminConfigured,
  isRoleConfigured,
  setAdminConfigured,
  setRoleConfigured,
  setModerationConfigured
};

export default authService; 