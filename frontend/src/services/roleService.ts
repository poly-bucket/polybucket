import { ApiClientFactory } from '../api/clientFactory';
import {
  CreateRoleRequest,
  UpdateRoleRequest,
  RoleDto
} from '../api/client';

export type { RoleDto, CreateRoleRequest, UpdateRoleRequest };

export interface CreateRoleInput {
  name: string;
  description: string;
  color?: string;
}

export interface UpdateRoleInput {
  name: string;
  description: string;
  color?: string;
}

const api = () => ApiClientFactory.getApiClient();

const getAllRoles = async (): Promise<RoleDto[]> => {
  try {
    return await api().roleManagement_GetAllRolesUnpaginated();
  } catch (error) {
    console.error('Error fetching roles:', error);
    throw error;
  }
};

const getRoleById = async (id: string): Promise<RoleDto> => {
  try {
    return await api().roleManagement_GetRole(id);
  } catch (error) {
    console.error(`Error fetching role with id ${id}:`, error);
    throw error;
  }
};

const createRole = async (role: CreateRoleInput): Promise<RoleDto> => {
  try {
    const request = new CreateRoleRequest({
      name: role.name,
      description: role.description,
      color: role.color
    });
    return await api().roleManagement_CreateRole(request);
  } catch (error) {
    console.error('Error creating role:', error);
    throw error;
  }
};

const updateRole = async (id: string, role: UpdateRoleInput): Promise<RoleDto> => {
  try {
    const request = new UpdateRoleRequest({
      name: role.name,
      description: role.description,
      color: role.color
    });
    return await api().roleManagement_UpdateRole(id, request);
  } catch (error) {
    console.error(`Error updating role with id ${id}:`, error);
    throw error;
  }
};

const deleteRole = async (id: string): Promise<void> => {
  try {
    await api().roleManagement_DeleteRole(id);
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
