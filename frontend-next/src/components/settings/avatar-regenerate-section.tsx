"use client";

import { useMemo, useState } from "react";
import { Input } from "@/components/primitives/input";
import { Button } from "@/components/primitives/button";
import { svgToDataUrl, previewUserAvatarSvg } from "@/lib/avatar/minidenticon";
import { regenerateUserAvatar } from "@/lib/services/avatarService";
import { useAuth } from "@/contexts/AuthContext";
import { toast } from "sonner";

interface AvatarRegenerateSectionProps {
  onSaved?: () => void | Promise<void>;
}

export function AvatarRegenerateSection({ onSaved }: AvatarRegenerateSectionProps) {
  const { user, refreshUserFromMe } = useAuth();
  const [salt, setSalt] = useState("");
  const [saving, setSaving] = useState(false);
  const userId = user?.id ?? "";

  const previewSrc = useMemo(
    () => svgToDataUrl(previewUserAvatarSvg(userId, salt || undefined)),
    [userId, salt]
  );

  if (!userId) return null;

  const handleSave = async () => {
    setSaving(true);
    try {
      await regenerateUserAvatar(salt.trim() || undefined);
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
        Preview uses your account pattern. The saved image uses the server
        style and may look slightly different.
      </p>
      <div className="flex flex-col gap-4 sm:flex-row sm:items-end">
        <div className="h-20 w-20 shrink-0 overflow-hidden rounded-full border-2 border-white/20 bg-white/10">
          <img
            src={previewSrc}
            alt="Avatar preview"
            className="h-full w-full [image-rendering:pixelated]"
          />
        </div>
        <div className="min-w-0 flex-1 space-y-2">
          <label
            htmlFor="avatar-pattern-salt"
            className="text-xs text-white/60"
          >
            Tweak the pattern (optional)
          </label>
          <Input
            id="avatar-pattern-salt"
            value={salt}
            onChange={(e) => setSalt(e.target.value)}
            maxLength={50}
            placeholder="word or phrase"
            className="border-white/20 bg-white/5 text-white"
            disabled={saving}
          />
        </div>
        <Button
          type="button"
          onClick={handleSave}
          disabled={saving}
          className="h-11 min-w-[10rem] shrink-0"
        >
          {saving ? "Saving…" : "Save avatar"}
        </Button>
      </div>
    </div>
  );
}
