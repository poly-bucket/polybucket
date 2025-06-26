import { loginClient } from './api.provider';
import { LoginRequest, LoginResponse } from './api.client';

const login = async (credentials: LoginRequest): Promise<LoginResponse> => {
  try {
    const response = await loginClient.login(credentials);
    if (response.token) {
      // For now, we'll just store the raw response.
      // A more robust solution would be to fetch the user details separately.
      localStorage.setItem('user_token', response.token);
    }
    return response;
  } catch (error) {
    console.error('API Login failed:', error);
    throw error;
  }
};

const apiAuthService = {
  login,
};

export default apiAuthService; 