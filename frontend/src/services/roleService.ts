import { API_CONFIG } from '../api/config';
import { AxiosHttpClient } from '../api/axiosAdapter';
import {
  RoleManagementClient,
  CreateRoleRequest,
  UpdateRoleRequest,
  RoleDto
} from './api.client';

const sharedHttpClient = new AxiosHttpClient(API_CONFIG.baseUrl);

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

const getAllRoles = async (): Promise<RoleDto[]> => {
  try {
    const client = new RoleManagementClient(API_CONFIG.baseUrl, sharedHttpClient);
    const response = await client.getAllRolesUnpaginated();
    return response;
  } catch (error) {
    console.error('Error fetching roles:', error);
    throw error;
  }
};

const getRoleById = async (id: string): Promise<RoleDto> => {
  try {
    const client = new RoleManagementClient(API_CONFIG.baseUrl, sharedHttpClient);
    const response = await client.getRole(id);
    return response;
  } catch (error) {
    console.error(`Error fetching role with id ${id}:`, error);
    throw error;
  }
};

const createRole = async (role: CreateRoleRequest): Promise<RoleDto> => {
  try {
    const client = new RoleManagementClient(API_CONFIG.baseUrl, sharedHttpClient);
    const request = new CreateRoleRequest({
      name: role.name,
      description: role.description,
      color: role.color
    });
    const response = await client.createRole(request);
    return response;
  } catch (error) {
    console.error('Error creating role:', error);
    throw error;
  }
};

const updateRole = async (id: string, role: UpdateRoleRequest): Promise<RoleDto> => {
  try {
    const client = new RoleManagementClient(API_CONFIG.baseUrl, sharedHttpClient);
    const request = new UpdateRoleRequest({
      name: role.name,
      description: role.description,
      color: role.color
    });
    const response = await client.updateRole(id, request);
    return response;
  } catch (error) {
    console.error(`Error updating role with id ${id}:`, error);
    throw error;
  }
};

const deleteRole = async (id: string): Promise<void> => {
  try {
    const client = new RoleManagementClient(API_CONFIG.baseUrl, sharedHttpClient);
    await client.deleteRole(id);
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
