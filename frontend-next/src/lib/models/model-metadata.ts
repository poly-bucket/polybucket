import { z } from "zod";
import { LicenseTypes, PrivacySettings } from "@/lib/api/client";

export const MODEL_CATEGORIES = [
  "Art",
  "Technology",
  "Toys",
  "Tools",
  "Games",
  "Household",
  "Engineering",
  "Fashion",
  "Medical",
  "Other",
] as const;

export type ModelCategoryName = (typeof MODEL_CATEGORIES)[number];

const modelMetadataFieldsSchema = z.object({
  name: z.string().min(1, "Name is required").max(255),
  description: z.string().max(2000).optional().or(z.literal("")),
  privacy: z.nativeEnum(PrivacySettings),
  license: z.nativeEnum(LicenseTypes),
  aiGenerated: z.boolean(),
  wip: z.boolean(),
  nsfw: z.boolean(),
  isRemix: z.boolean(),
  remixUrl: z.string().url("Invalid URL").optional().or(z.literal("")),
});

export const modelMetadataSchema = modelMetadataFieldsSchema.refine(
  (data) => !data.isRemix || (data.remixUrl && data.remixUrl.length > 0),
  {
    message: "Remix URL is required when marking as remix",
    path: ["remixUrl"],
  }
);

export type ModelMetadataFormValues = z.infer<typeof modelMetadataFieldsSchema>;

export function parseLicenseDefault(license: string): LicenseTypes {
  const values = Object.values(LicenseTypes) as string[];
  if (values.includes(license)) {
    return license as LicenseTypes;
  }
  return LicenseTypes.MIT;
}

export function parsePrivacyDefault(
  privacy: "Public" | "Private" | "Unlisted"
): PrivacySettings {
  if (privacy === "Private") return PrivacySettings.Private;
  if (privacy === "Unlisted") return PrivacySettings.Unlisted;
  return PrivacySettings.Public;
}

export function titleFromFileName(fileName: string): string {
  const lastDot = fileName.lastIndexOf(".");
  const base = lastDot > 0 ? fileName.slice(0, lastDot) : fileName;
  return base.replace(/[-_]+/g, " ").trim();
}
