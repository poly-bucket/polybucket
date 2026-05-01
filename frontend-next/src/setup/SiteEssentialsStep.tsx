"use client";

import { useState } from "react";
import { ApiClientFactory } from "@/lib/api/clientFactory";
import { UpdateSiteSettingsCommand } from "@/lib/api/client";
import { Button } from "@/components/primitives/button";
import { Input } from "@/components/primitives/input";
import {
  defaultNonSecuritySiteSettings,
  readSiteSecurityFromData,
} from "./setupSiteDefaults";

interface SiteEssentialsStepProps {
  onComplete: (data: Record<string, unknown>) => void;
  onBack: () => void;
  data: Record<string, unknown>;
}

export default function SiteEssentialsStep({
  onComplete,
  onBack,
  data,
}: SiteEssentialsStepProps) {
  const [formData, setFormData] = useState({
    siteName: (data.siteName as string) ?? "PolyBucket",
    contactEmail: (data.contactEmail as string) ?? "",
    allowPublicBrowsing: (data.allowPublicBrowsing as boolean) ?? true,
    allowUserRegistration: (data.allowUserRegistration as boolean) ?? true,
  });
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleInputChange = (field: string, value: string | boolean) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
    if (errors[field]) {
      setErrors((prev) => {
        const next = { ...prev };
        delete next[field];
        return next;
      });
    }
  };

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {};
    if (!formData.siteName.trim()) {
      newErrors.siteName = "Site name is required";
    }
    if (!formData.contactEmail.trim()) {
      newErrors.contactEmail = "Contact email is required";
    } else if (
      !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.contactEmail)
    ) {
      newErrors.contactEmail = "Please enter a valid email address";
    }
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validateForm()) return;
    setIsSubmitting(true);
    setErrors({});
    try {
      const client = ApiClientFactory.getApiClient();
      const base = defaultNonSecuritySiteSettings;
      const security = readSiteSecurityFromData(data);
      const command = new UpdateSiteSettingsCommand({
        siteName: formData.siteName,
        siteDescription: base.siteDescription,
        contactEmail: formData.contactEmail,
        allowPublicBrowsing: formData.allowPublicBrowsing,
        allowUserRegistration: formData.allowUserRegistration,
        requireLoginForUpload: security.requireLoginForUpload,
        requireEmailVerification: security.requireEmailVerification,
        disableEmailSettings: base.disableEmailSettings,
        maxFileSizeBytes: base.maxFileSizeBytes,
        allowedFileTypes: base.allowedFileTypes,
        maxFilesPerUpload: base.maxFilesPerUpload,
        enableFileCompression: base.enableFileCompression,
        autoGeneratePreviews: base.autoGeneratePreviews,
        defaultModelPrivacy: base.defaultModelPrivacy,
        autoApproveModels: security.autoApproveModels,
        requireModeration: security.requireModeration,
        defaultUserRole: base.defaultUserRole,
        allowCustomRoles: base.allowCustomRoles,
        maxFailedLoginAttempts: security.maxFailedLoginAttempts,
        lockoutDurationMinutes: security.lockoutDurationMinutes,
        requireStrongPasswords: security.requireStrongPasswords,
        passwordMinLength: security.passwordMinLength,
        defaultTheme: base.defaultTheme,
        defaultLanguage: base.defaultLanguage,
      });
      const response = await client.systemSetup_UpdateSiteSettings(command);
      if (response?.success) {
        onComplete(formData);
      } else {
        setErrors({
          submit: response?.message ?? "Failed to save site settings",
        });
      }
    } catch (err: unknown) {
      const message =
        (err as { response?: { data?: { message?: string }; message?: string }; message?: string })
          ?.response?.data?.message ??
        (err as { message?: string })?.message ??
        "An error occurred while saving";
      setErrors({ submit: message });
    } finally {
      setIsSubmitting(false);
    }
  };

  const inputClass =
    "border-white/20 bg-white/5 text-white placeholder:text-white/50";

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div>
        <h3 className="text-lg font-medium text-white mb-2">
          Site essentials
        </h3>
        <p className="text-white/70 text-sm">
          Configure your site name and contact email. You can adjust advanced
          settings later in the admin panel.
        </p>
      </div>

      <div className="space-y-4">
        <div>
          <label
            htmlFor="siteName"
            className="block text-sm font-medium text-white mb-2"
          >
            Site Name *
          </label>
          <Input
            id="siteName"
            type="text"
            value={formData.siteName}
            onChange={(e) => handleInputChange("siteName", e.target.value)}
            placeholder="PolyBucket"
            className={`${inputClass} ${errors.siteName ? "border-red-400" : ""}`}
          />
          {errors.siteName && (
            <p className="mt-1 text-sm text-red-400">{errors.siteName}</p>
          )}
        </div>

        <div>
          <label
            htmlFor="contactEmail"
            className="block text-sm font-medium text-white mb-2"
          >
            Contact Email *
          </label>
          <Input
            id="contactEmail"
            type="email"
            value={formData.contactEmail}
            onChange={(e) => handleInputChange("contactEmail", e.target.value)}
            placeholder="admin@example.com"
            className={`${inputClass} ${errors.contactEmail ? "border-red-400" : ""}`}
          />
          {errors.contactEmail && (
            <p className="mt-1 text-sm text-red-400">{errors.contactEmail}</p>
          )}
        </div>

        <label className="flex items-center gap-2 cursor-pointer">
          <input
            type="checkbox"
            checked={formData.allowPublicBrowsing}
            onChange={(e) =>
              handleInputChange("allowPublicBrowsing", e.target.checked)
            }
            className="rounded border-white/30 bg-white/5 text-blue-500 focus:ring-blue-500"
          />
          <span className="text-sm text-white/80">
            Allow public browsing
          </span>
        </label>

        <label className="flex items-center gap-2 cursor-pointer">
          <input
            type="checkbox"
            checked={formData.allowUserRegistration}
            onChange={(e) =>
              handleInputChange("allowUserRegistration", e.target.checked)
            }
            className="rounded border-white/30 bg-white/5 text-blue-500 focus:ring-blue-500"
          />
          <span className="text-sm text-white/80">
            Allow user registration
          </span>
        </label>
      </div>

      {errors.submit && (
        <div className="rounded-md border border-red-500/50 bg-red-500/10 px-3 py-2 text-sm text-red-300">
          {errors.submit}
        </div>
      )}

      <div className="flex justify-between pt-4">
        <Button
          type="button"
          variant="outline"
          onClick={onBack}
          className="border-white/20 text-white hover:bg-white/10"
        >
          Back
        </Button>
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting ? (
            <>
              <div className="size-4 animate-spin rounded-full border-2 border-white/30 border-t-white" />
              Saving...
            </>
          ) : (
            "Save & Continue"
          )}
        </Button>
      </div>
    </form>
  );
}
