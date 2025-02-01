import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import api from '../../services/api';
import { RootState } from '../store';
import { selectCurrentUserId } from './auth-slice';

export interface UserDetails {
  id: string;
  email: string;
  username: string;
  createdAt: string;
  // Add any other user properties from your API
}

export interface UserState {
  details: UserDetails | null;
  loading: boolean;
  error: string | null;
}

const initialState: UserState = {
  details: null,
  loading: false,
  error: null,
};

export const fetchUserDetails = createAsyncThunk(
  'user/fetchDetails',
  async (_, { getState }) => {
    const state = getState() as RootState;
    const token = state.auth.token;
    const userId = selectCurrentUserId(state);
    
    if (!token || !userId) {
      throw new Error('No authentication token or user ID found');
    }

    const response = await api.get(`/Users/${userId}`);
    return response.data;
  }
);

const userSlice = createSlice({
  name: 'user',
  initialState,
  reducers: {
    clearUser: (state) => {
      state.details = null;
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchUserDetails.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchUserDetails.fulfilled, (state, action: PayloadAction<UserDetails>) => {
        state.loading = false;
        state.details = action.payload;
      })
      .addCase(fetchUserDetails.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Failed to fetch user details';
      });
  },
});

export const { clearUser } = userSlice.actions;
export default userSlice.reducer; 