import api from '../utils/axiosConfig';

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

export interface GetCategoriesResponse {
  categories: CategoryDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface CreateCategoryResponse {
  id: string;
  name: string;
  createdAt: string;
  createdById: string;
}

export interface UpdateCategoryResponse {
  id: string;
  name: string;
  updatedAt: string;
  updatedById: string;
}

export interface DeleteCategoryResponse {
  id: string;
  name: string;
  success: boolean;
  message: string;
  deletedAt: string;
  deletedById: string;
}

class CategoryService {
  private readonly baseUrl = '/admin/categories';

  async getCategories(params?: GetCategoriesRequest): Promise<GetCategoriesResponse> {
    const response = await api.get<GetCategoriesResponse>(this.baseUrl, {
      params: {
        page: params?.page || 1,
        pageSize: params?.pageSize || 20,
        searchTerm: params?.searchTerm || undefined
      }
    });
    return response.data;
  }

  async createCategory(request: CreateCategoryRequest): Promise<CreateCategoryResponse> {
    const response = await api.post<CreateCategoryResponse>(this.baseUrl, request);
    return response.data;
  }

  async updateCategory(request: UpdateCategoryRequest): Promise<UpdateCategoryResponse> {
    const response = await api.put<UpdateCategoryResponse>(`${this.baseUrl}/${request.id}`, request);
    return response.data;
  }

  async deleteCategory(id: string): Promise<DeleteCategoryResponse> {
    const response = await api.delete<DeleteCategoryResponse>(`${this.baseUrl}/${id}`);
    return response.data;
  }
}

export const categoryService = new CategoryService();
export default categoryService;
