"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Eye, EyeOff } from "lucide-react";
import { toast } from "sonner";
import { changePassword } from "@/lib/services/changePasswordService";
import { SettingsSection } from "@/components/settings/settings-section";
import { SettingsField } from "@/components/settings/settings-field";
import { Button } from "@/components/primitives/button";
import { Input } from "@/components/primitives/input";
import { TwoFactorAuth } from "@/components/settings/two-factor-auth";

const passwordSchema = z
  .object({
    currentPassword: z.string().min(1, "Current password is required"),
    newPassword: z.string().min(8, "Password must be at least 8 characters"),
    confirmPassword: z.string().min(1, "Please confirm your new password"),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    message: "Passwords do not match",
    path: ["confirmPassword"],
  });

type PasswordFormData = z.infer<typeof passwordSchema>;

export default function SecuritySettingsPage() {
  const [showPasswords, setShowPasswords] = useState({
    current: false,
    new: false,
    confirm: false,
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<PasswordFormData>({
    resolver: zodResolver(passwordSchema),
  });

  const onSubmit = async (data: PasswordFormData) => {
    const result = await changePassword(
      data.currentPassword,
      data.newPassword,
      data.confirmPassword
    );
    if (result.success) {
      reset();
      toast.success("Password changed successfully");
    } else {
      toast.error(result.message ?? "Failed to change password");
    }
  };

  const toggleVisibility = (field: keyof typeof showPasswords) => {
    setShowPasswords((prev) => ({ ...prev, [field]: !prev[field] }));
  };

  return (
    <div className="space-y-8">
      <SettingsSection
        title="Change Password"
        description="Update your password to keep your account secure"
      >
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <SettingsField
            label="Current Password"
            description="Enter your current password"
            error={errors.currentPassword?.message}
          >
            <div className="relative max-w-md">
              <Input
                {...register("currentPassword")}
                variant="glass"
                type={showPasswords.current ? "text" : "password"}
                placeholder="Enter current password"
                error={!!errors.currentPassword}
                className="pr-10"
              />
              <button
                type="button"
                onClick={() => toggleVisibility("current")}
                className="absolute right-3 top-1/2 -translate-y-1/2 text-white/60 hover:text-white"
              >
                {showPasswords.current ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
              </button>
            </div>
          </SettingsField>
          <SettingsField
            label="New Password"
            description="At least 8 characters"
            error={errors.newPassword?.message}
          >
            <div className="relative max-w-md">
              <Input
                {...register("newPassword")}
                variant="glass"
                type={showPasswords.new ? "text" : "password"}
                placeholder="Enter new password"
                error={!!errors.newPassword}
                className="pr-10"
              />
              <button
                type="button"
                onClick={() => toggleVisibility("new")}
                className="absolute right-3 top-1/2 -translate-y-1/2 text-white/60 hover:text-white"
              >
                {showPasswords.new ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
              </button>
            </div>
          </SettingsField>
          <SettingsField
            label="Confirm New Password"
            error={errors.confirmPassword?.message}
          >
            <div className="relative max-w-md">
              <Input
                {...register("confirmPassword")}
                variant="glass"
                type={showPasswords.confirm ? "text" : "password"}
                placeholder="Confirm new password"
                error={!!errors.confirmPassword}
                className="pr-10"
              />
              <button
                type="button"
                onClick={() => toggleVisibility("confirm")}
                className="absolute right-3 top-1/2 -translate-y-1/2 text-white/60 hover:text-white"
              >
                {showPasswords.confirm ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
              </button>
            </div>
          </SettingsField>
          <div className="pt-2">
            <Button type="submit" disabled={isSubmitting} size="sm">
              {isSubmitting ? "Changing..." : "Change Password"}
            </Button>
          </div>
        </form>
      </SettingsSection>

      <SettingsSection
        title="Two-Factor Authentication"
        description="Add an extra layer of security to your account"
      >
        <TwoFactorAuth />
      </SettingsSection>

      <SettingsSection
        title="Security Tips"
        description="Best practices to keep your account secure"
      >
        <ul className="space-y-2 text-sm text-white/70">
          <li>• Use a strong password with at least 8 characters</li>
          <li>• Include a mix of uppercase, lowercase, numbers, and symbols</li>
          <li>• Don&apos;t reuse passwords from other accounts</li>
          <li>• Consider using a password manager</li>
        </ul>
      </SettingsSection>
    </div>
  );
}
