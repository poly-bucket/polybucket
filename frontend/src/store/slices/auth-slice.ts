import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { RootState } from '../store';

export interface User {
  id: string;
  email: string;
  username: string;
  firstName?: string;
  lastName?: string;
}

export interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  token: string | null;
  userId: string | null;
}

// Load initial state from localStorage
const loadState = (): AuthState => {
  try {
    const serializedState = localStorage.getItem('auth');
    if (serializedState === null) {
      return {
        user: null,
        isAuthenticated: false,
        token: null,
        userId: null,
      };
    }
    return JSON.parse(serializedState);
  } catch (err) {
    return {
      user: null,
      isAuthenticated: false,
      token: null,
      userId: null,
    };
  }
};

const initialState: AuthState = loadState();

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    setCredentials: (
      state,
      action: PayloadAction<{ user: User; token: string }>
    ) => {
      const { user, token } = action.payload;
      state.user = user;
      state.token = token;
      state.isAuthenticated = true;
      state.userId = user.id;
      // Save to localStorage
      localStorage.setItem('auth', JSON.stringify(state));
    },
    logout: (state) => {
      state.user = null;
      state.token = null;
      state.isAuthenticated = false;
      state.userId = null;
      // Clear from localStorage
      localStorage.removeItem('auth');
    },
  },
});

export const { setCredentials, logout } = authSlice.actions;
export default authSlice.reducer;

// Add this selector
export const selectCurrentUserId = (state: RootState) => state.auth.userId; 