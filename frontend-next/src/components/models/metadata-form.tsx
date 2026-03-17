"use client";

import React from "react";
import { Input } from "@/components/primitives/input";
import { Button } from "@/components/primitives/button";
import { cn } from "@/lib/utils";
import type { PrivacySettings } from "@/lib/api/client";

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

const LICENSES = [
  "MIT",
  "GPL",
  "Creative Commons",
  "Commercial",
  "Custom",
];

export interface ModelData {
  title: string;
  description: string;
  privacy: PrivacySettings;
  license: string;
  categories: string[];
  aiGenerated: boolean;
  workInProgress: boolean;
  nsfw: boolean;
  remix: boolean;
}

interface MetadataFormProps {
  data: ModelData;
  onChange: (field: keyof ModelData, value: unknown) => void;
  onCancel: () => void;
  onSubmit: () => void;
  isSubmitting: boolean;
  submitLabel?: string;
}

export default function MetadataForm({
  data,
  onChange,
  onCancel,
  onSubmit,
  isSubmitting,
  submitLabel = "Upload Model",
}: MetadataFormProps) {
  const handleCategoryToggle = (category: string) => {
    onChange(
      "categories",
      data.categories.includes(category)
        ? data.categories.filter((c) => c !== category)
        : [...data.categories, category]
    );
  };

  return (
    <div className="space-y-6">
      <div>
        <label className="block text-sm font-medium text-foreground mb-2">
          Title
        </label>
        <Input
          type="text"
          value={data.title}
          onChange={(e) => onChange("title", e.target.value)}
          placeholder="Enter model title"
          className="glass-bg border-white/20"
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-foreground mb-2">
          Description
        </label>
        <textarea
          value={data.description}
          onChange={(e) => onChange("description", e.target.value)}
          rows={4}
          placeholder="Enter model description"
          className={cn(
            "w-full min-w-0 rounded-md border bg-transparent px-3 py-2 text-base",
            "border-input dark:bg-input/30 glass-bg border-white/20",
            "placeholder:text-muted-foreground resize-none",
            "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary"
          )}
        />
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-foreground mb-2">
            Privacy
          </label>
          <select
            value={data.privacy}
            onChange={(e) =>
              onChange("privacy", e.target.value as PrivacySettings)
            }
            className={cn(
              "w-full h-9 rounded-md border px-3 py-1 text-sm",
              "border-input dark:bg-input/30 glass-bg border-white/20",
              "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary"
            )}
          >
            <option value="Public">Public</option>
            <option value="Private">Private</option>
            <option value="Unlisted">Unlisted</option>
          </select>
        </div>
        <div>
          <label className="block text-sm font-medium text-foreground mb-2">
            License
          </label>
          <select
            value={data.license}
            onChange={(e) => onChange("license", e.target.value)}
            className={cn(
              "w-full h-9 rounded-md border px-3 py-1 text-sm",
              "border-input dark:bg-input/30 glass-bg border-white/20",
              "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary"
            )}
          >
            {LICENSES.map((license) => (
              <option key={license} value={license}>
                {license}
              </option>
            ))}
          </select>
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium text-foreground mb-2">
          Categories
        </label>
        <div className="flex flex-wrap gap-2">
          {CATEGORIES.map((category) => (
            <button
              key={category}
              type="button"
              onClick={() => handleCategoryToggle(category)}
              className={cn(
                "px-3 py-1 rounded-full text-sm border transition-colors",
                data.categories.includes(category)
                  ? "bg-primary border-primary text-primary-foreground"
                  : "bg-transparent border-white/20 text-foreground hover:border-white/40"
              )}
            >
              {category}
            </button>
          ))}
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium text-foreground mb-3">
          Options
        </label>
        <div className="space-y-2">
          {[
            { key: "aiGenerated", label: "AI Generated" },
            { key: "workInProgress", label: "Work in Progress" },
            { key: "nsfw", label: "NSFW" },
            { key: "remix", label: "Remix of Another Model" },
          ].map(({ key, label }) => (
            <label
              key={key}
              className="flex items-center gap-2 cursor-pointer text-sm text-foreground"
            >
              <input
                type="checkbox"
                checked={data[key as keyof ModelData] as boolean}
                onChange={(e) =>
                  onChange(key as keyof ModelData, e.target.checked)
                }
                className="rounded border-input"
              />
              {label}
            </label>
          ))}
        </div>
      </div>

      <div className="flex justify-end gap-4 pt-6">
        <Button variant="outline" onClick={onCancel}>
          Cancel
        </Button>
        <Button
          onClick={onSubmit}
          disabled={isSubmitting}
          className="disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {isSubmitting ? "Uploading..." : submitLabel}
        </Button>
      </div>
    </div>
  );
}
