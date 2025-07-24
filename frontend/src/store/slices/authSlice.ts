import { createSlice, PayloadAction, ActionReducerMapBuilder } from '@reduxjs/toolkit';
import { 
  loginUser, 
  registerUser, 
  logoutUser, 
  refreshUserToken 
} from '../thunks/authThunks';
import { AuthResponse } from '../thunks/authThunks';
import { extractUserFromJWT } from '../../utils/jwtUtils';

interface AuthState {
  user: AuthResponse | null;
  isLoading: boolean;
  isSuccess: boolean;
  isError: boolean;
  errorMessage: string;
  isInitialized: boolean;
}

const initialState: AuthState = {
  user: null,
  isLoading: false,
  isSuccess: false,
  isError: false,
  errorMessage: '',
  isInitialized: false
};

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    reset: (state: AuthState) => {
      state.isLoading = false;
      state.isSuccess = false;
      state.isError = false;
      state.errorMessage = '';
    },
    clearUser: (state: AuthState) => {
      state.user = null;
      state.isSuccess = false;
      state.isError = false;
      state.errorMessage = '';
      
      // Clear localStorage
      localStorage.removeItem('user');
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
    },
    updateUserDetails: (state: AuthState, action: PayloadAction<Partial<AuthResponse>>) => {
      if (state.user) {
        state.user = { ...state.user, ...action.payload };
      }
    },
    setInitialized: (state: AuthState) => {
      state.isInitialized = true;
    }
  },
  extraReducers: (builder: ActionReducerMapBuilder<AuthState>) => {
    builder
      // Login
      .addCase(loginUser.pending, (state: AuthState) => {
        state.isLoading = true;
        state.isError = false;
        state.errorMessage = '';
      })
      .addCase(loginUser.fulfilled, (state: AuthState, action: PayloadAction<AuthResponse>) => {
        console.log('=== AUTH SLICE LOGIN FULFILLED ===');
        console.log('Setting user in Redux state:', action.payload);
        state.isLoading = false;
        state.isSuccess = true;
        state.user = action.payload;
        state.isInitialized = true;
        
        // Store user data in localStorage for persistence
        localStorage.setItem('user', JSON.stringify(action.payload));
        localStorage.setItem('accessToken', action.payload.accessToken);
        localStorage.setItem('refreshToken', action.payload.refreshToken);
      })
      .addCase(loginUser.rejected, (state: AuthState, action: any) => {
        state.isLoading = false;
        state.isError = true;
        state.errorMessage = action.payload || 'Login failed';
        state.user = null;
        state.isInitialized = true;
      })
      // Register
      .addCase(registerUser.pending, (state: AuthState) => {
        state.isLoading = true;
        state.isError = false;
        state.errorMessage = '';
      })
      .addCase(registerUser.fulfilled, (state: AuthState, action: PayloadAction<AuthResponse>) => {
        state.isLoading = false;
        state.isSuccess = true;
        state.user = action.payload;
        state.isInitialized = true;
      })
      .addCase(registerUser.rejected, (state: AuthState, action: any) => {
        state.isLoading = false;
        state.isError = true;
        state.errorMessage = action.payload || 'Registration failed';
        state.user = null;
        state.isInitialized = true;
      })
      // Logout
      .addCase(logoutUser.fulfilled, (state: AuthState) => {
        state.user = null;
        state.isSuccess = false;
        state.isError = false;
        state.errorMessage = '';
        state.isInitialized = true;
        
        // Clear localStorage
        localStorage.removeItem('user');
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
      })
      // Refresh Token
      .addCase(refreshUserToken.fulfilled, (state: AuthState, action: PayloadAction<{ accessToken: string; refreshToken?: string }>) => {
        if (state.user) {
          // Extract user information from the new JWT token
          const decodedUser = extractUserFromJWT(action.payload.accessToken);
          console.log('Decoded user from refreshed JWT:', decodedUser);
          
          state.user = { 
            ...state.user, 
            id: decodedUser?.id || state.user.id,
            username: decodedUser?.username || state.user.username,
            email: decodedUser?.email || state.user.email,
            roles: decodedUser?.role ? [decodedUser.role] : state.user.roles,
            accessToken: action.payload.accessToken,
            refreshToken: action.payload.refreshToken || state.user.refreshToken
          };
          
          // Update localStorage with new tokens and user data
          localStorage.setItem('accessToken', action.payload.accessToken);
          if (action.payload.refreshToken) {
            localStorage.setItem('refreshToken', action.payload.refreshToken);
          }
          localStorage.setItem('user', JSON.stringify(state.user));
        }
      })
      .addCase(refreshUserToken.rejected, (state: AuthState) => {
        // Clear user and mark as initialized when refresh fails
        state.user = null;
        state.isSuccess = false;
        state.isError = false;
        state.errorMessage = '';
        state.isInitialized = true;
        
        // Clear localStorage
        localStorage.removeItem('user');
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
      });
  },
});

export const { reset, clearUser, updateUserDetails, setInitialized } = authSlice.actions;
export default authSlice.reducer; 