import axiosInstance from "@/lib/api/axiosConfig";
import type { UserLikedModelListItemDto } from "@/lib/api/client";

export interface UserLikedModelsPage {
  models: UserLikedModelListItemDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export async function fetchUserLikedModels(
  username: string,
  page: number,
  pageSize: number
): Promise<UserLikedModelsPage> {
  const { data } = await axiosInstance.get<UserLikedModelsPage>(
    `/api/users/profile/${encodeURIComponent(username)}/liked-models`,
    { params: { page, pageSize } }
  );
  return {
    models: data.models ?? [],
    totalCount: data.totalCount ?? 0,
    page: data.page ?? page,
    pageSize: data.pageSize ?? pageSize,
    totalPages: data.totalPages ?? 1,
  };
}
