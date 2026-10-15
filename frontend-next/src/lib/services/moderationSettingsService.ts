import axiosInstance from "@/lib/api/axiosConfig";

export interface ModerationSettings {
  requireModeration?: boolean;
  autoApproveModels?: boolean;
  requireUploadModeration?: boolean;
  requireModeratorApproval?: boolean;
  autoApproveVerifiedUsers?: boolean;
}

export async function getModerationSettings(): Promise<ModerationSettings> {
  const { data } = await axiosInstance.get<ModerationSettings>(
    "/api/moderation/models/settings"
  );
  return data;
}

export async function updateModerationSettings(
  settings: ModerationSettings
): Promise<ModerationSettings> {
  const { data } = await axiosInstance.put<ModerationSettings>(
    "/api/moderation/models/settings",
    settings
  );
  return data;
}
