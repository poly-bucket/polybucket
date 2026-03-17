import { getApiConfig } from "@/lib/api/config";
import { ApiClientFactory } from "@/lib/api/clientFactory";
import type {
  PrivacySettings,
  GetModelsResponse,
  GetModelByIdResponse,
} from "@/lib/api/client";

const AUTH_STORAGE_KEY = "polybucket-auth";

export const RECENT_MODELS_PAGE_SIZE = 12;
export const RECENT_MODELS_SKELETON_COUNT = 8;

export interface ModelUploadData {
  name: string;
  description?: string;
  privacy: PrivacySettings;
  license?: string;
  categories?: string[];
  aiGenerated: boolean;
  workInProgress: boolean;
  nsfw: boolean;
  remix: boolean;
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

export async function uploadModel(request: ModelUploadRequest): Promise<UploadedModel> {
  const token = getAccessToken();
  if (!token) {
    throw new Error("Authentication token not found");
  }

  const formData = new FormData();
  formData.append("Name", request.modelData.name);

  if (request.modelData.description) {
    formData.append("Description", request.modelData.description);
  }

  request.files.forEach((file) => {
    formData.append("Files", file);
  });

  if (request.modelData.thumbnailFileId) {
    formData.append("ThumbnailFileId", request.modelData.thumbnailFileId);
  }

  if (request.modelData.privacy !== undefined) {
    formData.append(
      "Privacy",
      String(request.modelData.privacy).toLowerCase()
    );
  }

  if (request.modelData.license) {
    formData.append("License", request.modelData.license);
  }

  formData.append(
    "AIGenerated",
    String(request.modelData.aiGenerated)
  );
  formData.append(
    "WorkInProgress",
    String(request.modelData.workInProgress)
  );
  formData.append("NSFW", String(request.modelData.nsfw));
  formData.append("Remix", String(request.modelData.remix));

  const { baseUrl } = getApiConfig();
  const response = await fetch(`${baseUrl}/api/models`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${token}`,
    },
    body: formData,
  });

  if (!response.ok) {
    let errorMessage = `Upload failed: ${response.statusText}`;
    try {
      const errorText = await response.text();
      if (errorText) {
        try {
          const errorData = JSON.parse(errorText);
          errorMessage =
            errorData.message ||
            errorData.detail ||
            errorData.title ||
            errorText;
        } catch {
          errorMessage = errorText;
        }
      }
    } catch {
      // ignore
    }
    const error = new Error(errorMessage);
    (error as Error & { status?: number }).status = response.status;
    throw error;
  }

  const result = await response.json();
  const model = result.model ?? result;
  return {
    id: model.id,
    name: model.name,
    description: model.description,
  };
}
