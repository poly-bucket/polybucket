import { ApiClientFactory } from '../api/clientFactory';
import {
  CreateCategoryCommand,
  UpdateCategoryCommand,
  GetCategoriesResponse,
  CreateCategoryResponse,
  UpdateCategoryResponse,
  DeleteCategoryResponse
} from '../api/client';

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

const api = () => ApiClientFactory.getApiClient();

class CategoryService {
  async getCategories(params?: GetCategoriesRequest): Promise<GetCategoriesResponse> {
    return api().getCategories_GetCategories(
      params?.page || 1,
      params?.pageSize || 20,
      params?.searchTerm || null
    );
  }

  async createCategory(request: CreateCategoryRequest): Promise<CreateCategoryResponse> {
    const command = new CreateCategoryCommand({ name: request.name });
    return api().createCategory_CreateCategory(command);
  }

  async updateCategory(request: UpdateCategoryRequest): Promise<UpdateCategoryResponse> {
    const command = new UpdateCategoryCommand({ name: request.name });
    return api().updateCategory_UpdateCategory(request.id, command);
  }

  async deleteCategory(id: string): Promise<DeleteCategoryResponse> {
    return api().deleteCategory_DeleteCategory(id);
  }
}

export const categoryService = new CategoryService();
export default categoryService;
