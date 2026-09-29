import axiosInstance from "@/lib/api/axiosConfig";

export interface ModelModerationQueueItem {
  id: string;
  name: string;
  description: string;
  thumbnailUrl?: string;
  userName: string;
  fileFormat: string;
  createdAt: string;
  status: number;
}

export interface ModelsAwaitingModerationResponse {
  items: ModelModerationQueueItem[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export async function getModelsAwaitingModeration(
  page = 1,
  pageSize = 20
): Promise<ModelsAwaitingModerationResponse> {
  const { data } = await axiosInstance.get<ModelsAwaitingModerationResponse>(
    "/api/moderation/models",
    { params: { page, pageSize } }
  );
  return data;
}

export async function approveModel(modelId: string): Promise<void> {
  await axiosInstance.post(`/api/moderation/models/${modelId}/approve`);
}

export async function rejectModel(modelId: string, reason?: string): Promise<void> {
  await axiosInstance.post(`/api/moderation/models/${modelId}/reject`, { reason });
}
