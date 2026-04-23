"use client";

import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { toast } from "sonner";
import { useAuth } from "@/contexts/AuthContext";
import { fetchUserProfile } from "@/lib/services/userProfileService";
import type { UserProfileData } from "@/lib/services/userProfileService";
import { ApiClientFactory } from "@/lib/api/clientFactory";
import { UpdateUserProfileRequest } from "@/lib/api/client";
import { UserAvatar } from "@/components/layout/user-avatar";
import { AvatarRegenerateSection } from "@/components/settings/avatar-regenerate-section";
import { SettingsSection } from "@/components/settings/settings-section";
import { SettingsField } from "@/components/settings/settings-field";
import { SettingsFooter } from "@/components/settings/settings-footer";
import { Button } from "@/components/primitives/button";
import { Input } from "@/components/primitives/input";
import { useUnsavedChanges } from "@/hooks/use-unsaved-changes";

const profileSchema = z.object({
  bio: z.string().max(500).optional().or(z.literal("")),
  country: z.string().max(100).optional().or(z.literal("")),
  websiteUrl: z.string().url().optional().or(z.literal("")),
  twitterUrl: z.string().url().optional().or(z.literal("")),
  instagramUrl: z.string().url().optional().or(z.literal("")),
  youtubeUrl: z.string().url().optional().or(z.literal("")),
});

type ProfileFormData = z.infer<typeof profileSchema>;

export default function ProfileSettingsPage() {
  const { user } = useAuth();
  const [profile, setProfile] = useState<UserProfileData | null>(null);
  const [loading, setLoading] = useState(true);

  const {
    register,
    handleSubmit,
    reset,
    formState: { isDirty, isSubmitting },
  } = useForm<ProfileFormData>({
    resolver: zodResolver(profileSchema),
    defaultValues: {
      bio: "",
      country: "",
      websiteUrl: "",
      twitterUrl: "",
      instagramUrl: "",
      youtubeUrl: "",
    },
  });

  useUnsavedChanges(isDirty);

  useEffect(() => {
    if (!user?.id) return;
    setLoading(true);
    fetchUserProfile(user.id)
      .then((data) => {
        setProfile(data);
        reset({
          bio: data.bio ?? "",
          country: data.country ?? "",
          websiteUrl: data.websiteUrl ?? "",
          twitterUrl: data.twitterUrl ?? "",
          instagramUrl: data.instagramUrl ?? "",
          youtubeUrl: data.youtubeUrl ?? "",
        });
      })
      .catch((err) => {
        console.error("Failed to load profile:", err);
        toast.error("Failed to load profile");
      })
      .finally(() => setLoading(false));
  }, [user?.id, reset]);

  const onSubmit = async (data: ProfileFormData) => {
    if (!user?.id) return;
    try {
      const request = new UpdateUserProfileRequest({
        bio: data.bio || undefined,
        country: data.country || undefined,
        websiteUrl: data.websiteUrl || undefined,
        twitterUrl: data.twitterUrl || undefined,
        instagramUrl: data.instagramUrl || undefined,
        youTubeUrl: data.youtubeUrl || undefined,
      });
      await ApiClientFactory.getApiClient().updateUserProfile_UpdateUserProfile(request);
      setProfile((prev) =>
        prev ? { ...prev, ...data, websiteUrl: data.websiteUrl, twitterUrl: data.twitterUrl, instagramUrl: data.instagramUrl, youtubeUrl: data.youtubeUrl } : prev
      );
      reset(data);
      toast.success("Profile updated successfully");
    } catch (err) {
      console.error("Failed to update profile:", err);
      toast.error("Failed to update profile");
    }
  };

  if (loading) {
    return (
      <div className="flex min-h-48 items-center justify-center">
        <div className="h-10 w-10 animate-spin rounded-full border-2 border-white/30 border-t-white" />
      </div>
    );
  }

  if (!profile) {
    return (
      <div className="text-center text-white/70">
        Failed to load profile. Please try again.
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <SettingsSection title="Basic Information" description="Your public profile details">
        <div className="space-y-4 py-4">
          <div className="flex items-center gap-4">
            <UserAvatar
              userId={profile.id ?? user?.id ?? ""}
              username={profile.username ?? user?.username ?? ""}
              profilePictureUrl={profile.profilePictureUrl}
              avatar={profile.avatar}
              size="lg"
              className="h-20 w-20"
            />
            <div>
              <p className="text-sm font-medium text-white">Avatar</p>
              <p className="text-xs text-white/60">
                Regenerate a unique pattern for your public profile.
              </p>
            </div>
          </div>
          <AvatarRegenerateSection
            onSaved={async () => {
              if (!user?.id) return;
              const data = await fetchUserProfile(user.id);
              setProfile(data);
            }}
          />
        </div>
        <div className="space-y-1 py-2">
          <p className="text-sm text-white/60">Username</p>
          <p className="text-white">@{profile.username}</p>
        </div>
        <div className="space-y-1 py-2">
          <p className="text-sm text-white/60">Email</p>
          <p className="text-white">{user?.email ?? "—"}</p>
        </div>
        <SettingsField label="Bio" description="A short description about yourself">
          <Input
            {...register("bio")}
            variant="glass"
            placeholder="Tell us about yourself..."
            className="max-w-md"
            maxLength={500}
          />
        </SettingsField>
        <SettingsField label="Country">
          <Input
            {...register("country")}
            variant="glass"
            placeholder="Your country"
            className="max-w-md"
            maxLength={100}
          />
        </SettingsField>
        <SettingsField label="Website" description="Your personal website">
          <Input
            {...register("websiteUrl")}
            variant="glass"
            type="url"
            placeholder="https://example.com"
            className="max-w-md"
          />
        </SettingsField>
        <SettingsField label="Twitter" description="Your Twitter profile URL">
          <Input
            {...register("twitterUrl")}
            variant="glass"
            type="url"
            placeholder="https://twitter.com/username"
            className="max-w-md"
          />
        </SettingsField>
        <SettingsField label="Instagram" description="Your Instagram profile URL">
          <Input
            {...register("instagramUrl")}
            variant="glass"
            type="url"
            placeholder="https://instagram.com/username"
            className="max-w-md"
          />
        </SettingsField>
        <SettingsField label="YouTube" description="Your YouTube channel URL">
          <Input
            {...register("youtubeUrl")}
            variant="glass"
            type="url"
            placeholder="https://youtube.com/username"
            className="max-w-md"
          />
        </SettingsField>
        <SettingsFooter
          onSave={() => {}}
          onCancel={() => reset()}
          isSaving={isSubmitting}
          isDirty={isDirty}
          submit
        />
      </SettingsSection>
    </form>
  );
}
