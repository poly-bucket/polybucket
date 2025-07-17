import { createSlice, createAsyncThunk, PayloadAction, ActionReducerMapBuilder } from '@reduxjs/toolkit';
import authService, { 
  RegisterRequest, 
  AuthResponse,
  CheckFirstRunResponse,
  SystemSetupStatus
} from '../../services/authService';
import apiAuthService from '../../services/apiAuthService';
import { LoginRequest, LoginResponse } from '../../services/api.client';

// Define the initial state
interface AuthState {
  user: AuthResponse | null;
  isFirstRun: boolean;
  setupStatus: SystemSetupStatus | null;
  isLoading: boolean;
  isSuccess: boolean;
  isError: boolean;
  errorMessage: string;
}

const user = authService.getCurrentUser();

const initialState: AuthState = {
  user: user,
  isFirstRun: false,
  setupStatus: null,
  isLoading: false,
  isSuccess: false,
  isError: false,
  errorMessage: ''
};

// Async thunks
export const checkFirstRun = createAsyncThunk(
  'auth/checkFirstRun',
  async (_: void, { rejectWithValue }: { rejectWithValue: (value: string) => any }) => {
    try {
      return await authService.checkFirstRun();
    } catch (error: any) {
      const message = error.response?.data?.message || error.message || 'Failed to check first run status';
      return rejectWithValue(message);
    }
  }
);

export const getSetupStatus = createAsyncThunk(
  'auth/getSetupStatus',
  async (_: void, { rejectWithValue }: { rejectWithValue: (value: string) => any }) => {
    try {
      return await authService.getSetupStatus();
    } catch (error: any) {
      const message = error.response?.data?.message || error.message || 'Failed to get setup status';
      return rejectWithValue(message);
    }
  }
);

export const setAdminConfigured = createAsyncThunk(
  'auth/setAdminConfigured',
  async (isConfigured: boolean, { rejectWithValue }: { rejectWithValue: (value: string) => any }) => {
    try {
      return {
        success: await authService.setAdminConfigured(isConfigured),
        isConfigured
      };
    } catch (error: any) {
      const message = error.response?.data?.message || error.message || 'Failed to set admin configuration status';
      return rejectWithValue(message);
    }
  }
);

export const setRoleConfigured = createAsyncThunk(
  'auth/setRoleConfigured',
  async (isConfigured: boolean, { rejectWithValue }: { rejectWithValue: (value: string) => any }) => {
    try {
      return {
        success: await authService.setRoleConfigured(isConfigured),
        isConfigured
      };
    } catch (error: any) {
      const message = error.response?.data?.message || error.message || 'Failed to set role configuration status';
      return rejectWithValue(message);
    }
  }
);

export const setModerationConfigured = createAsyncThunk(
  'auth/setModerationConfigured',
  async (isConfigured: boolean, { rejectWithValue }: { rejectWithValue: (value: string) => any }) => {
    try {
      return {
        success: await authService.setModerationConfigured(isConfigured),
        isConfigured
      };
    } catch (error: any) {
      const message = error.response?.data?.message || error.message || 'Failed to set moderation configuration status';
      return rejectWithValue(message);
    }
  }
);

export const login = createAsyncThunk(
  'auth/login',
  async (credentials: { emailOrUsername: string; password: string }, { rejectWithValue }: { rejectWithValue: (value: string) => any }) => {
    try {
      // Convert to the format expected by apiAuthService
      const apiCredentials = new LoginRequest({
        email: credentials.emailOrUsername,
        password: credentials.password
      });
      
      const response = await apiAuthService.login(apiCredentials);
      
      // The login response only contains a token, so we need to construct a user object
      // We'll use the credentials to create a basic user object
      const userData: AuthResponse = {
        id: '', // We don't have the user ID from login, but we can get it later if needed
        username: credentials.emailOrUsername, // Use the email/username as username for now
        email: credentials.emailOrUsername.includes('@') ? credentials.emailOrUsername : '',
        accessToken: response.token || '',
        refreshToken: '', // We don't have refresh token from this login flow
        roles: ['Admin'] // Assume admin for now since this is the admin setup flow
      };
      
      return userData;
    } catch (error: any) {
      const message = error.response?.data?.message || error.message || 'Failed to login';
      return rejectWithValue(message);
    }
  }
);

