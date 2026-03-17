"use client";

import { useEffect, useState } from "react";
import { toast } from "sonner";
import { useAuth } from "@/contexts/AuthContext";
import { fetchUserProfile } from "@/lib/services/userProfileService";
import type { UserProfileData } from "@/lib/services/userProfileService";
import { ApiClientFactory } from "@/lib/api/clientFactory";
import { UpdateUserProfileRequest } from "@/lib/api/client";
import { SettingsSection } from "@/components/settings/settings-section";
import { SettingsToggle } from "@/components/settings/settings-toggle";

export default function PrivacySettingsPage() {
  const { user } = useAuth();
  const [profile, setProfile] = useState<UserProfileData | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!user?.id) return;
    setLoading(true);
    fetchUserProfile(user.id)
      .then(setProfile)
      .catch((err) => {
        console.error("Failed to load profile:", err);
        toast.error("Failed to load profile");
      })
      .finally(() => setLoading(false));
  }, [user?.id]);

  const updatePrivacy = async (
    field: keyof Pick<
      UserProfileData,
      "isProfilePublic" | "showEmail" | "showLastLogin" | "showStatistics"
    >,
    value: boolean
  ) => {
    if (!profile) return;
    const previous = profile[field];
    setProfile((prev) => (prev ? { ...prev, [field]: value } : prev));
    try {
      const request = new UpdateUserProfileRequest({
        bio: profile.bio,
        country: profile.country,
        websiteUrl: profile.websiteUrl,
        twitterUrl: profile.twitterUrl,
        instagramUrl: profile.instagramUrl,
        youTubeUrl: profile.youtubeUrl,
        isProfilePublic: field === "isProfilePublic" ? value : profile.isProfilePublic,
        showEmail: field === "showEmail" ? value : profile.showEmail,
        showLastLogin: field === "showLastLogin" ? value : profile.showLastLogin,
        showStatistics: field === "showStatistics" ? value : profile.showStatistics,
      });
      await ApiClientFactory.getApiClient().updateUserProfile_UpdateUserProfile(request);
      toast.success("Privacy settings updated");
    } catch (err) {
      console.error("Failed to update privacy:", err);
      setProfile((prev) => (prev ? { ...prev, [field]: previous } : prev));
      toast.error("Failed to update privacy settings");
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
    <SettingsSection
      title="Privacy"
      description="Control who can see your profile information"
    >
      <SettingsToggle
        label="Public Profile"
        description="Allow others to view your profile"
        checked={profile.isProfilePublic ?? true}
        onCheckedChange={(checked) => updatePrivacy("isProfilePublic", checked)}
      />
      <SettingsToggle
        label="Show Email"
        description="Display your email on your profile"
        checked={profile.showEmail ?? false}
        onCheckedChange={(checked) => updatePrivacy("showEmail", checked)}
      />
      <SettingsToggle
        label="Show Last Login"
        description="Display when you last logged in"
        checked={profile.showLastLogin ?? false}
        onCheckedChange={(checked) => updatePrivacy("showLastLogin", checked)}
      />
      <SettingsToggle
        label="Show Statistics"
        description="Display your model and collection statistics"
        checked={profile.showStatistics ?? true}
        onCheckedChange={(checked) => updatePrivacy("showStatistics", checked)}
      />
    </SettingsSection>
  );
}
