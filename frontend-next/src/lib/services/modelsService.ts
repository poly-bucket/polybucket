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
    let errorMessage = "Upload failed";
    let status: number | undefined;

    if (ApiException.isApiException(error)) {
      status = error.status;
      const rawResponse = typeof error.response === "string" ? error.response : "";
      errorMessage = error.message || errorMessage;
      if (rawResponse) {
        try {
          const parsed = JSON.parse(rawResponse) as {
            message?: string;
            detail?: string;
            title?: string;
          };
          errorMessage =
            parsed.message || parsed.detail || parsed.title || rawResponse;
        } catch {
          errorMessage = rawResponse;
        }
      }
    } else if (error && typeof error === "object") {
      const errObj = error as {
        status?: number;
        message?: string;
        detail?: string;
        title?: string;
      };
      status = errObj.status;
      errorMessage =
        errObj.detail ||
        errObj.message ||
        errObj.title ||
        errorMessage;
    } else if (error instanceof Error) {
      errorMessage = error.message;
    }

    // #region agent log
    fetch("http://127.0.0.1:7425/ingest/0e4469e7-8733-4f5f-86f7-c22a81b7a59f", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "X-Debug-Session-Id": "d87db7",
      },
      body: JSON.stringify({
        sessionId: "d87db7",
        runId: "pre-fix",
        hypothesisId: "A",
        location: "modelsService.ts:uploadModel",
        message: "Model upload failed",
        data: { status, errorMessage, fileCount: request.files.length },
        timestamp: Date.now(),
      }),
    }).catch(() => {});
    // #endregion

    const normalizedError = new Error(errorMessage);
    (normalizedError as Error & { status?: number }).status = status;
    throw normalizedError;
  }
}
