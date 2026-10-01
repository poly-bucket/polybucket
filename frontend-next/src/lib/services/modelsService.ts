import { ApiClientFactory } from "@/lib/api/clientFactory";
import type {
  FileParameter,
  PrivacySettings,
  GetModelsResponse,
  GetModelByIdResponse,
  ApiException,
  LicenseTypes,
} from "@/lib/api/client";
import { UpdateModelRequest } from "@/lib/api/client";
import { AUTH_STORAGE_KEY } from "@/lib/auth/authConstants";
import type { ModelMetadataFormValues } from "@/lib/models/model-metadata";

export const RECENT_MODELS_PAGE_SIZE = 12;
export const RECENT_MODELS_SKELETON_COUNT = 8;

export interface ModelUploadData {
  name: string;
  description?: string;
  privacy: PrivacySettings;
  license: LicenseTypes;
  categories?: string[];
  aiGenerated: boolean;
  workInProgress: boolean;
  nsfw: boolean;
  remix: boolean;
  remixUrl?: string;
  thumbnailFileId?: string;
}

export interface ModelUploadRequest {
  modelData: ModelUploadData;
  files: File[];
}

export interface UploadedModel {
  id: string;
  name?: string;
  description?: string;
}

export async function fetchRecentModels(): Promise<GetModelsResponse> {
  const client = ApiClientFactory.getApiClient();
  return client.getModels_GetModels(1, RECENT_MODELS_PAGE_SIZE, "createdAt");
}

export async function fetchModelById(
  id: string
): Promise<GetModelByIdResponse> {
  const client = ApiClientFactory.getApiClient();
  return client.getModelById_GetModel(id);
}

function getAccessToken(): string | null {
  if (typeof window === "undefined") return null;
  try {
    const stored = localStorage.getItem(AUTH_STORAGE_KEY);
    if (!stored) return null;
    const parsed = JSON.parse(stored) as { accessToken?: string };
    return parsed.accessToken ?? null;
  } catch {
    return null;
  }
}

export async function linkModelCategories(
  modelId: string,
  categoryNames: string[]
): Promise<{ failed: number; skipped: number }> {
  if (!modelId || categoryNames.length === 0) {
    return { failed: 0, skipped: 0 };
  }

  const client = ApiClientFactory.getApiClient();
  const categoryResponse = await client.getCategories_GetCategories(1, 100, null);
  const nameToId = new Map(
    (categoryResponse.categories ?? []).map((c) => [c.name ?? "", c.id ?? ""])
  );

  let skipped = 0;
  const toLink = categoryNames.filter((name) => {
    if (!nameToId.get(name)) {
      skipped += 1;
      return false;
    }
    return true;
  });

  const results = await Promise.allSettled(
    toLink.map((name) =>
      client.addCategoryToModel_AddCategoryToModel(modelId, nameToId.get(name)!)
    )
  );

  const failed = results.filter((r) => r.status === "rejected").length;
  return { failed, skipped };
}

export async function applyPostCreateModelMetadata(
  modelId: string,
  metadata: Pick<ModelMetadataFormValues, "isRemix" | "remixUrl">
): Promise<void> {
  if (!metadata.isRemix || !metadata.remixUrl?.trim()) {
    return;
  }

  const client = ApiClientFactory.getApiClient();
  const request = UpdateModelRequest.fromJS({
    isRemix: true,
    remixUrl: metadata.remixUrl.trim(),
  });
  await client.updateModel_UpdateModel(modelId, request);
}

export async function uploadModel(request: ModelUploadRequest): Promise<UploadedModel> {
  const token = getAccessToken();
  if (!token) {
    throw new Error("Authentication token not found");
  }

  const client = ApiClientFactory.getApiClient();
  const files: FileParameter[] = request.files.map((file) => ({
    data: file,
    fileName: file.name,
  }));

  try {
    const response = await client.createModel_CreateModel(
      request.modelData.name,
      request.modelData.description ?? null,
      files,
      request.modelData.thumbnailFileId ?? null,
      request.modelData.privacy !== undefined
        ? String(request.modelData.privacy).toLowerCase()
        : null,
      request.modelData.license ?? null,
      request.modelData.aiGenerated,
      request.modelData.workInProgress,
      request.modelData.nsfw,
      request.modelData.remix
    );

    const model = response.model;
    const modelId = model?.id ?? "";

    if (modelId && request.modelData.remix && request.modelData.remixUrl?.trim()) {
      try {
        await applyPostCreateModelMetadata(modelId, {
          isRemix: request.modelData.remix,
          remixUrl: request.modelData.remixUrl ?? "",
        });
      } catch {
        // Model was created; remix URL can be edited later.
      }
    }

    return {
      id: modelId,
      name: model?.name,
      description: model?.description,
    };
  } catch (error) {
    const apiError = error as ApiException & { response?: string; status?: number };
    const rawResponse = typeof apiError.response === "string" ? apiError.response : "";
    let errorMessage = apiError.message || "Upload failed";

    if (rawResponse) {
      try {
        const parsed = JSON.parse(rawResponse) as {
          message?: string;
          detail?: string;
          title?: string;
        };
        errorMessage = parsed.message || parsed.detail || parsed.title || rawResponse;
      } catch {
        errorMessage = rawResponse;
      }
    }

    const normalizedError = new Error(errorMessage);
    (normalizedError as Error & { status?: number }).status = apiError.status;
    throw normalizedError;
  }
}
