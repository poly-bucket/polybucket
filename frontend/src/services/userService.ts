import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL ? `${import.meta.env.VITE_API_URL}/api/admin/users` : 'http://localhost:11666/api/admin/users';

// Helper to get auth headers
const getAuthHeaders = () => {
  try {
    const userStr = localStorage.getItem('user');
    if (!userStr) {
      throw new Error('No user found in localStorage');
    }
    
    const user = JSON.parse(userStr);
    if (!user.accessToken) {
      throw new Error('No access token found in user data');
    }
    
    return {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${user.accessToken}`
    };
  } catch (error) {
    console.error('Error creating auth headers:', error);
    throw error;
  }
};

// Types for user creation
export interface CreateUserRequest {
  email: string;
  username: string;
  role: 'User' | 'Moderator' | 'Admin';
  firstName?: string;
  lastName?: string;
  country?: string;
}

export interface CreateUserResponse {
  userId: string;
  email: string;
  username: string;
  role: 'User' | 'Moderator' | 'Admin';
  firstName?: string;
  lastName?: string;
  country?: string;
  generatedPassword: string;
  createdAt: string;
  emailVerificationRequired: boolean;
}

// User service functions
const createUser = async (userData: CreateUserRequest): Promise<CreateUserResponse> => {
  try {
    const headers = getAuthHeaders();
    console.log('Creating user with data:', userData);
    
    const response = await axios.post(`${API_URL}`, userData, { headers });
    console.log('User creation response:', response.data);
    
    return response.data;
  } catch (error) {
    console.error('Error creating user:', error);
    if (axios.isAxiosError(error) && error.response) {
      console.error('Response status:', error.response.status);
      console.error('Response data:', error.response.data);
      
      if (error.response.status === 401) {
        localStorage.removeItem('user');
        throw new Error('Authentication failed. Please log in again.');
      } else if (error.response.status === 403) {
        throw new Error('Permission denied. You do not have the required permissions to create users.');
      } else if (error.response.status === 409) {
        throw new Error(error.response.data.message || 'User with this email or username already exists.');
      } else if (error.response.status === 400) {
        throw new Error(error.response.data.message || 'Invalid user data provided.');
      }
    }
    throw error;
  }
};

const userService = {
  createUser,
};

export default userService; 