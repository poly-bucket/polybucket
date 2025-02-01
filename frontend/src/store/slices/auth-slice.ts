import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { jwtDecode } from 'jwt-decode';

export interface User {
  id: string;
  username: string;
  role: string;
}

interface JwtPayload {
  sub: string;
  name: string;
  role: string;
  agent: string;
  exp: number;
}

export interface AuthState {
  isAuthenticated: boolean;
  token: string | null;
  user: User | null;
}

// Load initial state from localStorage
const loadState = (): AuthState => {
  try {
    const serializedState = localStorage.getItem('auth');
    if (serializedState === null) {
      return {
        isAuthenticated: false,
        token: null,
        user: null,
      };
    }
    const state = JSON.parse(serializedState);
    // Verify token is still valid
    if (state.token) {
      const decoded = jwtDecode<JwtPayload>(state.token);
      if (decoded.exp * 1000 < Date.now()) {
        return {
          isAuthenticated: false,
          token: null,
          user: null,
        };
      }
      // Add user information from token
      state.user = {
        id: decoded.sub,
        username: decoded.name,
        role: decoded.role,
      };
    }
    return state;
  } catch (err) {
    return {
      isAuthenticated: false,
      token: null,
      user: null,
    };
  }
};

const initialState: AuthState = loadState();

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    setCredentials: (state, action: PayloadAction<{ token: string }>) => {
      const { token } = action.payload;
      state.token = token;
      state.isAuthenticated = true;
      
      // Decode token and set user information
      try {
        const decoded = jwtDecode<JwtPayload>(token);
        state.user = {
          id: decoded.sub,
          username: decoded.name,
          role: decoded.role,
        };
      } catch (err) {
        console.error('Failed to decode token:', err);
      }

      // Save to localStorage
      localStorage.setItem('auth', JSON.stringify(state));
    },
    clearCredentials: (state) => {
      state.token = null;
      state.isAuthenticated = false;
      state.user = null;
      localStorage.removeItem('auth');
    },
  },
});

export const { setCredentials, clearCredentials } = authSlice.actions;
export const logout = clearCredentials;

// Selectors
export const selectCurrentToken = (state: { auth: AuthState }) => state.auth.token;
export const selectIsAuthenticated = (state: { auth: AuthState }) => state.auth.isAuthenticated;
export const selectCurrentUser = (state: { auth: AuthState }) => state.auth.user;
export const selectCurrentUserId = (state: { auth: AuthState }) => state.auth.user?.id ?? null;

export default authSlice.reducer; 