export const register = createAsyncThunk(
  'auth/register',
  async (userData: RegisterRequest, { rejectWithValue }: { rejectWithValue: (value: string) => any }) => {
    try {
      return await authService.register(userData);
    } catch (error: any) {
      // Extract detailed error message if available from axios response
      let errorMessage = 'Failed to register';
      
      if (error.response?.data) {
        // Check for different error formats from backend
        if (typeof error.response.data === 'string') {
          errorMessage = error.response.data;
        } else if (error.response.data.message) {
          errorMessage = error.response.data.message;
        } else if (error.response.data.errors) {
          // Handle validation errors
          const errors = error.response.data.errors;
          if (Array.isArray(errors)) {
            errorMessage = errors.join(', ');
          } else if (typeof errors === 'object') {
            errorMessage = Object.values(errors).flat().join(', ');
          }
        }
      } else if (error.message) {
        errorMessage = error.message;
      }
      
      console.error('Registration error details:', {
        message: errorMessage,
        response: error.response?.data,
        status: error.response?.status
      });
      
      return rejectWithValue(errorMessage);
    }
  }
);

export const adminSetup = createAsyncThunk(
  'auth/adminSetup',
  async (userData: RegisterRequest, { rejectWithValue }: { rejectWithValue: (value: string) => any }) => {
    try {
      return await authService.adminSetup(userData);
    } catch (error: any) {
      // Extract detailed error message if available from axios response
      let errorMessage = 'Failed to create admin account';
      
      if (error.response?.data) {
        // Check for different error formats from backend
        if (typeof error.response.data === 'string') {
          errorMessage = error.response.data;
        } else if (error.response.data.message) {
          errorMessage = error.response.data.message;
        } else if (error.response.data.errors) {
          // Handle validation errors
          const errors = error.response.data.errors;
          if (Array.isArray(errors)) {
            errorMessage = errors.join(', ');
          } else if (typeof errors === 'object') {
            errorMessage = Object.values(errors).flat().join(', ');
          }
        }
      } else if (error.message) {
        errorMessage = error.message;
      }
      
      console.error('Admin setup error details:', {
        message: errorMessage,
        response: error.response?.data,
        status: error.response?.status
      });
      
      return rejectWithValue(errorMessage);
    }
  }
);

export const logout = createAsyncThunk('auth/logout', async () => {
  authService.logout();
});

export const refreshToken = createAsyncThunk(
  'auth/refreshToken',
  async (_: void, { rejectWithValue }: { rejectWithValue: (value: string) => any }) => {
    try {
      return await authService.refreshToken();
    } catch (error: any) {
      const message = error.response?.data?.message || error.message || 'Failed to refresh token';
      return rejectWithValue(message);
    }
  }
);

