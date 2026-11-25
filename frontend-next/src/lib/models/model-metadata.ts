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

export type ModelMetadataRules = {
  requireModelDescription: boolean;
  minDescriptionLength: number;
  maxDescriptionLength: number;
  requireLicenseSelection: boolean;
  allowCustomLicenses: boolean;
  allowNsfwContent: boolean;
  allowAiGeneratedContent: boolean;
  requireThumbnail: boolean;
  maxTagsPerModel: number;
  requireModelTags: boolean;
  maxCategoriesPerModel: number;
  requireCategorySelection: boolean;
  allowModelRemixing: boolean;
  requireRemixAttribution: boolean;
};

export const defaultModelMetadataRules: ModelMetadataRules = {
  requireModelDescription: true,
  minDescriptionLength: 10,
  maxDescriptionLength: 2000,
  requireLicenseSelection: true,
  allowCustomLicenses: false,
  allowNsfwContent: false,
  allowAiGeneratedContent: true,
  requireThumbnail: false,
  maxTagsPerModel: 10,
  requireModelTags: false,
  maxCategoriesPerModel: 3,
  requireCategorySelection: false,
  allowModelRemixing: true,
  requireRemixAttribution: true,
};

export function parseModelMetadataRules(
  raw: Partial<ModelMetadataRules> | null | undefined
): ModelMetadataRules {
  if (!raw) {
    return defaultModelMetadataRules;
  }
  return {
    requireModelDescription:
      raw.requireModelDescription ?? defaultModelMetadataRules.requireModelDescription,
    minDescriptionLength:
      raw.minDescriptionLength ?? defaultModelMetadataRules.minDescriptionLength,
    maxDescriptionLength:
      raw.maxDescriptionLength ?? defaultModelMetadataRules.maxDescriptionLength,
    requireLicenseSelection:
      raw.requireLicenseSelection ?? defaultModelMetadataRules.requireLicenseSelection,
    allowCustomLicenses:
      raw.allowCustomLicenses ?? defaultModelMetadataRules.allowCustomLicenses,
    allowNsfwContent: raw.allowNsfwContent ?? defaultModelMetadataRules.allowNsfwContent,
    allowAiGeneratedContent:
      raw.allowAiGeneratedContent ?? defaultModelMetadataRules.allowAiGeneratedContent,
    requireThumbnail: raw.requireThumbnail ?? defaultModelMetadataRules.requireThumbnail,
    maxTagsPerModel: raw.maxTagsPerModel ?? defaultModelMetadataRules.maxTagsPerModel,
    requireModelTags: raw.requireModelTags ?? defaultModelMetadataRules.requireModelTags,
    maxCategoriesPerModel:
      raw.maxCategoriesPerModel ?? defaultModelMetadataRules.maxCategoriesPerModel,
    requireCategorySelection:
      raw.requireCategorySelection ?? defaultModelMetadataRules.requireCategorySelection,
    allowModelRemixing:
      raw.allowModelRemixing ?? defaultModelMetadataRules.allowModelRemixing,
    requireRemixAttribution:
      raw.requireRemixAttribution ?? defaultModelMetadataRules.requireRemixAttribution,
  };
}

export function createModelMetadataSchema(rules: ModelMetadataRules) {
  const descriptionBase = z.string().max(
    rules.maxDescriptionLength > 0 ? rules.maxDescriptionLength : 2000,
    `Description cannot exceed ${rules.maxDescriptionLength} characters`
  );

  const descriptionSchema = rules.requireModelDescription
    ? descriptionBase
        .min(1, "Description is required")
        .refine(
          (val) =>
            rules.minDescriptionLength <= 0 ||
            val.trim().length >= rules.minDescriptionLength,
          {
            message: `Description must be at least ${rules.minDescriptionLength} characters`,
          }
        )
    : descriptionBase
        .optional()
        .or(z.literal(""))
        .refine(
          (val) => {
            const trimmed = (val ?? "").trim();
            if (trimmed.length === 0) {
              return true;
            }
            return (
              rules.minDescriptionLength <= 0 ||
              trimmed.length >= rules.minDescriptionLength
            );
          },
          {
            message: `Description must be at least ${rules.minDescriptionLength} characters`,
          }
        );

  const licenseSchema = rules.requireLicenseSelection
    ? z.nativeEnum(LicenseTypes, { error: "License is required" })
    : z.nativeEnum(LicenseTypes);

  const modelMetadataFieldsSchema = z.object({
    name: z.string().min(1, "Name is required").max(255),
    description: descriptionSchema,
    privacy: z.nativeEnum(PrivacySettings),
    license: licenseSchema,
    aiGenerated: z
      .boolean()
      .refine((v) => !v || rules.allowAiGeneratedContent, {
        message: "AI-generated content is not allowed on this site",
      }),
    wip: z.boolean(),
    nsfw: z
      .boolean()
      .refine((v) => !v || rules.allowNsfwContent, {
        message: "NSFW content is not allowed on this site",
      }),
    isRemix: z.boolean(),
    remixUrl: z.string().url("Invalid URL").optional().or(z.literal("")),
  });

  return modelMetadataFieldsSchema
    .refine((data) => !data.isRemix || rules.allowModelRemixing, {
      message: "Model remixing is not allowed on this site",
      path: ["isRemix"],
    })
    .refine(
      (data) =>
        !data.isRemix ||
        !rules.requireRemixAttribution ||
        (data.remixUrl && data.remixUrl.length > 0),
      {
        message: "Remix URL is required when marking as remix",
        path: ["remixUrl"],
      }
    );
}

export const modelMetadataSchema = createModelMetadataSchema(defaultModelMetadataRules);

export type ModelMetadataFormValues = z.infer<
  ReturnType<typeof createModelMetadataSchema>
>;

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

const IMAGE_EXTENSIONS = new Set([
  ".jpg",
  ".jpeg",
  ".png",
  ".gif",
  ".webp",
  ".bmp",
]);

export function fileNameHasImageExtension(fileName: string): boolean {
  const lastDot = fileName.lastIndexOf(".");
  if (lastDot < 0) {
    return false;
  }
  return IMAGE_EXTENSIONS.has(fileName.slice(lastDot).toLowerCase());
}

export function uploadBundleHasImage(fileNames: string[]): boolean {
  return fileNames.some(fileNameHasImageExtension);
}