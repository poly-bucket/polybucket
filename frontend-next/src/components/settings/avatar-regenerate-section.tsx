"use client";

import { type ChangeEvent, useCallback, useMemo, useState } from "react";
import { Input } from "@/components/primitives/input";
import { Button } from "@/components/primitives/button";
import { svgToDataUrl, previewUserAvatarSvg } from "@/lib/avatar/minidenticon";
import { regenerateUserAvatar } from "@/lib/services/avatarService";
import { useAuth } from "@/contexts/AuthContext";
import { toast } from "sonner";
import { cn } from "@/lib/utils";
import { Lock, LockOpen, RefreshCw } from "lucide-react";

interface AvatarRegenerateSectionProps {
  onSaved?: () => void | Promise<void>;
}

export function AvatarRegenerateSection({ onSaved }: AvatarRegenerateSectionProps) {
  const { user, refreshUserFromMe } = useAuth();
  const [mode, setMode] = useState<"generate" | "upload">("generate");
  const [salt, setSalt] = useState("");
  const [isSaltLocked, setIsSaltLocked] = useState(false);
  const [lockedSalt, setLockedSalt] = useState("");
  const [uploadedAvatar, setUploadedAvatar] = useState("");
  const [saving, setSaving] = useState(false);
  const userId = user?.id ?? "";
  const effectiveSalt = mode === "generate" && isSaltLocked ? lockedSalt : salt;

  const previewSrc = useMemo(
    () => svgToDataUrl(previewUserAvatarSvg(userId, effectiveSalt || undefined)),
    [userId, effectiveSalt]
  );
  const finalPreviewSrc = mode === "upload" ? uploadedAvatar : previewSrc;

  const randomSalt = useCallback(() => {
    const chars =
      "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    let result = "";
    for (let i = 0; i < 8; i++) {
      result += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    return result;
  }, []);

  const handleRefreshPattern = useCallback(() => {
    if (isSaltLocked) return;
    setMode("generate");
    setSalt(randomSalt());
  }, [isSaltLocked, randomSalt]);

  const toggleSaltLock = useCallback(() => {
    if (mode !== "generate") return;
    if (isSaltLocked) {
      setIsSaltLocked(false);
      setLockedSalt("");
      return;
    }
    setLockedSalt(salt);
    setIsSaltLocked(true);
  }, [mode, isSaltLocked, salt]);

  const handleUploadChange = useCallback((e: ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = () => {
      const result = reader.result;
      if (typeof result !== "string") {
        toast.error("Could not read selected image");
        return;
      }
      setUploadedAvatar(result);
      setMode("upload");
    };
    reader.onerror = () => {
      toast.error("Could not read selected image");
    };
    reader.readAsDataURL(file);
    e.target.value = "";
  }, []);

  if (!userId) return null;

  const handleSave = async () => {
    setSaving(true);
    try {
      await regenerateUserAvatar(
        mode === "generate" ? salt.trim() || undefined : undefined,
        mode === "upload" ? uploadedAvatar : undefined
      );
      await refreshUserFromMe();
      await onSaved?.();
      toast.success("Avatar updated");
    } catch (e) {
      console.error(e);
      toast.error("Failed to update avatar");
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="min-h-[48px] space-y-3 rounded-md border border-white/15 bg-white/5 p-4">
      <p className="text-sm text-white/80">Avatar</p>
      <p className="text-xs text-white/50">
        Choose a generated pattern or upload your own image.
      </p>
      <div className="rounded-md border border-white/15 bg-white/5 p-3">
        <div className="flex items-center gap-3">
          <div className="h-20 w-20 shrink-0 overflow-hidden rounded-full border-2 border-white/20 bg-white/10">
            {finalPreviewSrc ? (
              <img
                src={finalPreviewSrc}
                alt="Avatar preview"
                className={cn(
                  "h-full w-full",
                  mode === "generate" ? "[image-rendering:pixelated]" : "object-cover"
                )}
              />
            ) : (
              <div className="flex h-full w-full items-center justify-center text-xs text-white/40">
                No image
              </div>
            )}
          </div>
          <div>
            <p className="text-sm font-medium text-white">Final avatar preview</p>
            <p className="text-xs text-white/50">
              This is the avatar that will be saved to your account.
            </p>
          </div>
        </div>
      </div>
      <div className="grid gap-3 md:grid-cols-2">
        <div
          role="button"
          tabIndex={0}
          onClick={() => setMode("generate")}
          onKeyDown={(e) => {
            if (e.key === "Enter" || e.key === " ") {
              e.preventDefault();
              setMode("generate");
            }
          }}
          className={cn(
            "space-y-3 rounded-md border p-3 transition-colors",
            mode === "generate"
              ? "border-primary/80 bg-primary/10"
              : "border-white/15 bg-white/5 hover:border-white/30"
          )}
        >
          <div className="flex items-start justify-between gap-3">
            <div>
              <p className="text-sm font-medium text-white">Generated</p>
              <p className="text-xs text-white/50">
                Use your account pattern and tweak the seed.
              </p>
            </div>
            <div className="flex items-center gap-2">
              <Button
                type="button"
                variant="outline"
                size="icon-sm"
                aria-label={isSaltLocked ? "Unlock avatar seed" : "Lock avatar seed"}
                onClick={(e) => {
                  e.stopPropagation();
                  setMode("generate");
                  toggleSaltLock();
                }}
                disabled={saving}
              >
                {isSaltLocked ? (
                  <Lock className="h-4 w-4" />
                ) : (
                  <LockOpen className="h-4 w-4" />
                )}
              </Button>
              <Button
                type="button"
                variant="outline"
                size="icon-sm"
                aria-label="New avatar pattern"
                onClick={(e) => {
                  e.stopPropagation();
                  handleRefreshPattern();
                }}
                disabled={saving || isSaltLocked}
              >
                <RefreshCw className="h-4 w-4" />
              </Button>
            </div>
          </div>
          <label
            htmlFor="avatar-pattern-salt"
            className="block text-xs text-white/60"
          >
            Tweak the pattern (optional)
          </label>
          <Input
            id="avatar-pattern-salt"
            value={salt}
            onChange={(e) => {
              setMode("generate");
              setSalt(e.target.value);
            }}
            maxLength={50}
            placeholder="word or phrase"
            className="border-white/20 bg-white/5 text-white"
            disabled={saving}
          />
          <p className="text-xs text-white/50">
            {isSaltLocked
              ? "Pattern seed is locked."
              : "Use refresh for a random seed."}
          </p>
        </div>
        <div
          role="button"
          tabIndex={0}
          onClick={() => setMode("upload")}
          onKeyDown={(e) => {
            if (e.key === "Enter" || e.key === " ") {
              e.preventDefault();
              setMode("upload");
            }
          }}
          className={cn(
            "space-y-3 rounded-md border p-3 transition-colors",
            mode === "upload"
              ? "border-primary/80 bg-primary/10"
              : "border-white/15 bg-white/5 hover:border-white/30"
          )}
        >
          <div>
            <p className="text-sm font-medium text-white">Upload Image</p>
            <p className="text-xs text-white/50">
              Upload a custom avatar image.
            </p>
          </div>
          <label className="inline-flex cursor-pointer items-center rounded-md border border-white/20 bg-white/5 px-3 py-2 text-xs font-medium text-white transition-colors hover:border-white/40">
            Choose image
            <input
              type="file"
              accept="image/*,.svg"
              className="sr-only"
              onChange={handleUploadChange}
              disabled={saving}
            />
          </label>
        </div>
      </div>
      <div className="flex justify-end">
        <Button
          type="button"
          onClick={handleSave}
          disabled={saving || (mode === "upload" && !uploadedAvatar)}
          className="h-11 min-w-[10rem] shrink-0"
        >
          {saving ? "Saving…" : "Save avatar"}
        </Button>
      </div>
    </div>
  );
}
