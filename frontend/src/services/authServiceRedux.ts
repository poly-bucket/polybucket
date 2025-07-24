import store from '../store';
import { 
  loginUser, 
  registerUser, 
  refreshUserToken, 
  logoutUser,
  LoginRequest,
  RegisterRequest,
  AuthResponse 
} from '../store/thunks/authThunks';

export interface AuthServiceInterface {
  login: (credentials: LoginRequest) => Promise<AuthResponse>;
  register: (userData: RegisterRequest) => Promise<AuthResponse>;
  logout: () => Promise<void>;
  getCurrentUser: () => AuthResponse | null;
  isAuthenticated: () => boolean;
  refreshToken: () => Promise<AuthResponse>;
}

const login = async (credentials: LoginRequest): Promise<AuthResponse> => {
  console.log('=== AUTH SERVICE LOGIN CALLED ===');
  
  const result = await store.dispatch(loginUser(credentials));
  
  if (loginUser.fulfilled.match(result)) {
    return result.payload;
  } else {
    throw new Error(result.payload as string || 'Login failed');
  }
};

const register = async (userData: RegisterRequest): Promise<AuthResponse> => {
  console.log('=== AUTH SERVICE REGISTER CALLED ===');
  
  const result = await store.dispatch(registerUser(userData));
  
  if (registerUser.fulfilled.match(result)) {
    return result.payload;
  } else {
    throw new Error(result.payload as string || 'Registration failed');
  }
};

const logout = async (): Promise<void> => {
  console.log('=== AUTH SERVICE LOGOUT CALLED ===');
  
  await store.dispatch(logoutUser());
  
  // Clear any remaining localStorage items except 'polybucket'
  const keysToKeep = ['polybucket'];
  const keysToRemove = Object.keys(localStorage).filter(key => !keysToKeep.includes(key));
  keysToRemove.forEach(key => localStorage.removeItem(key));
};

const getCurrentUser = (): AuthResponse | null => {
  const state = store.getState();
  return state.auth.user;
};

const isAuthenticated = (): boolean => {
  const user = getCurrentUser();
  return !!user && !!user.accessToken;
};

const refreshToken = async (): Promise<AuthResponse> => {
  const user = getCurrentUser();
  if (!user?.refreshToken) {
    throw new Error('No refresh token available');
  }
  
  const result = await store.dispatch(refreshUserToken(user.refreshToken));
  
  if (refreshUserToken.fulfilled.match(result)) {
    const tokenData = result.payload;
    const updatedUser = { 
      ...user, 
      accessToken: tokenData.accessToken,
      refreshToken: tokenData.refreshToken || user.refreshToken
    };
    
    // Update the user in the store
    store.dispatch({ 
      type: 'auth/updateUserDetails', 
      payload: updatedUser 
    });
    
    return updatedUser;
  } else {
    throw new Error('Failed to refresh token');
  }
};

export const authServiceRedux: AuthServiceInterface = {
  login,
  register,
  logout,
  getCurrentUser,
  isAuthenticated,
  refreshToken
};

export default authServiceRedux; 