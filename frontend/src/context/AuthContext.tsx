import React, { createContext, useContext, useEffect, ReactNode } from 'react';
import { useAppDispatch, useAppSelector } from '../store';
import { loginUser, logoutUser, registerUser } from '../store/thunks/authThunks';
import { clearUser } from '../store/slices/authSlice';
import { AuthResponse, LoginRequest, RegisterRequest } from '../store/thunks/authThunks';

interface AuthContextType {
  user: AuthResponse | null;
  isAuthenticated: boolean;
  isAdmin: boolean;
  isLoading: boolean;
  isError: boolean;
  errorMessage: string;
  login: (credentials: LoginRequest) => Promise<boolean>;
  logout: () => Promise<void>;
  register: (userData: RegisterRequest) => Promise<boolean>;
}

const defaultContext: AuthContextType = {
  user: null,
  isAuthenticated: false,
  isAdmin: false,
  isLoading: false,
  isError: false,
  errorMessage: '',
  login: async () => false,
  logout: async () => {},
  register: async () => false,
};

const AuthContext = createContext<AuthContextType>(defaultContext);

export const useAuth = () => useContext(AuthContext);

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const dispatch = useAppDispatch();
  const { user, isLoading, isError, errorMessage } = useAppSelector(state => state.auth);
  
  const isAuthenticated = !!user && !!user.accessToken;
  const isAdmin = user?.roles?.includes('Admin') || false;

  const login = async (credentials: LoginRequest): Promise<boolean> => {
    try {
      const result = await dispatch(loginUser(credentials));
      return loginUser.fulfilled.match(result);
    } catch (error) {
      console.error('Login failed:', error);
      return false;
    }
  };

  const logout = async (): Promise<void> => {
    try {
      await dispatch(logoutUser());
      dispatch(clearUser());
    } catch (error) {
      console.error('Logout failed:', error);
      // Even if logout fails, clear the user from state
      dispatch(clearUser());
    }
  };

  const register = async (userData: RegisterRequest): Promise<boolean> => {
    try {
      const result = await dispatch(registerUser(userData));
      return registerUser.fulfilled.match(result);
    } catch (error) {
      console.error('Registration failed:', error);
      return false;
    }
  };

  const value: AuthContextType = {
    user,
    isAuthenticated,
    isAdmin,
    isLoading,
    isError,
    errorMessage,
    login,
    logout,
    register
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
}; 