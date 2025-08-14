import axios from 'axios';
import store from '../store';

const API_URL = import.meta.env.VITE_API_URL ? `${import.meta.env.VITE_API_URL}/api/admin/roles` : 'http://localhost:11666/api/admin/roles';

export interface RoleDto {
  id: string;
  name: string;
  description: string;
  isSystemRole: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateRoleRequest {
  name: string;
  description: string;
  color?: string;
}

export interface UpdateRoleRequest {
  name: string;
  description: string;
  color?: string;
}

const getAuthHeaders = () => {
  const state = store.getState();
  const user = state.auth.user;
  
  if (user?.accessToken) {
    return {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${user.accessToken}`
    };
  }
  throw new Error('No access token found in Redux store');
};

const getAllRoles = async (): Promise<RoleDto[]> => {
  try {
    const headers = getAuthHeaders();
    console.log('Fetching roles with headers:', headers);
    
    const response = await axios.get(`${API_URL}`, { headers });
    return response.data;
  } catch (error) {
    console.error('Error fetching roles:', error);
    if (axios.isAxiosError(error) && error.response) {
      console.error('Response status:', error.response.status);
      console.error('Response data:', error.response.data);
      
      if (error.response.status === 401) {
        localStorage.removeItem('user');
      } else if (error.response.status === 403) {
        console.warn('Permission denied. User is authenticated but lacks required permissions.');
      }
    }
    throw error;
  }
};

const getRoleById = async (id: string): Promise<RoleDto> => {
  try {
    const headers = getAuthHeaders();
    const response = await axios.get(`${API_URL}/${id}`, { headers });
    return response.data;
  } catch (error) {
    console.error(`Error fetching role with id ${id}:`, error);
    throw error;
  }
};

const createRole = async (role: CreateRoleRequest): Promise<RoleDto> => {
  try {
    const headers = getAuthHeaders();
    const response = await axios.post(`${API_URL}`, role, { headers });
    return response.data;
  } catch (error) {
    console.error('Error creating role:', error);
    throw error;
  }
};

const updateRole = async (id: string, role: UpdateRoleRequest): Promise<RoleDto> => {
  try {
    const headers = getAuthHeaders();
    const response = await axios.put(`${API_URL}/${id}`, role, { headers });
    return response.data;
  } catch (error) {
    console.error(`Error updating role with id ${id}:`, error);
    throw error;
  }
};

const deleteRole = async (id: string): Promise<void> => {
  try {
    const headers = getAuthHeaders();
    await axios.delete(`${API_URL}/${id}`, { headers });
  } catch (error) {
    console.error(`Error deleting role with id ${id}:`, error);
    throw error;
  }
};

const roleService = {
  getAllRoles,
  getRoleById,
  createRole,
  updateRole,
  deleteRole
};

export default roleService;