import { API_CONFIG } from '../api/config';
import { AxiosHttpClient } from '../api/axiosAdapter';
import {
  GetCategoriesClient,
  CreateCategoryClient,
  UpdateCategoryClient,
  DeleteCategoryClient,
  CreateCategoryCommand,
  UpdateCategoryCommand,
  GetCategoriesResponse,
  CreateCategoryResponse,
  UpdateCategoryResponse,
  DeleteCategoryResponse
} from './api.client';

const sharedHttpClient = new AxiosHttpClient(API_CONFIG.baseUrl);

export interface CategoryDto {
  id: string;
  name: string;
  createdAt: string;
  createdBy: string;
  updatedAt: string;
  updatedBy: string;
}

export interface CreateCategoryRequest {
  name: string;
}

export interface UpdateCategoryRequest {
  id: string;
  name: string;
}

export interface GetCategoriesRequest {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
}

class CategoryService {
  async getCategories(params?: GetCategoriesRequest): Promise<GetCategoriesResponse> {
    const client = new GetCategoriesClient(API_CONFIG.baseUrl, sharedHttpClient);
    const response = await client.getCategories(
      params?.page || 1,
      params?.pageSize || 20,
      params?.searchTerm || null
    );
    return response;
  }

  async createCategory(request: CreateCategoryRequest): Promise<CreateCategoryResponse> {
    const client = new CreateCategoryClient(API_CONFIG.baseUrl, sharedHttpClient);
    const command = new CreateCategoryCommand({
      name: request.name
    });
    const response = await client.createCategory(command);
    return response;
  }

  async updateCategory(request: UpdateCategoryRequest): Promise<UpdateCategoryResponse> {
    const client = new UpdateCategoryClient(API_CONFIG.baseUrl, sharedHttpClient);
    const command = new UpdateCategoryCommand({
      name: request.name
    });
    const response = await client.updateCategory(request.id, command);
    return response;
  }

  async deleteCategory(id: string): Promise<DeleteCategoryResponse> {
    const client = new DeleteCategoryClient(API_CONFIG.baseUrl, sharedHttpClient);
    const response = await client.deleteCategory(id);
    return response;
  }
}

export const categoryService = new CategoryService();
export default categoryService;
