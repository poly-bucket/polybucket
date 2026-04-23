"use client";

import { useState, useRef, useEffect, useCallback, useMemo } from "react";
import { Eye, EyeOff } from "lucide-react";
import { toast } from "sonner";
import { useAuth } from "@/contexts/AuthContext";
import { svgToDataUrl, previewUserAvatarSvg } from "@/lib/avatar/minidenticon";
import { regenerateUserAvatar } from "@/lib/services/avatarService";
import { ApiClientFactory } from "@/lib/api/clientFactory";
import {
  ChangePasswordCommand,
  type ChangePasswordResponse,
} from "@/lib/api/client";
import { twoFactorAuthService } from "@/services/twoFactorAuthService";
import QRCode from "react-qr-code";
import { Button } from "@/components/primitives/button";
import { Input } from "@/components/primitives/input";
import PasswordStrengthIndicator from "./PasswordStrengthIndicator";

interface SecurityStepProps {
  onComplete: (data: {
    passwordChanged?: boolean;
    passwordSkipped?: boolean;
    twoFactorEnabled?: boolean;
    backupCodes?: string[];
  }) => void;
  onBack: () => void;
  isFirstStep: boolean;
}

type SecurityPhase = "password" | "2fa";

export default function SecurityStep({
  onComplete,
  onBack,
  isFirstStep,
}: SecurityStepProps) {
  const { user, refreshUserFromMe } = useAuth();
  const [setupAvatarSalt, setSetupAvatarSalt] = useState("");
  const [setupAvatarSaving, setSetupAvatarSaving] = useState(false);
  const setupAvatarPreview = useMemo(
    () =>
      user?.id
        ? svgToDataUrl(
            previewUserAvatarSvg(user.id, setupAvatarSalt || undefined)
          )
        : "",
    [user?.id, setupAvatarSalt]
  );

  const handleSaveSetupAvatar = async () => {
    if (!user?.id) return;
    setSetupAvatarSaving(true);
    try {
      await regenerateUserAvatar(setupAvatarSalt.trim() || undefined);
      await refreshUserFromMe();
      toast.success("Avatar saved");
    } catch (e) {
      console.error(e);
      toast.error("Could not save avatar");
    } finally {
      setSetupAvatarSaving(false);
    }
  };

  const [phase, setPhase] = useState<SecurityPhase>("password");
  const [formData, setFormData] = useState({
    currentPassword: "",
    newPassword: "",
    confirmPassword: "",
  });
  const [enable2FA, setEnable2FA] = useState(false);
  const [showPasswords, setShowPasswords] = useState({
    current: false,
    new: false,
    confirm: false,
  });
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isSkipping, setIsSkipping] = useState(false);
  const [qrCode, setQrCode] = useState("");
  const [verificationCode, setVerificationCode] = useState([
    "",
    "",
    "",
    "",
    "",
    "",
  ]);
  const [isGenerating, setIsGenerating] = useState(false);
  const [isVerifying, setIsVerifying] = useState(false);
  const [hasAutoSubmitted, setHasAutoSubmitted] = useState(false);
  const inputRefs = useRef<(HTMLInputElement | null)[]>([]);

  const handleInputChange = (field: string, value: string) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
    if (errors[field]) {
      setErrors((prev) => {
        const next = { ...prev };
        delete next[field];
        return next;
      });
    }
  };

  const isPasswordFormValid = (): boolean =>
    formData.currentPassword.trim() !== "" &&
    formData.newPassword.trim() !== "" &&
    formData.confirmPassword.trim() !== "" &&
    formData.newPassword === formData.confirmPassword &&
    formData.newPassword.length >= 8 &&
    /[A-Z]/.test(formData.newPassword) &&
    /[a-z]/.test(formData.newPassword) &&
    /\d/.test(formData.newPassword) &&
    /[!@#$%^&*(),.?":{}|<>]/.test(formData.newPassword);

  const validatePasswordForm = (): boolean => {
    const newErrors: Record<string, string> = {};
    if (!formData.currentPassword) {
      newErrors.currentPassword = "Current password is required";
    }
    if (!formData.newPassword) {
      newErrors.newPassword = "New password is required";
    } else if (formData.newPassword.length < 8) {
      newErrors.newPassword = "Password must be at least 8 characters long";
    } else if (!/[A-Z]/.test(formData.newPassword)) {
      newErrors.newPassword = "Password must contain at least one uppercase letter";
    } else if (!/[a-z]/.test(formData.newPassword)) {
      newErrors.newPassword = "Password must contain at least one lowercase letter";
    } else if (!/\d/.test(formData.newPassword)) {
      newErrors.newPassword = "Password must contain at least one number";
    } else if (
      !/[!@#$%^&*(),.?":{}|<>]/.test(formData.newPassword)
    ) {
      newErrors.newPassword =
        "Password must contain at least one special character";
    }
    if (!formData.confirmPassword) {
      newErrors.confirmPassword = "Please confirm your new password";
    } else if (formData.newPassword !== formData.confirmPassword) {
      newErrors.confirmPassword = "Passwords do not match";
    }
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handlePasswordChange = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validatePasswordForm()) return;
    setIsSubmitting(true);
    setErrors({});
    try {
      const client = ApiClientFactory.getApiClient();
      const command = new ChangePasswordCommand({
        currentPassword: formData.currentPassword,
        newPassword: formData.newPassword,
        confirmPassword: formData.confirmPassword,
      });
      const response: ChangePasswordResponse = await client.changePassword_ChangePassword(
        command
      );
      if (response?.success) {
        if (enable2FA) {
          setPhase("2fa");
          await initialize2FA();
        } else {
          onComplete({ passwordChanged: true });
        }
      } else {
        setErrors({
          submit: response?.message ?? "Failed to change password",
        });
      }
    } catch (err: unknown) {
      const message =
        (err as { response?: { data?: { message?: string }; message?: string }; message?: string })
          ?.response?.data?.message ??
        (err as { message?: string })?.message ??
        "An error occurred while changing password";
      setErrors({ submit: message });
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleSkipPassword = async () => {
    setIsSkipping(true);
    setErrors({});
    try {
      const client = ApiClientFactory.getApiClient();
      const response = await client.changePassword_SkipPasswordChange();
      if (response?.success) {
        if (enable2FA) {
          setPhase("2fa");
          await initialize2FA();
        } else {
          onComplete({ passwordSkipped: true });
        }
      } else {
        setErrors({
          submit: response?.message ?? "Failed to skip password change",
        });
      }
    } catch (err: unknown) {
      const message =
        (err as { response?: { data?: { message?: string }; message?: string }; message?: string })
          ?.response?.data?.message ??
        (err as { message?: string })?.message ??
        "An error occurred while skipping";
      setErrors({ submit: message });
    } finally {
      setIsSkipping(false);
    }
  };

  const initialize2FA = useCallback(async () => {
    setIsGenerating(true);
    setErrors({});
    try {
      const response = await twoFactorAuthService.initialize();
      setQrCode(response.qrCodeUrl);
    } catch (err: unknown) {
      const message =
        (err as { response?: { data?: { message?: string }; message?: string }; message?: string })
          ?.response?.data?.message ??
        (err as { message?: string })?.message ??
        "Failed to setup 2FA";
      const str = String(message);
      if (
        str.includes("modified by another operation") ||
        str.includes("Concurrent modification")
      ) {
        setErrors({
          setup: "The 2FA setup was modified by another operation. Please try again.",
        });
      } else {
        setErrors({ setup: message });
      }
    } finally {
      setIsGenerating(false);
    }
  }, []);

  const handleVerifyCode = useCallback(async () => {
    if (isVerifying) return;
    const code = verificationCode.join("");
    if (code.length !== 6) {
      setErrors({
        verification: "Please enter the complete 6-digit verification code",
      });
      return;
    }
    setIsVerifying(true);
    setErrors({});
    try {
      const response = await twoFactorAuthService.enable(code);
      if (response.success) {
        onComplete({
          twoFactorEnabled: true,
          backupCodes: response.backupCodes ?? [],
        });
      } else {
        setErrors({ verification: response.message ?? "Invalid verification code" });
        setHasAutoSubmitted(false);
      }
    } catch (err: unknown) {
      const message =
        (err as { response?: { data?: { message?: string }; message?: string }; message?: string })
          ?.response?.data?.message ??
        (err as { message?: string })?.message ??
        "An error occurred while verifying the code";
      const str = String(message);
      if (
        str.includes("modified by another operation") ||
        str.includes("Concurrent modification")
      ) {
        setErrors({
          verification:
            "The 2FA setup was modified by another operation. Please try again.",
        });
      } else {
        setErrors({ verification: message });
      }
      setHasAutoSubmitted(false);
    } finally {
      setIsVerifying(false);
    }
  }, [verificationCode, isVerifying, onComplete]);

  useEffect(() => {
    const code = verificationCode.join("");
    if (code.length === 6 && !isVerifying && !hasAutoSubmitted) {
      setHasAutoSubmitted(true);
      const timer = setTimeout(() => {
        if (!isVerifying) handleVerifyCode();
      }, 500);
      return () => clearTimeout(timer);
    }
  }, [verificationCode, isVerifying, hasAutoSubmitted, handleVerifyCode]);

  const handleCodeChange = (index: number, value: string) => {
    if (!/^\d*$/.test(value)) return;
    const newCode = [...verificationCode];
    newCode[index] = value;
    setVerificationCode(newCode);
    if (errors.verification) {
      setErrors((prev) => {
        const next = { ...prev };
        delete next.verification;
        return next;
      });
    }
    if (value && index < 5) inputRefs.current[index + 1]?.focus();
  };

  const handleKeyDown = (index: number, e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Backspace" && !verificationCode[index] && index > 0) {
      inputRefs.current[index - 1]?.focus();
    }
  };

  const handleSkip2FA = () => {
    onComplete({ twoFactorEnabled: false });
  };

  const handleBackToPassword = () => {
    setPhase("password");
    setQrCode("");
    setVerificationCode(["", "", "", "", "", ""]);
    setErrors({});
    setHasAutoSubmitted(false);
  };

  const togglePasswordVisibility = (field: "current" | "new" | "confirm") => {
    setShowPasswords((prev) => ({ ...prev, [field]: !prev[field] }));
  };

  const inputClass =
    "border-white/20 bg-white/5 text-white placeholder:text-white/50";

  if (phase === "2fa") {
    return (
      <div className="space-y-6">
        <div>
          <h3 className="text-lg font-medium text-white mb-2">
            Two-Factor Authentication
          </h3>
          <p className="text-white/70 text-sm">
            Scan the QR code with your authenticator app, then enter the 6-digit
            code.
          </p>
        </div>

        {!qrCode && isGenerating && (
          <div className="flex items-center justify-center gap-2 py-8 text-white/70">
            <div className="size-5 animate-spin rounded-full border-2 border-white/30 border-t-white" />
            Setting up 2FA...
          </div>
        )}

        {qrCode && (
          <>
            <div className="flex flex-col items-center gap-4">
              <div className="bg-white p-4 rounded-lg">
                <QRCode
                  value={qrCode}
                  size={192}
                  level="M"
                  bgColor="#FFFFFF"
                  fgColor="#000000"
                  style={{ width: "100%", height: "auto" }}
                />
              </div>
              <p className="text-white/60 text-sm text-center">
                Use Google Authenticator, Authy, or Microsoft Authenticator.
              </p>
            </div>

            <div className="space-y-4">
              <p className="text-sm text-white/70">
                Enter the 6-digit code from your app:
              </p>
              <div className="flex justify-center gap-2">
                {verificationCode.map((digit, index) => (
                  <Input
                    key={index}
                    ref={(el) => {
                      inputRefs.current[index] = el;
                    }}
                    type="text"
                    inputMode="numeric"
                    maxLength={1}
                    value={digit}
                    onChange={(e) => handleCodeChange(index, e.target.value)}
                    onKeyDown={(e) => handleKeyDown(index, e)}
                    className={`w-12 h-12 text-center text-lg font-semibold ${inputClass}`}
                  />
                ))}
              </div>
              {errors.verification && (
                <p className="text-sm text-red-400">{errors.verification}</p>
              )}
            </div>
          </>
        )}

        {errors.setup && (
          <div className="rounded-md border border-red-500/50 bg-red-500/10 px-3 py-2 text-sm text-red-300">
            {errors.setup}
          </div>
        )}

        <div className="flex justify-between pt-4">
          <Button
            type="button"
            variant="outline"
            onClick={handleBackToPassword}
            className="border-white/20 text-white hover:bg-white/10"
          >
            Back
          </Button>
          <div className="flex gap-3">
            <Button
              type="button"
              variant="outline"
              onClick={handleSkip2FA}
              disabled={isVerifying || isGenerating}
              className="border-white/20 text-white hover:bg-white/10"
            >
              Skip 2FA
            </Button>
            <Button
              type="button"
              onClick={handleVerifyCode}
              disabled={
                isVerifying ||
                isGenerating ||
                verificationCode.join("").length !== 6
              }
            >
              {isVerifying ? (
                <>
                  <div className="size-4 animate-spin rounded-full border-2 border-white/30 border-t-white" />
                  Verifying...
                </>
              ) : (
                "Verify & Continue"
              )}
            </Button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <form onSubmit={handlePasswordChange} className="space-y-6">
      <div>
        <h3 className="text-lg font-medium text-white mb-4">Security</h3>
        <p className="text-white/70 text-sm mb-4">
          Change your default admin password for security. You can optionally
          enable two-factor authentication.
        </p>
      </div>

      {user?.id && setupAvatarPreview && (
        <div className="space-y-3 rounded-md border border-white/15 bg-white/5 p-4">
          <p className="text-sm font-medium text-white">Your avatar</p>
          <p className="text-xs text-white/50">
            Optional: save a default pattern for your account. The stored image
            may differ slightly from the preview.
          </p>
          <div className="flex flex-col gap-3 sm:flex-row sm:items-end">
            <div className="h-16 w-16 shrink-0 overflow-hidden rounded-full border border-white/20">
              <img
                src={setupAvatarPreview}
                alt=""
                className="h-full w-full [image-rendering:pixelated]"
              />
            </div>
            <div className="min-w-0 flex-1">
              <label
                htmlFor="setup-avatar-salt"
                className="mb-1 block text-xs text-white/60"
              >
                Tweak the pattern (optional)
              </label>
              <Input
                id="setup-avatar-salt"
                value={setupAvatarSalt}
                onChange={(e) => setSetupAvatarSalt(e.target.value)}
                maxLength={50}
                className={inputClass}
                disabled={setupAvatarSaving}
              />
            </div>
            <Button
              type="button"
              onClick={handleSaveSetupAvatar}
              disabled={setupAvatarSaving}
              className="h-11 min-w-[8rem] shrink-0"
            >
              {setupAvatarSaving ? "Saving…" : "Save avatar"}
            </Button>
          </div>
        </div>
      )}

      <div className="space-y-4">
        <div>
          <label
            htmlFor="currentPassword"
            className="block text-sm font-medium text-white mb-2"
          >
            Current Password
          </label>
          <div className="relative">
            <Input
              id="currentPassword"
              type={showPasswords.current ? "text" : "password"}
              value={formData.currentPassword}
              onChange={(e) => handleInputChange("currentPassword", e.target.value)}
              placeholder="Enter current password"
              className={`pr-10 ${inputClass} ${errors.currentPassword ? "border-red-400" : ""}`}
            />
            <button
              type="button"
              className="absolute right-3 top-1/2 -translate-y-1/2 text-white/50 hover:text-white"
              onClick={() => togglePasswordVisibility("current")}
            >
              {showPasswords.current ? <EyeOff className="size-4" /> : <Eye className="size-4" />}
            </button>
          </div>
          {errors.currentPassword && (
            <p className="mt-1 text-sm text-red-400">{errors.currentPassword}</p>
          )}
        </div>

        <div>
          <label
            htmlFor="newPassword"
            className="block text-sm font-medium text-white mb-2"
          >
            New Password
          </label>
          <div className="relative">
            <Input
              id="newPassword"
              type={showPasswords.new ? "text" : "password"}
              value={formData.newPassword}
              onChange={(e) => handleInputChange("newPassword", e.target.value)}
              placeholder="Enter new password"
              className={`pr-10 ${inputClass} ${errors.newPassword ? "border-red-400" : ""}`}
            />
            <button
              type="button"
              className="absolute right-3 top-1/2 -translate-y-1/2 text-white/50 hover:text-white"
              onClick={() => togglePasswordVisibility("new")}
            >
              {showPasswords.new ? <EyeOff className="size-4" /> : <Eye className="size-4" />}
            </button>
          </div>
          {formData.newPassword && (
            <PasswordStrengthIndicator password={formData.newPassword} />
          )}
          {errors.newPassword && (
            <p className="mt-1 text-sm text-red-400">{errors.newPassword}</p>
          )}
        </div>

        <div>
          <label
            htmlFor="confirmPassword"
            className="block text-sm font-medium text-white mb-2"
          >
            Confirm New Password
          </label>
          <div className="relative">
            <Input
              id="confirmPassword"
              type={showPasswords.confirm ? "text" : "password"}
              value={formData.confirmPassword}
              onChange={(e) => handleInputChange("confirmPassword", e.target.value)}
              placeholder="Confirm new password"
              className={`pr-10 ${inputClass} ${errors.confirmPassword ? "border-red-400" : ""}`}
            />
            <button
              type="button"
              className="absolute right-3 top-1/2 -translate-y-1/2 text-white/50 hover:text-white"
              onClick={() => togglePasswordVisibility("confirm")}
            >
              {showPasswords.confirm ? (
                <EyeOff className="size-4" />
              ) : (
                <Eye className="size-4" />
              )}
            </button>
          </div>
          {errors.confirmPassword && (
            <p className="mt-1 text-sm text-red-400">{errors.confirmPassword}</p>
          )}
        </div>

        <label className="flex items-center gap-2 cursor-pointer">
          <input
            type="checkbox"
            checked={enable2FA}
            onChange={(e) => setEnable2FA(e.target.checked)}
            className="rounded border-white/30 bg-white/5 text-blue-500 focus:ring-blue-500"
          />
          <span className="text-sm text-white/80">Enable two-factor authentication</span>
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
          disabled={isFirstStep}
          className="border-white/20 text-white hover:bg-white/10 disabled:opacity-50"
        >
          Back
        </Button>
        <div className="flex gap-3">
          <Button
            type="button"
            variant="outline"
            onClick={handleSkipPassword}
            disabled={isSubmitting || isSkipping}
            className="border-white/20 text-white hover:bg-white/10"
          >
            {isSkipping ? "Skipping..." : "Skip"}
          </Button>
          <Button
            type="submit"
            disabled={
              isSubmitting || isSkipping || !isPasswordFormValid()
            }
          >
            {isSubmitting ? (
              <>
                <div className="size-4 animate-spin rounded-full border-2 border-white/30 border-t-white" />
                Changing...
              </>
            ) : (
              "Change Password & Continue"
            )}
          </Button>
        </div>
      </div>
    </form>
  );
}
