"use client";

import { useEffect, useState } from "react";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { toast } from "sonner";
import {
  type Model,
  LicenseTypes,
  PrivacySettings,
  UpdateModelRequest,
} from "@/lib/api/client";
import { ApiClientFactory } from "@/lib/api/clientFactory";
import { Input } from "@/components/primitives/input";
import { Textarea } from "@/components/ui/glass/textarea";
import { Button } from "@/components/primitives/button";
import { Switch } from "@/components/primitives/switch";
import { SettingsField } from "@/components/settings/settings-field";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { cn } from "@/lib/utils";
import { X } from "lucide-react";

const CATEGORIES = [
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
];

const editModelSchema = z
  .object({
    name: z.string().min(1, "Name is required").max(255),
    description: z.string().max(2000).optional().or(z.literal("")),
    privacy: z.nativeEnum(PrivacySettings),
    license: z.nativeEnum(LicenseTypes),
    aiGenerated: z.boolean(),
    wip: z.boolean(),
    nsfw: z.boolean(),
    isRemix: z.boolean(),
    remixUrl: z.string().url("Invalid URL").optional().or(z.literal("")),
  })
  .refine((data) => !data.isRemix || (data.remixUrl && data.remixUrl.length > 0), {
    message: "Remix URL is required when marking as remix",
    path: ["remixUrl"],
  });

type EditModelFormData = z.infer<typeof editModelSchema>;

interface ModelEditFormProps {
  model: Model;
  onSave: (model: Model) => void;
  onCancel: () => void;
  onDirtyChange?: (dirty: boolean) => void;
}