// Create the auth slice
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
  },
  extraReducers: (builder: ActionReducerMapBuilder<AuthState>) => {
    builder
      // Check First Run
      .addCase(checkFirstRun.pending, (state: AuthState) => {
        state.isLoading = true;
      })
      .addCase(checkFirstRun.fulfilled, (state: AuthState, action: PayloadAction<CheckFirstRunResponse>) => {
        state.isLoading = false;
        state.isSuccess = true;
        state.isFirstRun = action.payload.isFirstRun;
      })
      .addCase(checkFirstRun.rejected, (state: AuthState, action: PayloadAction<string>) => {
        state.isLoading = false;
        state.isError = true;
        state.errorMessage = action.payload;
      })
      // Get Setup Status
      .addCase(getSetupStatus.pending, (state: AuthState) => {
        state.isLoading = true;
      })
      .addCase(getSetupStatus.fulfilled, (state: AuthState, action: PayloadAction<SystemSetupStatus>) => {
        state.isLoading = false;
        state.isSuccess = true;
        state.setupStatus = action.payload;
        // Update isFirstRun based on setup status
        state.isFirstRun = !action.payload.isAdminConfigured || !action.payload.isRoleConfigured;
      })
      .addCase(getSetupStatus.rejected, (state: AuthState, action: PayloadAction<string>) => {
        state.isLoading = false;
        state.isError = true;
        state.errorMessage = action.payload;
      })
      // Set Admin Configured
      .addCase(setAdminConfigured.pending, (state: AuthState) => {
        state.isLoading = true;
      })
      .addCase(setAdminConfigured.fulfilled, (state: AuthState, action: PayloadAction<{success: boolean, isConfigured: boolean}>) => {
        state.isLoading = false;
        state.isSuccess = true;
        if (state.setupStatus) {
          state.setupStatus.isAdminConfigured = action.payload.isConfigured;
          // Update isFirstRun based on setup status
          state.isFirstRun = !state.setupStatus.isAdminConfigured || !state.setupStatus.isRoleConfigured;
        }
      })
      .addCase(setAdminConfigured.rejected, (state: AuthState, action: PayloadAction<string>) => {
        state.isLoading = false;
        state.isError = true;
        state.errorMessage = action.payload;
      })
      // Set Role Configured
      .addCase(setRoleConfigured.pending, (state: AuthState) => {
        state.isLoading = true;
      })
      .addCase(setRoleConfigured.fulfilled, (state: AuthState, action: PayloadAction<{success: boolean, isConfigured: boolean}>) => {
        state.isLoading = false;
        state.isSuccess = true;
        if (state.setupStatus) {
          state.setupStatus.isRoleConfigured = action.payload.isConfigured;
          // Update isFirstRun based on setup status
          state.isFirstRun = !state.setupStatus.isAdminConfigured || !state.setupStatus.isRoleConfigured;
        }
      })
      .addCase(setRoleConfigured.rejected, (state: AuthState, action: PayloadAction<string>) => {
        state.isLoading = false;
        state.isError = true;
        state.errorMessage = action.payload;
      })
      // Set Moderation Configured
      .addCase(setModerationConfigured.pending, (state: AuthState) => {
        state.isLoading = true;
      })
      .addCase(setModerationConfigured.fulfilled, (state: AuthState, action: PayloadAction<{success: boolean, isConfigured: boolean}>) => {
        state.isLoading = false;
        state.isSuccess = true;
        if (state.setupStatus) {
          state.setupStatus.isModerationConfigured = action.payload.isConfigured;
        }
      })
      .addCase(setModerationConfigured.rejected, (state: AuthState, action: PayloadAction<string>) => {
        state.isLoading = false;
        state.isError = true;
        state.errorMessage = action.payload;
      })
      // Login
      .addCase(login.pending, (state: AuthState) => {
        state.isLoading = true;
      })
      .addCase(login.fulfilled, (state: AuthState, action: PayloadAction<AuthResponse>) => {
        state.isLoading = false;
        state.isSuccess = true;
        state.user = action.payload;
      })
      .addCase(login.rejected, (state: AuthState, action: any) => {
        state.isLoading = false;
        state.isError = true;
        state.errorMessage = action.payload;
        state.user = null;
      })
      // Register
      .addCase(register.pending, (state: AuthState) => {
        state.isLoading = true;
      })
      .addCase(register.fulfilled, (state: AuthState, action: PayloadAction<AuthResponse>) => {
        state.isLoading = false;
        state.isSuccess = true;
        state.user = action.payload;
      })
      .addCase(register.rejected, (state: AuthState, action: PayloadAction<string>) => {
        state.isLoading = false;
        state.isError = true;
        state.errorMessage = action.payload;
        state.user = null;
      })
      // Admin Setup
      .addCase(adminSetup.pending, (state: AuthState) => {
        state.isLoading = true;
      })
      .addCase(adminSetup.fulfilled, (state: AuthState, action: PayloadAction<AuthResponse>) => {
        state.isLoading = false;
        state.isSuccess = true;
        state.user = action.payload;
      })
      .addCase(adminSetup.rejected, (state: AuthState, action: PayloadAction<string>) => {
        state.isLoading = false;
        state.isError = true;
        state.errorMessage = action.payload;
        state.user = null;
      })
      // Logout
      .addCase(logout.fulfilled, (state: AuthState) => {
        state.user = null;
      })
      // Refresh Token
      .addCase(refreshToken.fulfilled, (state: AuthState, action: PayloadAction<AuthResponse>) => {
        state.user = action.payload;
      })
      .addCase(refreshToken.rejected, (state: AuthState) => {
        state.user = null;
      });
  },
});

export const { reset } = authSlice.actions;
export default authSlice.reducer; 