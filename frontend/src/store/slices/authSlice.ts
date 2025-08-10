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
    resetAuthState: (state: AuthState) => {
      state.user = null;
      state.isLoading = false;
      state.isSuccess = false;
      state.isError = false;
      state.errorMessage = '';
      state.isInitialized = false;
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
    setUser: (state: AuthState, action: PayloadAction<AuthResponse>) => {
      // Ensure roles is always an array
      const userData = action.payload;
      if (userData.roles && !Array.isArray(userData.roles)) {
        // Handle case where roles might be stored as a single string
        userData.roles = [userData.roles as any];
      } else if (!userData.roles) {
        userData.roles = [];
      }
      
      state.user = userData;
      state.isSuccess = true;
      state.isError = false;
      state.errorMessage = '';
    },
    setInitialized: (state: AuthState) => {
      state.isInitialized = true;
    }
  },
  extraReducers: (builder: ActionReducerMapBuilder<AuthState>) => {
    builder
      // Login
      .addCase(loginUser.pending, (state: AuthState) => {
        console.log('=== AUTH SLICE LOGIN PENDING ===');
        state.isLoading = true;
        state.isError = false;
        state.errorMessage = '';
        state.isSuccess = false; // Reset success state on new login attempt
      })
      .addCase(loginUser.fulfilled, (state: AuthState, action: PayloadAction<AuthResponse>) => {
        console.log('=== AUTH SLICE LOGIN FULFILLED ===');
        console.log('Setting user in Redux state:', action.payload);
        state.isLoading = false;
        state.user = action.payload;
        state.isInitialized = true;
        
        // Store user data in localStorage for persistence if we have a valid token
        if (action.payload.accessToken && action.payload.accessToken.length > 0) {
          console.log('Login successful - storing user data in localStorage');
          localStorage.setItem('user', JSON.stringify(action.payload));
          localStorage.setItem('accessToken', action.payload.accessToken);
          localStorage.setItem('refreshToken', action.payload.refreshToken);
          
          // Set success if no 2FA is required
          if (!action.payload.requiresTwoFactor) {
            console.log('Login fully successful - setting isSuccess to true');
            state.isSuccess = true;
          } else {
            console.log('2FA required - keeping isSuccess as false');
            state.isSuccess = false;
          }
        } else {
          console.log('No valid token - not storing in localStorage');
          state.isSuccess = false;
        }
      })
      .addCase(loginUser.rejected, (state: AuthState, action: any) => {
        console.log('=== AUTH SLICE LOGIN REJECTED ===');
        console.log('Action payload:', action.payload);
        console.log('Action error:', action.error);
        
        state.isLoading = false;
        state.isError = true;
        state.errorMessage = action.payload || 'Login failed';
        state.user = null;
        state.isInitialized = true;
        
        console.log('Updated state:', {
          isError: state.isError,
          errorMessage: state.errorMessage,
          user: state.user
        });
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
            roles: decodedUser?.role ? [decodedUser.role] : (state.user.roles || []),
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

export const { reset, clearUser, updateUserDetails, setUser, setInitialized, resetAuthState } = authSlice.actions;
export default authSlice.reducer; 