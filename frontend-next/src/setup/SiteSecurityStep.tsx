"use client";

import { useState } from "react";
import { Button } from "@/components/primitives/button";
import { Input } from "@/components/primitives/input";
import { readSiteSecurityFromData } from "./setupSiteDefaults";

interface SiteSecurityStepProps {
  onComplete: (data: Record<string, unknown>) => void;
  onBack: () => void;
  data: Record<string, unknown>;
}

export default function SiteSecurityStep({
  onComplete,
  onBack,
  data,
}: SiteSecurityStepProps) {
  const [formData, setFormData] = useState(() =>
    readSiteSecurityFromData(data)
  );
  const [errors, setErrors] = useState<Record<string, string>>({});

  const handleInputChange = (
    field: string,
    value: string | number | boolean
  ) => {
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
    if (
      formData.maxFailedLoginAttempts < 1 ||
      formData.maxFailedLoginAttempts > 10
    ) {
      newErrors.maxFailedLoginAttempts = "Must be between 1 and 10";
    }
    if (
      formData.lockoutDurationMinutes < 5 ||
      formData.lockoutDurationMinutes > 60
    ) {
      newErrors.lockoutDurationMinutes = "Must be between 5 and 60 minutes";
    }
    if (formData.passwordMinLength < 6 || formData.passwordMinLength > 20) {
      newErrors.passwordMinLength = "Must be between 6 and 20";
    }
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!validateForm()) return;
    onComplete({
      maxFailedLoginAttempts: formData.maxFailedLoginAttempts,
      lockoutDurationMinutes: formData.lockoutDurationMinutes,
      requireStrongPasswords: formData.requireStrongPasswords,
      passwordMinLength: formData.passwordMinLength,
      requireLoginForUpload: formData.requireLoginForUpload,
      requireEmailVerification: formData.requireEmailVerification,
      requireModeration: formData.requireModeration,
      autoApproveModels: formData.autoApproveModels,
    });
  };

  const inputClass =
    "border-white/20 bg-white/5 text-white placeholder:text-white/50";

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div>
        <h3 className="text-lg font-medium text-white mb-2">Site security</h3>
        <p className="text-white/70 text-sm">
          Set login protection, password rules for all users, and how new content
          is handled. You can refine these later in the admin panel.
        </p>
      </div>

      <div className="space-y-4">
        <div>
          <label
            htmlFor="maxFailedLoginAttempts"
            className="block text-sm font-medium text-white mb-2"
          >
            Max failed login attempts (1–10)
          </label>
          <Input
            id="maxFailedLoginAttempts"
            type="number"
            min={1}
            max={10}
            value={formData.maxFailedLoginAttempts}
            onChange={(e) =>
              handleInputChange(
                "maxFailedLoginAttempts",
                parseInt(e.target.value, 10) || 1
              )
            }
            className={`${inputClass} ${errors.maxFailedLoginAttempts ? "border-red-400" : ""}`}
          />
          {errors.maxFailedLoginAttempts && (
            <p className="mt-1 text-sm text-red-400">
              {errors.maxFailedLoginAttempts}
            </p>
          )}
        </div>

        <div>
          <label
            htmlFor="lockoutDurationMinutes"
            className="block text-sm font-medium text-white mb-2"
          >
            Account lockout duration (minutes, 5–60)
          </label>
          <Input
            id="lockoutDurationMinutes"
            type="number"
            min={5}
            max={60}
            value={formData.lockoutDurationMinutes}
            onChange={(e) =>
              handleInputChange(
                "lockoutDurationMinutes",
                parseInt(e.target.value, 10) || 5
              )
            }
            className={`${inputClass} ${errors.lockoutDurationMinutes ? "border-red-400" : ""}`}
          />
          {errors.lockoutDurationMinutes && (
            <p className="mt-1 text-sm text-red-400">
              {errors.lockoutDurationMinutes}
            </p>
          )}
        </div>

        <div>
          <label
            htmlFor="passwordMinLength"
            className="block text-sm font-medium text-white mb-2"
          >
            Minimum password length (6–20)
          </label>
          <Input
            id="passwordMinLength"
            type="number"
            min={6}
            max={20}
            value={formData.passwordMinLength}
            onChange={(e) =>
              handleInputChange(
                "passwordMinLength",
                parseInt(e.target.value, 10) || 6
              )
            }
            className={`${inputClass} ${errors.passwordMinLength ? "border-red-400" : ""}`}
          />
          {errors.passwordMinLength && (
            <p className="mt-1 text-sm text-red-400">
              {errors.passwordMinLength}
            </p>
          )}
        </div>

        <label className="flex items-center gap-2 cursor-pointer">
          <input
            type="checkbox"
            checked={formData.requireStrongPasswords}
            onChange={(e) =>
              handleInputChange("requireStrongPasswords", e.target.checked)
            }
            className="rounded border-white/30 bg-white/5 text-blue-500 focus:ring-blue-500"
          />
          <span className="text-sm text-white/80">
            Require strong passwords (mixed case, numbers, symbols)
          </span>
        </label>

        <label className="flex items-center gap-2 cursor-pointer">
          <input
            type="checkbox"
            checked={formData.requireLoginForUpload}
            onChange={(e) =>
              handleInputChange("requireLoginForUpload", e.target.checked)
            }
            className="rounded border-white/30 bg-white/5 text-blue-500 focus:ring-blue-500"
          />
          <span className="text-sm text-white/80">
            Require sign-in to upload
          </span>
        </label>

        <label className="flex items-center gap-2 cursor-pointer">
          <input
            type="checkbox"
            checked={formData.requireEmailVerification}
            onChange={(e) =>
              handleInputChange("requireEmailVerification", e.target.checked)
            }
            className="rounded border-white/30 bg-white/5 text-blue-500 focus:ring-blue-500"
          />
          <span className="text-sm text-white/80">
            Require email verification for new accounts
          </span>
        </label>

        <label className="flex items-center gap-2 cursor-pointer">
          <input
            type="checkbox"
            checked={formData.requireModeration}
            onChange={(e) =>
              handleInputChange("requireModeration", e.target.checked)
            }
            className="rounded border-white/30 bg-white/5 text-blue-500 focus:ring-blue-500"
          />
          <span className="text-sm text-white/80">
            Require moderation before new models are visible
          </span>
        </label>

        <label className="flex items-center gap-2 cursor-pointer">
          <input
            type="checkbox"
            checked={formData.autoApproveModels}
            onChange={(e) =>
              handleInputChange("autoApproveModels", e.target.checked)
            }
            className="rounded border-white/30 bg-white/5 text-blue-500 focus:ring-blue-500"
          />
          <span className="text-sm text-white/80">
            Auto-approve new models (only if moderation is off)
          </span>
        </label>
      </div>

      <div className="flex justify-between pt-4">
        <Button
          type="button"
          variant="outline"
          onClick={onBack}
          className="border-white/20 text-white hover:bg-white/10"
        >
          Back
        </Button>
        <Button type="submit">Continue</Button>
      </div>
    </form>
  );
}