export function ModelEditForm({
  model,
  onSave,
  onCancel,
  onDirtyChange,
}: ModelEditFormProps) {
  const [categories, setCategories] = useState<string[]>(
    () => model.categories?.map((c) => c.name ?? "").filter(Boolean) ?? []
  );
  const [tags, setTags] = useState<string[]>(
    () => model.tags?.map((t) => t.name ?? "").filter(Boolean) ?? []
  );
  const [newTag, setNewTag] = useState("");

  const {
    register,
    handleSubmit,
    control,
    formState: { isDirty, isSubmitting, errors },
    reset,
    watch,
  } = useForm<EditModelFormData>({
    resolver: zodResolver(editModelSchema),
    defaultValues: {
      name: model.name ?? "",
      description: model.description ?? "",
      privacy: model.privacy ?? PrivacySettings.Public,
      license: model.license ?? LicenseTypes.MIT,
      aiGenerated: model.aiGenerated ?? false,
      wip: model.wip ?? false,
      nsfw: model.nsfw ?? false,
      isRemix: model.isRemix ?? false,
      remixUrl: model.remixUrl ?? "",
    },
  });

  useEffect(() => {
    onDirtyChange?.(isDirty);
  }, [isDirty, onDirtyChange]);

  const isRemix = watch("isRemix");

  const handleCategoryToggle = (category: string) => {
    setCategories((prev) =>
      prev.includes(category) ? prev.filter((c) => c !== category) : [...prev, category]
    );
  };

  const handleAddTag = () => {
    const trimmed = newTag.trim();
    if (trimmed && !tags.includes(trimmed)) {
      setTags((prev) => [...prev, trimmed]);
      setNewTag("");
    }
  };

  const handleRemoveTag = (tagToRemove: string) => {
    setTags((prev) => prev.filter((t) => t !== tagToRemove));
  };

  const onSubmit = async (data: EditModelFormData) => {
    if (!model.id) return;

    const client = ApiClientFactory.getApiClient();

    try {
      const request = UpdateModelRequest.fromJS({
        name: data.name,
        description: data.description || undefined,
        privacy: data.privacy,
        license: data.license,
        aiGenerated: data.aiGenerated,
        wip: data.wip,
        nsfw: data.nsfw,
        isRemix: data.isRemix,
        remixUrl: data.isRemix ? data.remixUrl || undefined : undefined,
      });

      const response = await client.updateModel_UpdateModel(model.id, request);
      let updatedModel = response?.model;

      if (!updatedModel) {
        updatedModel = { ...model, ...request } as Model;
      }

      const currentTagNames = new Set(model.tags?.map((t) => t.name) ?? []);
      const desiredTagNames = new Set(tags);
      const toAdd = tags.filter((t) => !currentTagNames.has(t));
      const toRemove = (model.tags ?? []).filter((t) => t.name && !desiredTagNames.has(t.name));

      const tagResults = await Promise.allSettled([
        ...toAdd.map((tagName) => client.addTagToModel_AddTagToModel(model.id!, tagName)),
        ...toRemove.map((tag) =>
          tag.id ? client.removeTagFromModel_RemoveTagFromModel(model.id!, tag.id) : Promise.resolve()
        ),
      ]);

      const failedTagOps = tagResults.filter((r) => r.status === "rejected");
      if (failedTagOps.length > 0) {
        toast.error(`Some tag updates failed (${failedTagOps.length})`);
      }

      const currentCategoryNames = new Set(
        model.categories?.map((c) => c.name ?? "").filter(Boolean) ?? []
      );
      const desiredCategorySet = new Set(categories);
      const toRemoveCategories = (model.categories ?? []).filter(
        (c) => c.id && !desiredCategorySet.has(c.name ?? "")
      );
      const toAddCategories = categories.filter((name) => !currentCategoryNames.has(name));

      try {
        const categoryClient = ApiClientFactory.getApiClient();
        const categoryResponse = await categoryClient.getCategories_GetCategories(
          1,
          100,
          null
        );
        const nameToId = new Map(
          (categoryResponse.categories ?? []).map((c) => [c.name ?? "", c.id ?? ""])
        );

        await Promise.allSettled([
          ...toRemoveCategories.map((c) =>
            c.id
              ? client.removeCategoryFromModel_RemoveCategoryFromModel(model.id!, c.id)
              : Promise.resolve()
          ),
          ...toAddCategories
            .filter((name) => nameToId.has(name))
            .map((name) =>
              client.addCategoryToModel_AddCategoryToModel(model.id!, nameToId.get(name)!)
            ),
        ]);
      } catch {
        // Category sync may require admin; skip gracefully
      }

      const refreshed = await client.getModelById_GetModel(model.id);
      onSave(refreshed?.model ?? updatedModel);
      reset(data);
    } catch (err) {
      toast.error(err instanceof Error ? err.message : "Failed to update model");
    }
  };

  return (
    <form
      onSubmit={handleSubmit(onSubmit)}
      className="space-y-6 py-4"
      aria-busy={isSubmitting}
    >
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-6">
        <SettingsField label="Model Title" error={errors.name?.message}>
          <Input
            {...register("name")}
            variant="glass"
            placeholder="Enter model title"
            className="w-full"
            aria-invalid={!!errors.name}
          />
        </SettingsField>
        <SettingsField label="Privacy" error={errors.privacy?.message}>
          <Controller
            name="privacy"
            control={control}
            render={({ field }) => (
              <Select value={field.value} onValueChange={field.onChange}>
                <SelectTrigger variant="glass" className="w-full">
                  <SelectValue placeholder="Select privacy" />
                </SelectTrigger>
                <SelectContent variant="glass">
                  <SelectItem value={PrivacySettings.Public}>
                    Public - Everyone can see this model
                  </SelectItem>
                  <SelectItem value={PrivacySettings.Private}>
                    Private - Only you can see this model
                  </SelectItem>
                  <SelectItem value={PrivacySettings.Unlisted}>
                    Unlisted - Only people with the link can see this model
                  </SelectItem>
                </SelectContent>
              </Select>
            )}
          />
        </SettingsField>
      </div>

      <SettingsField label="Description" error={errors.description?.message}>
        <Textarea
          {...register("description")}
          variant="glass"
          placeholder="Describe your model..."
          rows={4}
          className="w-full resize-none"
          aria-invalid={!!errors.description}
        />
      </SettingsField>

      <SettingsField label="License" error={errors.license?.message}>
        <Controller
          name="license"
          control={control}
          render={({ field }) => (
            <Select value={field.value} onValueChange={field.onChange}>
              <SelectTrigger variant="glass" className="w-full">
                <SelectValue placeholder="Select license" />
              </SelectTrigger>
              <SelectContent variant="glass">
                {Object.entries(LicenseTypes).map(([key, value]) => (
                  <SelectItem key={value} value={value}>
                    {value}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          )}
        />
      </SettingsField>

      <div>
        <label className="block text-sm font-medium text-white mb-2">Categories</label>
        <div className="flex flex-wrap gap-2" role="list">
          {CATEGORIES.map((category) => (
            <button
              key={category}
              type="button"
              role="listitem"
              onClick={() => handleCategoryToggle(category)}
              className={cn(
                "px-3 py-1.5 rounded-full text-sm border transition-colors min-h-[32px]",
                categories.includes(category)
                  ? "bg-primary border-primary text-primary-foreground"
                  : "bg-transparent border-white/20 text-white/80 hover:border-white/40"
              )}
            >
              {category}
            </button>
          ))}
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium text-white mb-2">Tags</label>
        <div className="flex flex-wrap gap-2 mb-2" role="list" aria-live="polite">
          {tags.map((tag) => (
            <span
              key={tag}
              role="listitem"
              className="inline-flex items-center px-3 py-1 rounded-full text-sm bg-white/20 text-white"
            >
              {tag}
              <button
                type="button"
                onClick={() => handleRemoveTag(tag)}
                className="ml-2 text-white/80 hover:text-white"
                aria-label={`Remove tag ${tag}`}
              >
                <X className="h-3.5 w-3.5" />
              </button>
            </span>
          ))}
        </div>
        <div className="flex gap-2">
          <Input
            type="text"
            value={newTag}
            onChange={(e) => setNewTag(e.target.value)}
            onKeyDown={(e) => e.key === "Enter" && (e.preventDefault(), handleAddTag())}
            variant="glass"
            placeholder="Add a tag..."
            className="flex-1"
          />
          <Button type="button" variant="glass" onClick={handleAddTag}>
            Add
          </Button>
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium text-white mb-3">Model Properties</label>
        <div className="space-y-3">
          {(
            [
              { name: "aiGenerated" as const, label: "AI Generated" },
              { name: "wip" as const, label: "Work in Progress" },
              { name: "nsfw" as const, label: "NSFW (Not Safe for Work)" },
              { name: "isRemix" as const, label: "Remix of Another Model" },
            ] as const
          ).map(({ name, label }) => (
            <div key={name} className="flex items-center gap-3">
              <Controller
                name={name}
                control={control}
                render={({ field }) => (
                  <Switch
                    checked={field.value}
                    onCheckedChange={field.onChange}
                  />
                )}
              />
              <span className="text-sm text-white/80">{label}</span>
            </div>
          ))}
        </div>
      </div>

      {isRemix && (
        <SettingsField label="Original Model URL" error={errors.remixUrl?.message}>
          <Input
            {...register("remixUrl")}
            variant="glass"
            type="url"
            placeholder="https://..."
            className="w-full"
            aria-invalid={!!errors.remixUrl}
          />
        </SettingsField>
      )}

      <div className="flex justify-end gap-4 pt-6 border-t border-white/10">
        <Button type="button" variant="outline" onClick={onCancel} disabled={isSubmitting}>
          Cancel
        </Button>
        <Button type="submit" variant="glass" disabled={isSubmitting}>
          {isSubmitting ? "Saving..." : "Save Changes"}
        </Button>
      </div>
    </form>
  );
}
