import { createSlice, createAsyncThunk, PayloadAction, ActionReducerMapBuilder } from '@reduxjs/toolkit';
import roleService, { RoleDto, CreateRoleRequest, UpdateRoleRequest } from '../../services/roleService';

// Define the initial state
interface RoleState {
  roles: RoleDto[];
  role: RoleDto | null;
  isLoading: boolean;
  isSuccess: boolean;
  isError: boolean;
  errorMessage: string;
}

const initialState: RoleState = {
  roles: [],
  role: null,
  isLoading: false,
  isSuccess: false,
  isError: false,
  errorMessage: ''
};

// Async thunks
export const getAllRoles = createAsyncThunk(
  'roles/getAll',
  async (_: void, { rejectWithValue }: { rejectWithValue: (value: string) => any }) => {
    try {
      return await roleService.getAllRoles();
    } catch (error: any) {
      const message = error.response?.data?.message || error.message || 'Failed to fetch roles';
      return rejectWithValue(message);
    }
  }
);

export const getRoleById = createAsyncThunk(
  'roles/getById',
  async (id: string, { rejectWithValue }: { rejectWithValue: (value: string) => any }) => {
    try {
      return await roleService.getRoleById(id);
    } catch (error: any) {
      const message = error.response?.data?.message || error.message || 'Failed to fetch role';
      return rejectWithValue(message);
    }
  }
);

export const createRole = createAsyncThunk(
  'roles/create',
  async (role: CreateRoleRequest, { rejectWithValue }: { rejectWithValue: (value: string) => any }) => {
    try {
      return await roleService.createRole(role);
    } catch (error: any) {
      const message = error.response?.data?.message || error.message || 'Failed to create role';
      return rejectWithValue(message);
    }
  }
);

export const updateRole = createAsyncThunk(
  'roles/update',
  async ({ id, role }: { id: string, role: UpdateRoleRequest }, { rejectWithValue }: { rejectWithValue: (value: string) => any }) => {
    try {
      return await roleService.updateRole(id, role);
    } catch (error: any) {
      const message = error.response?.data?.message || error.message || 'Failed to update role';
      return rejectWithValue(message);
    }
  }
);

export const deleteRole = createAsyncThunk(
  'roles/delete',
  async (id: string, { rejectWithValue }: { rejectWithValue: (value: string) => any }) => {
    try {
      await roleService.deleteRole(id);
      return id;
    } catch (error: any) {
      const message = error.response?.data?.message || error.message || 'Failed to delete role';
      return rejectWithValue(message);
    }
  }
);

// Create the role slice
const roleSlice = createSlice({
  name: 'roles',
  initialState,
  reducers: {
    reset: (state: RoleState) => {
      state.isLoading = false;
      state.isSuccess = false;
      state.isError = false;
      state.errorMessage = '';
    },
    clearRole: (state: RoleState) => {
      state.role = null;
    }
  },
  extraReducers: (builder: ActionReducerMapBuilder<RoleState>) => {
    builder
      // Get All Roles
      .addCase(getAllRoles.pending, (state: RoleState) => {
        state.isLoading = true;
      })
      .addCase(getAllRoles.fulfilled, (state: RoleState, action: PayloadAction<RoleDto[]>) => {
        state.isLoading = false;
        state.isSuccess = true;
        state.roles = action.payload;
      })
      .addCase(getAllRoles.rejected, (state: RoleState, action: PayloadAction<any>) => {
        state.isLoading = false;
        state.isError = true;
        state.errorMessage = action.payload;
      })
      // Get Role By Id
      .addCase(getRoleById.pending, (state: RoleState) => {
        state.isLoading = true;
      })
      .addCase(getRoleById.fulfilled, (state: RoleState, action: PayloadAction<RoleDto>) => {
        state.isLoading = false;
        state.isSuccess = true;
        state.role = action.payload;
      })
      .addCase(getRoleById.rejected, (state: RoleState, action: PayloadAction<any>) => {
        state.isLoading = false;
        state.isError = true;
        state.errorMessage = action.payload;
      })
      // Create Role
      .addCase(createRole.pending, (state: RoleState) => {
        state.isLoading = true;
      })
      .addCase(createRole.fulfilled, (state: RoleState, action: PayloadAction<RoleDto>) => {
        state.isLoading = false;
        state.isSuccess = true;
        state.roles.push(action.payload);
      })
      .addCase(createRole.rejected, (state: RoleState, action: PayloadAction<any>) => {
        state.isLoading = false;
        state.isError = true;
        state.errorMessage = action.payload;
      })
      // Update Role
      .addCase(updateRole.pending, (state: RoleState) => {
        state.isLoading = true;
      })
      .addCase(updateRole.fulfilled, (state: RoleState, action: PayloadAction<RoleDto>) => {
        state.isLoading = false;
        state.isSuccess = true;
        const index = state.roles.findIndex(role => role.id === action.payload.id);
        if (index !== -1) {
          state.roles[index] = action.payload;
        }
      })
      .addCase(updateRole.rejected, (state: RoleState, action: PayloadAction<any>) => {
        state.isLoading = false;
        state.isError = true;
        state.errorMessage = action.payload;
      })
      // Delete Role
      .addCase(deleteRole.pending, (state: RoleState) => {
        state.isLoading = true;
      })
      .addCase(deleteRole.fulfilled, (state: RoleState, action: PayloadAction<string>) => {
        state.isLoading = false;
        state.isSuccess = true;
        state.roles = state.roles.filter(role => role.id !== action.payload);
      })
      .addCase(deleteRole.rejected, (state: RoleState, action: PayloadAction<any>) => {
        state.isLoading = false;
        state.isError = true;
        state.errorMessage = action.payload;
      });
  },
});

export const { reset, clearRole } = roleSlice.actions;
export default roleSlice.reducer;