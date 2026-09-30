import axiosInstance from "@/lib/api/axiosConfig";

export interface ModelViewResult {
  views: number;
  counted: boolean;
}

export function modelViewSessionKey(modelId: string): string {
  return `model-view:${modelId}`;
}

export async function recordModelView(modelId: string): Promise<ModelViewResult> {
  const { data } = await axiosInstance.post<ModelViewResult>(
    `/api/models/${modelId}/view`
  );
  return data;
}
