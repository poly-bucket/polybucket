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

function clampBoundedInt(value: number, min: number, max: number): number {
  if (Number.isNaN(value)) {
    return min;
  }
  return Math.min(max, Math.max(min, value));
}

function parseNumericInput(raw: string): number {
  if (raw === "") {
    return Number.NaN;
  }
  const parsed = parseInt(raw, 10);
  return Number.isNaN(parsed) ? Number.NaN : parsed;
}

export default function SiteSecurityStep({
  onComplete,
  onBack,
  data,
}: SiteSecurityStepProps) {
  const [formData, setFormData] = useState(() =>
    readSiteSecurityFromData(data)
  );

  const handleInputChange = (
    field: string,
    value: string | number | boolean
  ) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  const handleBoundedIntChange = (
    field:
      | "maxFailedLoginAttempts"
      | "lockoutDurationMinutes"
      | "passwordMinLength",
    raw: string
  ) => {
    const parsed = parseNumericInput(raw);
    if (Number.isNaN(parsed)) {
      return;
    }
    handleInputChange(field, parsed);
  };

  const handleBoundedIntBlur = (
    field:
      | "maxFailedLoginAttempts"
      | "lockoutDurationMinutes"
      | "passwordMinLength",
    raw: string,
    min: number,
    max: number
  ) => {
    const parsed = parseNumericInput(raw);
    const clamped = clampBoundedInt(parsed, min, max);
    handleInputChange(field, clamped);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const normalized = {
      ...formData,
      maxFailedLoginAttempts: clampBoundedInt(
        formData.maxFailedLoginAttempts,
        1,
        10
      ),
      lockoutDurationMinutes: clampBoundedInt(
        formData.lockoutDurationMinutes,
        5,
        60
      ),
      passwordMinLength: clampBoundedInt(formData.passwordMinLength, 6, 20),
    };
    setFormData(normalized);
    onComplete({
      maxFailedLoginAttempts: normalized.maxFailedLoginAttempts,
      lockoutDurationMinutes: normalized.lockoutDurationMinutes,
      requireStrongPasswords: normalized.requireStrongPasswords,
      passwordMinLength: normalized.passwordMinLength,
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
              handleBoundedIntChange("maxFailedLoginAttempts", e.target.value)
            }
            onBlur={(e) =>
              handleBoundedIntBlur(
                "maxFailedLoginAttempts",
                e.target.value,
                1,
                10
              )
            }
            className={inputClass}
          />
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
              handleBoundedIntChange("lockoutDurationMinutes", e.target.value)
            }
            onBlur={(e) =>
              handleBoundedIntBlur(
                "lockoutDurationMinutes",
                e.target.value,
                5,
                60
              )
            }
            className={inputClass}
          />
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
              handleBoundedIntChange("passwordMinLength", e.target.value)
            }
            onBlur={(e) =>
              handleBoundedIntBlur("passwordMinLength", e.target.value, 6, 20)
            }
            className={inputClass}
          />
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
